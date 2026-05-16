using UnityEngine;
using UnityEngine.InputSystem;

public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")] public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 3f;
    public float mouseSensitivity = 0.2f;

    public LayerMask groundLayer;
    public Transform playerCamera;
    public Transform armCamera;

    [Header("Ground Control Settings")] public float groundCheckOffset = 1f;
    public float groundCheckRadius = 0.4f;

    Rigidbody rb;
    InputAction moveAction, jumpAction, lookAction, sprintAction;
    float xRotation = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;

        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
        sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/shift");
    }

    void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
        lookAction?.Enable();
        sprintAction?.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
        lookAction?.Disable();
        sprintAction?.Disable();

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.None) return;

        if (jumpAction.WasPressedThisFrame() &&
            Physics.CheckSphere(transform.position + (Vector3.down * groundCheckOffset), groundCheckRadius,
                groundLayer))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        Vector2 look = lookAction.ReadValue<Vector2>() * mouseSensitivity;

        xRotation -= look.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        armCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * look.x);
    }

    void FixedUpdate()
    {
        if (Cursor.lockState == CursorLockMode.None)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        Vector2 input = moveAction.ReadValue<Vector2>();
        float currentSpeed = sprintAction.IsPressed() ? sprintSpeed : walkSpeed;
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        rb.linearVelocity = new Vector3(move.x * currentSpeed, rb.linearVelocity.y, move.z * currentSpeed);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + (Vector3.down * groundCheckOffset), groundCheckRadius);
    }
}