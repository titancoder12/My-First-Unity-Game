using UnityEngine;

[System.Serializable]
public class PlayerWeapon
{
    [Header("Weapon Stats")]
    public string name = "Assault Rifle";
    
    [Header("Damage")]
    public float damage = 25f;
    public float headShotMultiplier = 2f;
    
    [Header("Ammo System")]
    public int maxAmmoInClip = 30;
    public int maxTotalAmmo = 10000000;
    public float reloadTime = 2.5f;
    
    [Header("Range & Accuracy")]
    public float range = 100f;
    public float accuracy = 0.95f;
    
    [Header("Rate of Fire")]
    public float fireRate = 600f; // rounds per minute
    public bool isAutomatic = true;
    
    [Header("Audio")]
    public AudioClip[] fireSounds;
    public AudioClip reloadSound;
    public AudioClip emptySound;
    
    // Current ammo state (these will be managed by the shooting script)
    [System.NonSerialized]
    public int currentAmmoInClip;
    [System.NonSerialized]
    public int currentTotalAmmo;
    [System.NonSerialized]
    public bool isReloading = false;
    
    // Initialize ammo on weapon creation
    public void Initialize()
    {
        currentAmmoInClip = maxAmmoInClip;
        currentTotalAmmo = maxTotalAmmo;
        isReloading = false;
    }
    
    // Calculate time between shots
    public float GetTimeBetweenShots()
    {
        return 60f / fireRate;
    }
    
    // Check if weapon can shoot
    public bool CanShoot()
    {
        return currentAmmoInClip > 0 && !isReloading;
    }
    
    // Check if weapon can reload
    public bool CanReload()
    {
        return !isReloading && currentAmmoInClip < maxAmmoInClip && currentTotalAmmo > 0;
    }
    
    // Consume ammo when shooting
    public void ConsumeAmmo()
    {
        if (currentAmmoInClip > 0)
        {
            currentAmmoInClip--;
        }
    }
    
    // Reload weapon
    public void StartReload()
    {
        if (CanReload())
        {
            isReloading = true;
        }
    }
    
    // Complete reload process
    public void CompleteReload()
    {
        if (isReloading)
        {
            int ammoNeeded = maxAmmoInClip - currentAmmoInClip;
            int ammoToReload = Mathf.Min(ammoNeeded, currentTotalAmmo);
            
            currentAmmoInClip += ammoToReload;
            currentTotalAmmo -= ammoToReload;
            isReloading = false;
        }
    }
}