using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Jump / Gravity")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Crouch")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 8f;

    private CharacterController controller;
    private Vector3 velocity; // vertical velocity stored here
    private float currentSpeed;
    private bool isCrouching;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        // initialize controller height if it was left at default
        controller.height = standingHeight;
        controller.center = new Vector3(0, controller.height / 2f, 0);
    }

    void Update()
    {
        HandleMovement();
        ApplyGravityAndJump();
        HandleCrouchHeight();
    }

    private void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(inputX, 0, inputZ);
        input = Vector3.ClampMagnitude(input, 1f);

        // Determine speed
        bool sprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool crouchHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        isCrouching = crouchHeld;

        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (sprinting && input.magnitude > 0.1f)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = walkSpeed;

        // Move relative to player orientation
        Vector3 move = transform.TransformDirection(input) * currentSpeed;

        // Apply horizontal movement (vertical handled separately)
        Vector3 horizontal = new Vector3(move.x, 0, move.z);
        controller.Move(horizontal * Time.deltaTime);
    }

    private void ApplyGravityAndJump()
    {
        if (controller.isGrounded && velocity.y < 0f)
        {
            // small negative to keep grounded
            velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded && !isCrouching)
        {
            // v = sqrt(2 * g * h)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCrouchHeight()
    {
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        if (Mathf.Approximately(controller.height, targetHeight))
            return;

        float newHeight = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        float heightDiff = controller.height - newHeight;

        controller.height = newHeight;
        // adjust center so player doesn't sink into floor
        controller.center = new Vector3(0, controller.height / 2f, 0);

        // If we reduced height while grounded, make sure player remains grounded
        if (controller.isGrounded && heightDiff > 0f)
        {
            // small nudge up so the controller doesn't clip
            controller.Move(Vector3.up * heightDiff);
        }
    }
}
