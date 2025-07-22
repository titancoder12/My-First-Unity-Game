using UnityEngine;
using UnityEngine.UI;
// using TMPro; // Comment out TMPro if not available

public class PlayerUI : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private Text ammoText; // Changed from TextMeshProUGUI to Text
    [SerializeField] private Text speedText; // Changed from TextMeshProUGUI to Text
    [SerializeField] private Text statusText; // Changed from TextMeshProUGUI to Text
    [SerializeField] private Image crosshair;
    [SerializeField] private GameObject reloadIndicator;
    
    [Header("Animation Feedback")]
    [SerializeField] private Image jumpIndicator;
    [SerializeField] private Color jumpColor = Color.yellow;
    [SerializeField] private float indicatorFadeTime = 0.5f;
    
    private PlayerWeaponController weaponController;
    private PlayerAnimationController animationController;
    private PlayerMotor playerMotor;
    private Rigidbody playerRb;
    
    void Start()
    {
        // Find components on the local player
        var networkManager = Unity.Netcode.NetworkManager.Singleton;
        if (networkManager != null && networkManager.LocalClient != null)
        {
            var playerObject = networkManager.LocalClient.PlayerObject;
            if (playerObject != null)
            {
                var player = playerObject.gameObject;
                weaponController = player.GetComponent<PlayerWeaponController>();
                animationController = player.GetComponent<PlayerAnimationController>();
                playerMotor = player.GetComponent<PlayerMotor>();
                playerRb = player.GetComponent<Rigidbody>();
            }
        }
        
        // Fallback: try to find by tag
        if (weaponController == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                weaponController = player.GetComponent<PlayerWeaponController>();
                animationController = player.GetComponent<PlayerAnimationController>();
                playerMotor = player.GetComponent<PlayerMotor>();
                playerRb = player.GetComponent<Rigidbody>();
            }
        }
    }
    
    void Update()
    {
        UpdateHUD();
    }
    
    void UpdateHUD()
    {
        UpdateAmmoDisplay();
        UpdateSpeedDisplay();
        UpdateStatusDisplay();
        UpdateReloadIndicator();
    }
    
    void UpdateAmmoDisplay()
    {
        if (weaponController != null && ammoText != null)
        {
            ammoText.text = $"Ammo: {weaponController.GetCurrentAmmo()}/{weaponController.GetMaxAmmo()}";
        }
    }
    
    void UpdateSpeedDisplay()
    {
        if (playerRb != null && speedText != null)
        {
            float currentSpeed = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z).magnitude;
            speedText.text = $"Speed: {currentSpeed:F1} m/s";
        }
    }
    
    void UpdateStatusDisplay()
    {
        if (statusText != null)
        {
            string status = "";
            
            if (playerMotor != null)
            {
                if (!playerMotor.IsGrounded())
                    status += "Airborne ";
                else
                    status += "Grounded ";
            }
            
            if (weaponController != null && weaponController.IsReloading())
                status += "Reloading ";
            
            statusText.text = status.Trim();
        }
    }
    
    void UpdateReloadIndicator()
    {
        if (weaponController != null && reloadIndicator != null)
        {
            reloadIndicator.SetActive(weaponController.IsReloading());
        }
    }
    
    public void ShowJumpFeedback()
    {
        if (jumpIndicator != null)
        {
            StartCoroutine(FadeIndicator(jumpIndicator, jumpColor));
        }
    }
    
    private System.Collections.IEnumerator FadeIndicator(Image indicator, Color color)
    {
        indicator.color = color;
        indicator.gameObject.SetActive(true);
        
        float elapsedTime = 0f;
        Color startColor = color;
        Color endColor = new Color(color.r, color.g, color.b, 0f);
        
        while (elapsedTime < indicatorFadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / indicatorFadeTime;
            indicator.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        indicator.gameObject.SetActive(false);
    }
}
