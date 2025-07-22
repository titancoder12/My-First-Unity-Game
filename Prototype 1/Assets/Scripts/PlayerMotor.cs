using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : NetworkBehaviour
{
    [SerializeField]
    private Camera cam;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private LayerMask groundLayerMask = -1; // All layers
    [SerializeField] private float groundCheckDistance = 1.2f;
    [SerializeField] private float groundCheckRadius = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool showGroundCheckGizmos = true;

    private Rigidbody rb;
    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private Vector3 cameraRotation = Vector3.zero;
    private bool shouldJump = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }

    public void RotateCamera(Vector3 _cameraRotation)
    {
        cameraRotation = _cameraRotation;
    }
    
    public void Jump()
    {
        if (IsGrounded())
        {
            Debug.Log("Jumping - Ground detected!");
            shouldJump = true;
        }
        else
        {
            Debug.Log("Cannot jump - Not grounded");
        }
    }
    
    public bool IsGrounded()
    {
        // Get the player's collider to determine the bottom position
        Collider playerCollider = GetComponent<Collider>();
        Vector3 raycastOrigin;
        
        if (playerCollider != null)
        {
            // Start raycast from the bottom of the collider
            raycastOrigin = new Vector3(transform.position.x, 
                                      playerCollider.bounds.min.y, 
                                      transform.position.z);
        }
        else
        {
            // Fallback to transform position
            raycastOrigin = transform.position;
        }
        
        // Perform multiple raycasts for better ground detection
        bool centerHit = Physics.Raycast(raycastOrigin, Vector3.down, groundCheckDistance, groundLayerMask);
        bool leftHit = Physics.Raycast(raycastOrigin + Vector3.left * 0.3f, Vector3.down, groundCheckDistance, groundLayerMask);
        bool rightHit = Physics.Raycast(raycastOrigin + Vector3.right * 0.3f, Vector3.down, groundCheckDistance, groundLayerMask);
        bool forwardHit = Physics.Raycast(raycastOrigin + Vector3.forward * 0.3f, Vector3.down, groundCheckDistance, groundLayerMask);
        bool backHit = Physics.Raycast(raycastOrigin + Vector3.back * 0.3f, Vector3.down, groundCheckDistance, groundLayerMask);
        
        bool isGrounded = centerHit || leftHit || rightHit || forwardHit || backHit;
        
        // Alternative: Use SphereCast for more reliable ground detection
        bool sphereCastHit = Physics.SphereCast(raycastOrigin + Vector3.up * 0.1f, groundCheckRadius, 
                                               Vector3.down, out RaycastHit hit, 
                                               groundCheckDistance + 0.1f, groundLayerMask);
        
        if (showGroundCheckGizmos)
        {
            Debug.Log($"Ground Check - Raycast: {isGrounded}, SphereCast: {sphereCastHit}, Y Velocity: {rb.velocity.y}");
        }
        
        return isGrounded || sphereCastHit;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsOwner)
        {
            return; // Only the owner of this object can control it
        }
        PerformMovement();
        PerformRotation();
        PerformCameraRotation();
        PerformJump();
    }

    void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            // Use velocity-based movement instead of position-based to work better with physics
            Vector3 targetVelocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.fixedDeltaTime * 10f);
        }
    }

    void PerformRotation()
    {
        if (rotation != Vector3.zero)
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        }
    }

    void PerformCameraRotation()
    {
        if (cam != null && cameraRotation != Vector3.zero)
        {
            cam.transform.Rotate(-cameraRotation);
        }
    }
    
    void PerformJump()
    {
        if (shouldJump)
        {
            Debug.Log($"Performing jump! Current Y velocity: {rb.velocity.y}");
            // Reset Y velocity before jumping for consistent jump height
            //Vector3 currentVelocity = rb.velocity;
            //currentVelocity.y = 0f;
            //rb.velocity = currentVelocity;
            
            // Apply jump force
            //rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            //shouldJump = false;
            transform.position += Vector3.up * 0.5f; // Slightly adjust position to avoid immediate re-grounding
            shouldJump = false;
        }
        //transform.position += Vector3.up * 2f;
    }
    
    // Debug visualization for ground checking
    void OnDrawGizmosSelected()
    {
        if (!showGroundCheckGizmos) return;
        
        Collider playerCollider = GetComponent<Collider>();
        Vector3 raycastOrigin;
        
        if (playerCollider != null)
        {
            raycastOrigin = new Vector3(transform.position.x, 
                                      playerCollider.bounds.min.y, 
                                      transform.position.z);
        }
        else
        {
            raycastOrigin = transform.position;
        }
        
        // Draw raycast lines
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawRay(raycastOrigin, Vector3.down * groundCheckDistance);
        Gizmos.DrawRay(raycastOrigin + Vector3.left * 0.3f, Vector3.down * groundCheckDistance);
        Gizmos.DrawRay(raycastOrigin + Vector3.right * 0.3f, Vector3.down * groundCheckDistance);
        Gizmos.DrawRay(raycastOrigin + Vector3.forward * 0.3f, Vector3.down * groundCheckDistance);
        Gizmos.DrawRay(raycastOrigin + Vector3.back * 0.3f, Vector3.down * groundCheckDistance);
        
        // Draw sphere for SphereCast
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(raycastOrigin + Vector3.up * 0.1f, groundCheckRadius);
        Gizmos.DrawWireSphere(raycastOrigin + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
