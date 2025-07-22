using UnityEngine;
using Unity.Netcode;

public class SimplePlayerAnimationController : NetworkBehaviour
{
    [Header("Animation Components")]
    [SerializeField] private Animator animator;
    
    [Header("Animation Settings")]
    [SerializeField] private float walkThreshold = 0.1f;
    [SerializeField] private float runThreshold = 3f;
    
    private PlayerMotor playerMotor;
    private Rigidbody rb;
    private float currentSpeed;
    private bool wasGrounded = true;
    
    // Animation parameter hashes
    private int isWalkingHash;
    private int isRunningHash;
    private int isJumpingHash;
    private int speedHash;
    private int isGroundedHash;
    
    public override void OnNetworkSpawn()
    {
        playerMotor = GetComponent<PlayerMotor>();
        rb = GetComponent<Rigidbody>();
        
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        
        // Cache animation parameter hashes
        if (animator != null)
        {
            isWalkingHash = Animator.StringToHash("IsWalking");
            isRunningHash = Animator.StringToHash("IsRunning");
            isJumpingHash = Animator.StringToHash("IsJumping");
            speedHash = Animator.StringToHash("Speed");
            isGroundedHash = Animator.StringToHash("IsGrounded");
        }
    }
    
    void Update()
    {
        if (!IsOwner || animator == null) return;
        
        UpdateAnimations();
    }
    
    void UpdateAnimations()
    {
        // Calculate current speed
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        currentSpeed = horizontalVelocity.magnitude;
        
        // Update movement animations
        bool isWalking = currentSpeed > walkThreshold && currentSpeed <= runThreshold;
        bool isRunning = currentSpeed > runThreshold;
        bool isGrounded = playerMotor != null ? playerMotor.IsGrounded() : true;
        
        // Detect jump
        bool isJumping = !isGrounded && wasGrounded && rb.velocity.y > 0.1f;
        
        // Update animator parameters
        animator.SetFloat(speedHash, currentSpeed);
        animator.SetBool(isWalkingHash, isWalking);
        animator.SetBool(isRunningHash, isRunning);
        animator.SetBool(isGroundedHash, isGrounded);
        
        if (isJumping)
        {
            animator.SetTrigger(isJumpingHash);
        }
        
        wasGrounded = isGrounded;
    }
    
    public void TriggerShoot()
    {
        if (animator != null && IsOwner)
        {
            animator.SetTrigger("IsShooting");
        }
    }
    
    public void TriggerJump()
    {
        if (animator != null && IsOwner)
        {
            animator.SetTrigger(isJumpingHash);
        }
    }
}
