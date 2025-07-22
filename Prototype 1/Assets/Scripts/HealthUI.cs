using UnityEngine;
using Unity.Netcode;

public class HealthUI : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private GUIStyle healthBarStyle;
    private GUIStyle healthTextStyle;
    private bool stylesInitialized = false;
    
    void Start()
    {
        // Find the local player's health component
        StartCoroutine(FindLocalPlayerHealth());
    }
    
    private System.Collections.IEnumerator FindLocalPlayerHealth()
    {
        // Wait a bit for network spawning to complete
        yield return new WaitForSeconds(1f);
        
        // Find all player health components
        PlayerHealth[] allHealthComponents = FindObjectsOfType<PlayerHealth>();
        
        foreach (var health in allHealthComponents)
        {
            if (health.IsOwner)
            {
                playerHealth = health;
                break;
            }
        }
        
        if (playerHealth == null)
        {
            // Try again in a second
            yield return new WaitForSeconds(1f);
            StartCoroutine(FindLocalPlayerHealth());
        }
    }
    
    void OnGUI()
    {
        if (playerHealth == null || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
            return;
            
        InitializeStyles();
        DrawHealthBar();
    }
    
    private void InitializeStyles()
    {
        if (stylesInitialized) return;
        
        healthBarStyle = new GUIStyle(GUI.skin.box);
        healthBarStyle.normal.background = MakeTexture(2, 2, Color.red);
        
        healthTextStyle = new GUIStyle(GUI.skin.label);
        healthTextStyle.fontSize = 16;
        healthTextStyle.fontStyle = FontStyle.Bold;
        healthTextStyle.normal.textColor = Color.white;
        healthTextStyle.alignment = TextAnchor.MiddleCenter;
        
        stylesInitialized = true;
    }
    
    private void DrawHealthBar()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        // Health bar dimensions
        float barWidth = 200f;
        float barHeight = 20f;
        float barX = 20f;
        float barY = screenHeight - 60f;
        
        // Background (dark)
        GUI.color = Color.black;
        GUI.Box(new Rect(barX - 2, barY - 2, barWidth + 4, barHeight + 4), "");
        
        // Health bar background (dark red)
        GUI.color = new Color(0.3f, 0.1f, 0.1f, 1f);
        GUI.Box(new Rect(barX, barY, barWidth, barHeight), "", healthBarStyle);
        
        // Health bar foreground (green to red based on health)
        float healthPercentage = playerHealth.GetHealthPercentage();
        float healthBarWidth = barWidth * healthPercentage;
        
        Color healthColor = Color.Lerp(Color.red, Color.green, healthPercentage);
        GUI.color = healthColor;
        
        if (healthPercentage > 0)
        {
            GUI.Box(new Rect(barX, barY, healthBarWidth, barHeight), "", healthBarStyle);
        }
        
        // Health text
        GUI.color = Color.white;
        string healthText = $"{Mathf.Ceil(playerHealth.GetCurrentHealth())}/{playerHealth.GetMaxHealth()}";
        GUI.Label(new Rect(barX, barY, barWidth, barHeight), healthText, healthTextStyle);
        
        // Player status
        if (playerHealth.IsDead())
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(barX, barY - 30, barWidth, 25), "ELIMINATED", healthTextStyle);
        }
        
        // Reset color
        GUI.color = Color.white;
    }
    
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = color;
            
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
