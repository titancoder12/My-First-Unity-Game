using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    
    [Header("Respawn Settings")]
    [SerializeField] private Transform[] respawnPoints;
    [SerializeField] private float respawnHeight = 2f;
    [SerializeField] private float respawnRadius = 15f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip hitSoundClip;
    [SerializeField] private AudioClip deathSoundClip;
    [SerializeField] private AudioSource audioSource;
    
    // Network variables for health synchronization
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    // Events
    public System.Action<float, float> OnHealthChanged; // currentHealth, maxHealth
    public System.Action OnPlayerDied;
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            isDead.Value = false;
        }
        
        // Subscribe to health changes
        currentHealth.OnValueChanged += OnHealthValueChanged;
        isDead.OnValueChanged += OnDeathStateChanged;
        
        // Initialize audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }
    
    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthValueChanged;
        isDead.OnValueChanged -= OnDeathStateChanged;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, ulong attackerId)
    {
        if (!IsServer || isDead.Value) return;
        
        currentHealth.Value = Mathf.Max(0, currentHealth.Value - damage);
        
        // Play hit sound effect
        PlayHitEffectClientRpc();
        
        if (currentHealth.Value <= 0 && !isDead.Value)
        {
            isDead.Value = true;
            HandlePlayerDeathServerRpc(attackerId);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void HandlePlayerDeathServerRpc(ulong killerId)
    {
        if (!IsServer) return;
        
        // Play death sound effect
        PlayDeathEffectClientRpc();
        
        // Log the kill
        string killerName = "Unknown";
        string victimName = transform.name;
        
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(killerId))
        {
            var killerObject = NetworkManager.Singleton.ConnectedClients[killerId].PlayerObject;
            if (killerObject != null)
            {
                killerName = killerObject.name;
            }
        }
        
        Debug.Log($"{killerName} eliminated {victimName}!");
        
        // Respawn after delay
        Invoke(nameof(RespawnPlayer), 3f);
    }
    
    [ClientRpc]
    private void PlayHitEffectClientRpc()
    {
        if (hitSoundClip && audioSource)
        {
            audioSource.PlayOneShot(hitSoundClip);
        }
    }
    
    [ClientRpc]
    private void PlayDeathEffectClientRpc()
    {
        if (deathSoundClip && audioSource)
        {
            audioSource.PlayOneShot(deathSoundClip);
        }
    }
    
    private void RespawnPlayer()
    {
        if (!IsServer) return;
        
        // Reset health
        currentHealth.Value = maxHealth;
        isDead.Value = false;
        
        // Find a respawn point
        Vector3 respawnPosition = GetRespawnPosition();
        
        // Use teleportation instead of direct position change for better network sync
        if (TryGetComponent<CharacterController>(out CharacterController controller))
        {
            controller.enabled = false;
            transform.position = respawnPosition;
            controller.enabled = true;
        }
        else
        {
            transform.position = respawnPosition;
        }
        
        // Reset velocity if using Rigidbody
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        Debug.Log($"{transform.name} respawned at {respawnPosition}!");
    }
    
    private Vector3 GetRespawnPosition()
    {
        // First, try to use predefined respawn points
        if (respawnPoints != null && respawnPoints.Length > 0)
        {
            // Filter out null respawn points and find available ones
            var availableSpawnPoints = new System.Collections.Generic.List<Transform>();
            
            foreach (var spawnPoint in respawnPoints)
            {
                if (spawnPoint != null && IsSpawnPointSafe(spawnPoint.position))
                {
                    availableSpawnPoints.Add(spawnPoint);
                }
            }
            
            if (availableSpawnPoints.Count > 0)
            {
                Transform chosenSpawn = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
                return chosenSpawn.position;
            }
        }
        
        // If no predefined spawn points, try to find PlayerSpawner spawn points
        PlayerSpawner spawner = FindObjectOfType<PlayerSpawner>();
        if (spawner != null)
        {
            // Use the spawner's logic for consistent spawn positions
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            return GetFallbackSpawnPosition(clientId);
        }
        
        // Last resort: try to find a safe random position
        return FindSafeRandomPosition();
    }
    
    private bool IsSpawnPointSafe(Vector3 position)
    {
        // Check if there's ground below and no players nearby
        if (!Physics.Raycast(position, Vector3.down, respawnHeight + 2f))
        {
            return false; // No ground below
        }
        
        // Check if any other players are too close
        Collider[] nearbyPlayers = Physics.OverlapSphere(position, 3f);
        foreach (var collider in nearbyPlayers)
        {
            if (collider.CompareTag("Player") && collider.gameObject != this.gameObject)
            {
                return false; // Another player is too close
            }
        }
        
        return true;
    }
    
    private Vector3 FindSafeRandomPosition()
    {
        // Try multiple random positions
        for (int attempts = 0; attempts < 20; attempts++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * respawnRadius;
            Vector3 testPosition = new Vector3(randomCircle.x, respawnHeight + 10f, randomCircle.y);
            
            // Raycast down to find ground
            if (Physics.Raycast(testPosition, Vector3.down, out RaycastHit hit, respawnHeight + 15f))
            {
                Vector3 groundPosition = hit.point + Vector3.up * respawnHeight;
                
                if (IsSpawnPointSafe(groundPosition))
                {
                    return groundPosition;
                }
            }
        }
        
        // Ultimate fallback positions (elevated to avoid terrain issues)
        Vector3[] fallbackPositions = {
            new Vector3(0, 5, 0),
            new Vector3(10, 5, 0),
            new Vector3(-10, 5, 0),
            new Vector3(0, 5, 10),
            new Vector3(0, 5, -10)
        };
        
        return fallbackPositions[Random.Range(0, fallbackPositions.Length)];
    }
    
    private Vector3 GetFallbackSpawnPosition(ulong clientId)
    {
        // Similar logic to PlayerSpawner but for respawning
        return new Vector3(clientId * 4f, respawnHeight, Random.Range(-5f, 5f));
    }
    
    private void OnHealthValueChanged(float oldValue, float newValue)
    {
        OnHealthChanged?.Invoke(newValue, maxHealth);
    }
    
    private void OnDeathStateChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            OnPlayerDied?.Invoke();
        }
    }
    
    // Public getters
    public float GetCurrentHealth() => currentHealth.Value;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead.Value;
    public float GetHealthPercentage() => currentHealth.Value / maxHealth;
}
