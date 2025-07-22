using UnityEngine;
using Unity.Netcode;

public class PlayerShootScript : NetworkBehaviour
{
    private const string PLAYER_TAG = "Player";
    public PlayerWeapon weapon; // Reference to the player's weapon script

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;
    public AudioClip gunshotClip;
    private AudioSource audioSource;
    
    // Shooting UI feedback
    private bool isShooting = false;
    private float shootingUITimer = 0f;
    private float shootingUIDuration = 0.1f; // How long the shooting effect lasts
    
    // Firing rate control
    private float lastShotTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (cam == null)
        {
            Debug.LogError("PlayerShoot: Camera component is missing from the GameObject.");
            this.enabled = false; // Disable the script if no camera is assigned
        }
        
        // Initialize audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Initialize weapon ammo
        if (weapon != null)
        {
            weapon.Initialize();
        }
    }

    // Draw crosshair on screen
    void OnGUI()
    {
        if (!IsOwner) return; // Only draw crosshair for the local player
        
        // Get screen center
        float screenCenterX = Screen.width / 2f;
        float screenCenterY = Screen.height / 2f;
        
        // Crosshair size - bigger when shooting
        float crosshairSize = isShooting ? 60f : 50f;
        float crosshairThickness = isShooting ? 7f : 5f;
        
        // Draw crosshair lines - red when shooting, white normally
        GUI.color = isShooting ? Color.red : Color.white;
        
        // Horizontal line
        GUI.DrawTexture(new Rect(screenCenterX - crosshairSize/2, screenCenterY - crosshairThickness/2, 
                                crosshairSize, crosshairThickness), Texture2D.whiteTexture);
        
        // Vertical line  
        GUI.DrawTexture(new Rect(screenCenterX - crosshairThickness/2, screenCenterY - crosshairSize/2, 
                                crosshairThickness, crosshairSize), Texture2D.whiteTexture);
    }

    // Update is called once per frame
    void Update()
    {
        // Handle shooting UI timer
        if (isShooting)
        {
            shootingUITimer -= Time.deltaTime;
            if (shootingUITimer <= 0f)
            {
                isShooting = false;
            }
        }

        // Handle reload input
        if (Input.GetKeyDown(KeyCode.R) && IsOwner)
        {
            StartReload();
        }

        // Handle shooting input with firing rate and ammo check
        bool shouldShoot = weapon != null && weapon.isAutomatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");
        
        if (shouldShoot && IsOwner && weapon != null && Time.time >= lastShotTime + weapon.GetTimeBetweenShots())
        {
            if (weapon.CanShoot())
            {
                Shoot();
                lastShotTime = Time.time;
            }
            else if (weapon.currentAmmoInClip == 0)
            {
                // Play empty sound when trying to shoot with no ammo
                PlayEmptySound();
                lastShotTime = Time.time; // Prevent spam clicking empty sound
            }
        }
        
        // Auto-reload when clip is empty and player tries to shoot
        if (Input.GetButtonDown("Fire1") && IsOwner && weapon != null && weapon.currentAmmoInClip == 0 && weapon.CanReload())
        {
            StartReload();
        }
    }

    void PlayGunshotLocal()
    {
        if (audioSource)
        {
            // Use weapon fire sounds if available, otherwise use default gunshot clip
            if (weapon != null && weapon.fireSounds != null && weapon.fireSounds.Length > 0)
            {
                AudioClip randomFireSound = weapon.fireSounds[Random.Range(0, weapon.fireSounds.Length)];
                audioSource.PlayOneShot(randomFireSound);
            }
            else if (gunshotClip)
            {
                audioSource.PlayOneShot(gunshotClip);
            }
        }
    }

    [ClientRpc]
    void PlayGunshotClientRpc()
    {
        if (!IsOwner) // skip self, already played
        {
            PlayGunshotLocal();
        }
    }

    void Shoot()
    {
        if (!IsClient || weapon == null || !weapon.CanShoot()) return;
        
        RaycastHit _hit;
        PlayGunshotLocal();
        
        // Consume ammo
        weapon.ConsumeAmmo();
        
        // Trigger shooting UI effect
        isShooting = true;
        shootingUITimer = shootingUIDuration;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, weapon.range, mask))
        {
            // We hit something!
            Debug.Log("We hit: " + _hit.collider.name);
            if (_hit.collider.CompareTag("Player"))
            {
                Debug.Log("We hit a player!");
                
                // Get the hit player's health component
                PlayerHealth targetHealth = _hit.collider.GetComponent<PlayerHealth>();
                if (targetHealth != null)
                {
                    // Deal damage to the target player
                    targetHealth.TakeDamageServerRpc(weapon.damage, NetworkManager.Singleton.LocalClientId);
                }
                
                // Call method to handle the shot
                CmdPlayerShotServerRPC(_hit.collider.name);
            }
        }
    }
    
    void StartReload()
    {
        if (weapon == null || !weapon.CanReload()) return;
        
        weapon.StartReload();
        PlayReloadSound();
        
        // Start reload coroutine
        StartCoroutine(ReloadCoroutine());
    }
    
    private System.Collections.IEnumerator ReloadCoroutine()
    {
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(weapon.reloadTime);
        
        weapon.CompleteReload();
        Debug.Log($"Reload complete! Ammo: {weapon.currentAmmoInClip}/{weapon.currentTotalAmmo}");
    }
    
    void PlayReloadSound()
    {
        if (audioSource && weapon.reloadSound)
        {
            audioSource.PlayOneShot(weapon.reloadSound);
        }
    }
    
    void PlayEmptySound()
    {
        if (audioSource && weapon.emptySound)
        {
            audioSource.PlayOneShot(weapon.emptySound);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    void CmdPlayerShotServerRPC(string _ID)
    {
        Debug.Log(_ID + " has been shot!");
        PlayGunshotClientRpc();
    }
}
