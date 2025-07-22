using UnityEngine;
using Unity.Netcode;

public class PlayerWeaponController : NetworkBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private float reloadTime = 2f;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;
    
    private PlayerAnimationController animationController;
    private AudioSource audioSource;
    private float lastFireTime;
    private int currentAmmo;
    private bool isReloading;
    
    public override void OnNetworkSpawn()
    {
        animationController = GetComponent<PlayerAnimationController>();
        audioSource = GetComponent<AudioSource>();
        currentAmmo = maxAmmo;
    }
    
    void Update()
    {
        if (!IsOwner) return;
        
        HandleInput();
    }
    
    void HandleInput()
    {
        // Fire weapon
        if (Input.GetButton("Fire1") && CanFire())
        {
            Fire();
        }
        
        // Reload weapon
        if (Input.GetKeyDown(KeyCode.R) && CanReload())
        {
            StartReload();
        }
    }
    
    bool CanFire()
    {
        return Time.time - lastFireTime > fireRate && currentAmmo > 0 && !isReloading;
    }
    
    bool CanReload()
    {
        return currentAmmo < maxAmmo && !isReloading;
    }
    
    void Fire()
    {
        lastFireTime = Time.time;
        currentAmmo--;
        
        // Trigger shooting animation
        if (animationController != null)
        {
            animationController.TriggerShoot();
        }
        
        // Play effects
        if (muzzleFlash != null)
            muzzleFlash.Play();
            
        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);
        
        FireServerRpc();
    }
    
    [ServerRpc]
    void FireServerRpc()
    {
        // Server-side projectile spawning logic would go here
        // For now, just play effects on all clients
        FireClientRpc();
    }
    
    [ClientRpc]
    void FireClientRpc()
    {
        if (!IsOwner)
        {
            // Play effects for other players
            if (muzzleFlash != null)
                muzzleFlash.Play();
                
            if (audioSource != null && fireSound != null)
                audioSource.PlayOneShot(fireSound);
        }
    }
    
    void StartReload()
    {
        isReloading = true;
        
        if (audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound);
        
        Invoke(nameof(CompleteReload), reloadTime);
    }
    
    void CompleteReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }
    
    // Public getters for UI
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
    public bool IsReloading() => isReloading;
}
