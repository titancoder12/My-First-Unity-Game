using UnityEngine;
using Unity.Netcode;
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float lookSensitivity = 1f;
    [SerializeField]
    string remoteLayerName = "RemotePlayer";
    private PlayerMotor motor;
    private PlayerAnimationController animationController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public AudioListener audioListener;
    public Camera playerCamera;
    Camera sceneCamera;

    public override void OnNetworkSpawn()
    {
        string _ID = "Player " + GetComponent<NetworkObject>().NetworkObjectId;
        transform.name = _ID;

        motor = GetComponent<PlayerMotor>();
        if (motor == null)
        {
            Debug.LogError("PlayerMotor component is missing from the GameObject.");
        }

        animationController = GetComponent<PlayerAnimationController>();
        // Temporarily comment out animation controller to fix compilation
        /*
        if (animationController == null)
        {
            Debug.LogWarning("PlayerAnimationController component is missing. Animations will not work.");
        }
        */

        if (!IsOwner)
        {
            // Disable audio listener and camera on non-local players
            if (audioListener != null)
                audioListener.enabled = false;

            if (playerCamera != null)
                playerCamera.enabled = false;

            playerCamera.gameObject.SetActive(false);
            AssignRemoteLayer();
            return;
        }

        // Lock cursor for local player
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        sceneCamera = Camera.main;

        if (sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(false); // Disable the main camera if it exists
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return; // Only the owner of this object can control it
        }

        // Handle cursor lock toggle with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }

        float _xMov = Input.GetAxisRaw("Horizontal");
        float _zMov = Input.GetAxisRaw("Vertical");
        Vector3 _movhorizontal = transform.right * _xMov;
        Vector3 _movvertical = transform.forward * _zMov;

        Vector3 _velocity = (_movhorizontal + _movvertical).normalized * speed;

        motor.Move(_velocity);

        // Handle jump input
        if (Input.GetButtonDown("Jump"))
        {
            motor.Jump();
            // Trigger jump animation - temporarily commented out
            /*
            if (animationController != null)
            {
                animationController.TriggerJump();
            }
            */
        }

        // Handle shooting input (example - you can modify this based on your shooting system)
        if (Input.GetButtonDown("Fire1"))
        {
            // Trigger shoot animation - temporarily commented out
            /*
            if (animationController != null)
            {
                animationController.TriggerShoot();
            }
            */
        }

        // Turning the player
        float _yRot = Input.GetAxisRaw("Mouse X");

        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * lookSensitivity;
        motor.Rotate(_rotation);

        float _xRot = Input.GetAxisRaw("Mouse Y");

        Vector3 _cameraRotation = new Vector3(_xRot, 0f, 0f) * lookSensitivity;
        motor.RotateCamera(_cameraRotation);
        //Quaternion _rotationQuaternion = Quaternion.Euler(_rotation);
        //transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation * _rotationQuaternion, Time.deltaTime * 10f);
    }

    void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // Unlock cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }
}
