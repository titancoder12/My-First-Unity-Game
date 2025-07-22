using UnityEngine;
using Unity.Netcode;

public class EditorMultiplayerLauncher : MonoBehaviour
{
    public bool autoStartInEditor = true;

    void Start()
    {
#if UNITY_EDITOR
        if (!Application.isBatchMode && autoStartInEditor)
        {
            // Start Host
            NetworkManager.Singleton.StartHost();

            // Simulate a second client by manually calling connection logic
            NetworkManager.Singleton.OnClientConnectedCallback += SimulateFakeClient;
        }
#endif
    }

    private void SimulateFakeClient(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"Fake Client connected: {clientId}");
        }
    }
}
