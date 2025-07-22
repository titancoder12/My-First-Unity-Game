using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkLauncher : MonoBehaviour
{
    private UnityTransport transport;
    [SerializeField] private ushort port = 7002;
    [SerializeField] private string serverAddress = "127.0.0.1";
    private string statusMessage = "";
    
    // UI State
    private bool showMainMenu = true;
    private bool showServerList = false;
    private string customServerIP = "127.0.0.1";
    private string customServerPort = "7002";
    
    // UI Styling
    private GUIStyle buttonStyle;
    private GUIStyle labelStyle;
    private GUIStyle textFieldStyle;
    private GUIStyle titleStyle;
    private bool stylesInitialized = false;

    private void Awake()
    {
        // Check if NetworkManager exists before accessing it
        if (NetworkManager.Singleton != null)
        {
            transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            
            // Subscribe to network events for better error handling
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        else
        {
            Debug.LogError("NetworkManager not found in scene! Make sure you have a NetworkManager GameObject in your scene.");
            statusMessage = "NetworkManager not found in scene!";
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        statusMessage = $"Client {clientId} connected";
        Debug.Log(statusMessage);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        statusMessage = $"Client {clientId} disconnected";
        Debug.Log(statusMessage);
    }

    void OnGUI()
    {
        InitializeStyles();
        
        // Check if NetworkManager exists before using it
        if (NetworkManager.Singleton == null)
        {
            DrawErrorScreen("NetworkManager not found in scene!");
            return;
        }

        if (!NetworkManager.Singleton.IsListening)
        {
            if (showMainMenu)
            {
                DrawMainMenu();
            }
            else if (showServerList)
            {
                DrawServerBrowser();
            }
        }
        else
        {
            DrawGameSession();
        }
    }

    private void InitializeStyles()
    {
        if (stylesInitialized) return;

        // Professional button style
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 16;
        buttonStyle.padding = new RectOffset(20, 20, 10, 10);
        buttonStyle.margin = new RectOffset(5, 5, 5, 5);

        // Label style
        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 14;
        labelStyle.normal.textColor = Color.white;

        // Text field style
        textFieldStyle = new GUIStyle(GUI.skin.textField);
        textFieldStyle.fontSize = 14;
        textFieldStyle.padding = new RectOffset(10, 10, 5, 5);

        // Title style
        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 24;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        stylesInitialized = true;
    }

    private void DrawErrorScreen(string error)
    {
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        
        GUI.Label(new Rect(centerX - 200, centerY - 50, 400, 100), error, titleStyle);
    }

    private void DrawMainMenu()
    {
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        float panelWidth = 400f;
        float panelHeight = 500f;
        
        // Background panel
        GUI.Box(new Rect(centerX - panelWidth/2, centerY - panelHeight/2, panelWidth, panelHeight), "");
        
        float startY = centerY - panelHeight/2 + 30;
        float buttonWidth = 300f;
        float buttonHeight = 50f;
        float spacing = 20f;
        float currentY = startY;
        
        // Title
        GUI.Label(new Rect(centerX - panelWidth/2, currentY, panelWidth, 50), "Sphere Fighter", titleStyle);
        currentY += 80;
        
        // Host Game Button
        if (GUI.Button(new Rect(centerX - buttonWidth/2, currentY, buttonWidth, buttonHeight), "Host Game", buttonStyle))
        {
            StartHost();
        }
        currentY += buttonHeight + spacing;
        
        // Join Game Button
        if (GUI.Button(new Rect(centerX - buttonWidth/2, currentY, buttonWidth, buttonHeight), "Join Game", buttonStyle))
        {
            showMainMenu = false;
            showServerList = true;
        }
        currentY += buttonHeight + spacing;
        
        // Server Settings
        GUI.Label(new Rect(centerX - buttonWidth/2, currentY, 100, 30), "Host Port:", labelStyle);
        string newPort = GUI.TextField(new Rect(centerX - buttonWidth/2 + 100, currentY, 100, 30), port.ToString(), textFieldStyle);
        if (ushort.TryParse(newPort, out ushort parsedPort))
        {
            port = parsedPort;
        }
        currentY += 50;
        
        // Status message
        if (!string.IsNullOrEmpty(statusMessage))
        {
            GUI.Label(new Rect(centerX - panelWidth/2 + 20, currentY, panelWidth - 40, 60), "Status: " + statusMessage, labelStyle);
        }
        
        // Exit button
        if (GUI.Button(new Rect(centerX - 100, centerY + panelHeight/2 - 80, 200, 40), "Exit Game", buttonStyle))
        {
            Application.Quit();
        }
    }

    private void DrawServerBrowser()
    {
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        float panelWidth = 500f;
        float panelHeight = 400f;
        
        // Background panel
        GUI.Box(new Rect(centerX - panelWidth/2, centerY - panelHeight/2, panelWidth, panelHeight), "");
        
        float startY = centerY - panelHeight/2 + 30;
        float currentY = startY;
        
        // Title
        GUI.Label(new Rect(centerX - panelWidth/2, currentY, panelWidth, 50), "Join Server", titleStyle);
        currentY += 60;
        
        // Quick Join Localhost
        if (GUI.Button(new Rect(centerX - 200, currentY, 400, 40), "Quick Join (Localhost)", buttonStyle))
        {
            customServerIP = "127.0.0.1";
            customServerPort = "7002";
            ConnectToServer(customServerIP, ushort.Parse(customServerPort));
        }
        currentY += 60;
        
        // Custom Server Section
        GUI.Label(new Rect(centerX - 200, currentY, 400, 30), "Connect to Custom Server:", labelStyle);
        currentY += 35;
        
        // Server IP input
        GUI.Label(new Rect(centerX - 200, currentY, 80, 30), "Server IP:", labelStyle);
        customServerIP = GUI.TextField(new Rect(centerX - 110, currentY, 200, 30), customServerIP, textFieldStyle);
        currentY += 40;
        
        // Port input
        GUI.Label(new Rect(centerX - 200, currentY, 50, 30), "Port:", labelStyle);
        customServerPort = GUI.TextField(new Rect(centerX - 140, currentY, 100, 30), customServerPort, textFieldStyle);
        currentY += 50;
        
        // Connect button
        if (GUI.Button(new Rect(centerX - 100, currentY, 200, 40), "Connect", buttonStyle))
        {
            if (ushort.TryParse(customServerPort, out ushort serverPort))
            {
                ConnectToServer(customServerIP, serverPort);
            }
            else
            {
                statusMessage = "Invalid port number!";
            }
        }
        currentY += 60;
        
        // Back button
        if (GUI.Button(new Rect(centerX - 100, currentY, 200, 40), "Back", buttonStyle))
        {
            showMainMenu = true;
            showServerList = false;
        }
        
        // Status message
        if (!string.IsNullOrEmpty(statusMessage))
        {
            GUI.Label(new Rect(centerX - panelWidth/2 + 20, centerY + panelHeight/2 - 60, panelWidth - 40, 40), statusMessage, labelStyle);
        }
    }

    private void DrawGameSession()
    {
        float panelWidth = 300f;
        float panelHeight = 150f;
        
        // Game session info panel (top-right corner)
        GUI.Box(new Rect(Screen.width - panelWidth - 20, 20, panelWidth, panelHeight), "");
        
        float startY = 40;
        float currentY = startY;
        
        GUI.Label(new Rect(Screen.width - panelWidth, currentY, panelWidth - 20, 30), $"Mode: {GetNetworkMode()}", labelStyle);
        currentY += 25;
        
        GUI.Label(new Rect(Screen.width - panelWidth, currentY, panelWidth - 20, 30), $"Players: {NetworkManager.Singleton.ConnectedClients.Count}", labelStyle);
        currentY += 25;
        
        GUI.Label(new Rect(Screen.width - panelWidth, currentY, panelWidth - 20, 30), $"Port: {port}", labelStyle);
        currentY += 35;
        
        // Disconnect button
        if (GUI.Button(new Rect(Screen.width - panelWidth + 10, currentY, panelWidth - 40, 35), "Disconnect", buttonStyle))
        {
            NetworkManager.Singleton.Shutdown();
            statusMessage = "Disconnected from server";
            showMainMenu = true;
            showServerList = false;
        }
    }

    private void StartHost()
    {
        try
        {
            transport.SetConnectionData("127.0.0.1", port);
            bool success = NetworkManager.Singleton.StartHost();
            statusMessage = success ? $"Host started on port {port}" : "Failed to start host";
        }
        catch (System.Exception e)
        {
            statusMessage = $"Host start failed: {e.Message}";
            Debug.LogError(statusMessage);
        }
    }

    private void StartClient()
    {
        ConnectToServer("127.0.0.1", port);
    }

    private void ConnectToServer(string serverIP, ushort serverPort)
    {
        try
        {
            transport.SetConnectionData(serverIP, serverPort);
            bool success = NetworkManager.Singleton.StartClient();
            statusMessage = success ? $"Connecting to {serverIP}:{serverPort}..." : "Failed to start client";
            serverAddress = serverIP;
            port = serverPort;
        }
        catch (System.Exception e)
        {
            statusMessage = $"Connection failed: {e.Message}";
            Debug.LogError(statusMessage);
        }
    }

    private void StartServer()
    {
        try
        {
            transport.SetConnectionData("127.0.0.1", port);
            bool success = NetworkManager.Singleton.StartServer();
            statusMessage = success ? $"Server started on port {port}" : "Failed to start server";
        }
        catch (System.Exception e)
        {
            statusMessage = $"Server start failed: {e.Message}";
            Debug.LogError(statusMessage);
        }
    }

    private string GetNetworkMode()
    {
        if (NetworkManager.Singleton.IsHost) return "Host";
        if (NetworkManager.Singleton.IsServer) return "Server";
        if (NetworkManager.Singleton.IsClient) return "Client";
        return "None";
    }
}
