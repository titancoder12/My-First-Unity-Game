using UnityEngine;
using Unity.Netcode;

public class AmmoUI : MonoBehaviour
{
    private PlayerShootScript playerShoot;
    private GUIStyle ammoStyle;
    private GUIStyle reloadStyle;
    private bool stylesInitialized = false;
    
    void Start()
    {
        // Find the local player's shoot script
        StartCoroutine(FindLocalPlayerShoot());
    }
    
    private System.Collections.IEnumerator FindLocalPlayerShoot()
    {
        // Wait a bit for network spawning to complete
        yield return new WaitForSeconds(1f);
        
        // Find all player shoot components
        PlayerShootScript[] allShootComponents = FindObjectsOfType<PlayerShootScript>();
        
        foreach (var shoot in allShootComponents)
        {
            if (shoot.IsOwner)
            {
                playerShoot = shoot;
                break;
            }
        }
        
        if (playerShoot == null)
        {
            // Try again in a second
            yield return new WaitForSeconds(1f);
            StartCoroutine(FindLocalPlayerShoot());
        }
    }
    
    void OnGUI()
    {
        if (playerShoot == null || playerShoot.weapon == null || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
            return;
            
        InitializeStyles();
        DrawAmmoDisplay();
    }
    
    private void InitializeStyles()
    {
        if (stylesInitialized) return;
        
        ammoStyle = new GUIStyle(GUI.skin.label);
        ammoStyle.fontSize = 20;
        ammoStyle.fontStyle = FontStyle.Bold;
        ammoStyle.normal.textColor = Color.white;
        ammoStyle.alignment = TextAnchor.MiddleRight;
        
        reloadStyle = new GUIStyle(GUI.skin.label);
        reloadStyle.fontSize = 16;
        reloadStyle.fontStyle = FontStyle.Bold;
        reloadStyle.normal.textColor = Color.yellow;
        reloadStyle.alignment = TextAnchor.MiddleRight;
        
        stylesInitialized = true;
    }
    
    private void DrawAmmoDisplay()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        // Ammo display dimensions (bottom-right corner)
        float displayWidth = 150f;
        float displayHeight = 80f;
        float displayX = screenWidth - displayWidth - 20f;
        float displayY = screenHeight - displayHeight - 20f;
        
        var weapon = playerShoot.weapon;
        
        // Background
        GUI.color = new Color(0, 0, 0, 0.5f);
        GUI.Box(new Rect(displayX - 10, displayY - 10, displayWidth + 20, displayHeight + 20), "");
        
        // Ammo count
        if (weapon.currentAmmoInClip <= 0)
        {
            GUI.color = Color.red;
        }
        else if (weapon.currentAmmoInClip <= weapon.maxAmmoInClip * 0.25f)
        {
            GUI.color = Color.yellow;
        }
        else
        {
            GUI.color = Color.white;
        }
        
        string ammoText = $"{weapon.currentAmmoInClip} / ∞";
        GUI.Label(new Rect(displayX, displayY, displayWidth, 40), ammoText, ammoStyle);
        
        // Weapon name
        GUI.color = Color.gray;
        GUI.Label(new Rect(displayX, displayY + 25, displayWidth, 25), weapon.name, reloadStyle);
        
        // Reload indicator
        if (weapon.isReloading)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(displayX, displayY + 45, displayWidth, 25), "RELOADING...", reloadStyle);
        }
        else if (weapon.currentAmmoInClip == 0 && weapon.currentTotalAmmo > 0)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(displayX, displayY + 45, displayWidth, 25), "PRESS R TO RELOAD", reloadStyle);
        }
        
        // Reset color
        GUI.color = Color.white;
    }
}
