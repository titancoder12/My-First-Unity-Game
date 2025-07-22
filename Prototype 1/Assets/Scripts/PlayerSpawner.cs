using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnHeight = 2f;
    [SerializeField] private float spawnRadius = 10f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Spawning player for client {clientId}");

        Vector3 spawnPos = GetSpawnPosition(clientId);
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        // If we have predefined spawn points, use them
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int spawnIndex = (int)(clientId % (ulong)spawnPoints.Length);
            if (spawnPoints[spawnIndex] != null)
            {
                return spawnPoints[spawnIndex].position;
            }
        }
        
        // Otherwise, generate a random spawn position
        Vector3 basePosition = Vector3.zero;
        
        // Try to find a good spawn position with raycast
        for (int attempts = 0; attempts < 10; attempts++)
        {
            // Generate random position in a circle
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 testPosition = basePosition + new Vector3(randomCircle.x, spawnHeight + 5f, randomCircle.y);
            
            // Raycast down to find ground
            if (Physics.Raycast(testPosition, Vector3.down, out RaycastHit hit, spawnHeight + 10f))
            {
                return hit.point + Vector3.up * spawnHeight;
            }
        }
        
        // Fallback: use simple spaced-out positions with proper height
        return new Vector3(clientId * 3f, spawnHeight, 0f);
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
}
