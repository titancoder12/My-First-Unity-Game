using UnityEngine;
using Unity.Netcode;

public class PlayerAnimationController : NetworkBehaviour
{
    [Header("Animation Components")]
    [SerializeField] private Animator animator;
    
    [Header("Animation Parameters")]
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int IsShootingHash = Animator.StringToHash("IsShooting");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    
    [Header("Animation Settings")]
    [SerializeField] private float walkThreshold = 0.1f;
    [SerializeField] private float runThreshold = 3f;
    [SerializeField] private float jumpCooldown = 1f;
    
    // Network variables for animation synchronization
    private NetworkVariable<bool> networkIsWalking = new NetworkVariable<bool>();
    private NetworkVariable<bool> networkIsRunning = new NetworkVariable<bool>();
    private NetworkVariable<bool> networkIsJumping = new NetworkVariable<bool>();
    private NetworkVariable<bool> networkIsShooting = new NetworkVariable<bool>();
    private NetworkVariable<float> networkSpeed = new NetworkVariable<float>();
    private NetworkVariable<bool> networkIsGrounded = new NetworkVariable<bool>();
    
    private PlayerMotor playerMotor;
    private Rigidbody rb;
    private float lastJumpTime;
    private Vector3 lastPosition;
    private float currentSpeed;
    
    public override void OnNetworkSpawn()
    {
        playerMotor = GetComponent<PlayerMotor>();
        rb = GetComponent<Rigidbody>();
        
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
            
        lastPosition = transform.position;
        
        // Subscribe to network variable changes
        networkIsWalking.OnValueChanged += OnWalkingChanged;
        networkIsRunning.OnValueChanged += OnRunningChanged;
        networkIsJumping.OnValueChanged += OnJumpingChanged;
        networkIsShooting.OnValueChanged += OnShootingChanged;
        networkSpeed.OnValueChanged += OnSpeedChanged;
        networkIsGrounded.OnValueChanged += OnGroundedChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        // Unsubscribe from network variable changes
        networkIsWalking.OnValueChanged -= OnWalkingChanged;
        networkIsRunning.OnValueChanged -= OnRunningChanged;
        networkIsJumping.OnValueChanged -= OnJumpingChanged;
        networkIsShooting.OnValueChanged -= OnShootingChanged;
        networkSpeed.OnValueChanged -= OnSpeedChanged;
        networkIsGrounded.OnValueChanged -= OnGroundedChanged;
    }
    
    void Update()
    {
        if (!IsOwner) return;
        
        UpdateAnimationStates();
    }
    
    void UpdateAnimationStates()
    {
        // Calculate current speed
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        currentSpeed = horizontalVelocity.magnitude;
        
        // Update movement animations
        bool isWalking = currentSpeed > walkThreshold && currentSpeed <= runThreshold;
        bool isRunning = currentSpeed > runThreshold;
        bool isGrounded = playerMotor.IsGrounded();
        
        // Update network variables
        networkSpeed.Value = currentSpeed;
        networkIsWalking.Value = isWalking;
        networkIsRunning.Value = isRunning;
        networkIsGrounded.Value = isGrounded;
        
        // Update local animator
        UpdateAnimator();
    }
    
    void UpdateAnimator()
    {
        if (animator == null) return;
        
        animator.SetFloat(SpeedHash, networkSpeed.Value);
        animator.SetBool(IsWalkingHash, networkIsWalking.Value);
        animator.SetBool(IsRunningHash, networkIsRunning.Value);
        animator.SetBool(IsJumpingHash, networkIsJumping.Value);
        animator.SetBool(IsShootingHash, networkIsShooting.Value);
        animator.SetBool(IsGroundedHash, networkIsGrounded.Value);
    }
    
    public void TriggerJump()
    {
        if (Time.time - lastJumpTime > jumpCooldown && networkIsGrounded.Value)
        {
            lastJumpTime = Time.time;
            TriggerJumpServerRpc();
        }
    }
    
    [ServerRpc]
    void TriggerJumpServerRpc()
    {
        networkIsJumping.Value = true;
        // Reset jumping animation after a short delay
        Invoke(nameof(ResetJumpAnimation), 0.5f);
    }
    
    void ResetJumpAnimation()
    {
        networkIsJumping.Value = false;
    }
    
    public void TriggerShoot()
    {
        TriggerShootServerRpc();
    }
    
    [ServerRpc]
    void TriggerShootServerRpc()
    {
        networkIsShooting.Value = true;
        // Reset shooting animation after a short delay
        Invoke(nameof(ResetShootAnimation), 0.3f);
    }
    
    void ResetShootAnimation()
    {
        networkIsShooting.Value = false;
    }
    
    // Network variable change callbacks
    void OnWalkingChanged(bool previous, bool current)
    {
        if (animator != null)
            animator.SetBool(IsWalkingHash, current);
    }
    
    void OnRunningChanged(bool previous, bool current)
    {
        if (animator != null)
            animator.SetBool(IsRunningHash, current);
    }
    
    void OnJumpingChanged(bool previous, bool current)
    {
        if (animator != null)
            animator.SetBool(IsJumpingHash, current);
    }
    
    void OnShootingChanged(bool previous, bool current)
    {
        if (animator != null)
            animator.SetBool(IsShootingHash, current);
    }
    
    void OnSpeedChanged(float previous, float current)
    {
        if (animator != null)
            animator.SetFloat(SpeedHash, current);
    }
    
    void OnGroundedChanged(bool previous, bool current)
    {
        if (animator != null)
            animator.SetBool(IsGroundedHash, current);
    }
}
