using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class JumpKing : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float jumpImpulse = 10f;
    public float maxJumpCharge = 20f;
    public float chargeRate = 10f;

    private float jumpCharge = 0f;
    private bool isCharging = false;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool _isFacingRight = true;
    private bool _isMoving = false;

    public LayerMask groundLayer;
    public Transform groundCheck;
    public Vector2 groundCheckSize;

    private Rigidbody2D rb;
    private Animator animator;

    // Animation Parameters
    public static class AnimationStrings
    {
        public const string isMoving = "isMoving";
        public const string jump = "jump";
        public const string yVelocity = "yVelocity";
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        // Apply horizontal velocity
        rb.velocity = new Vector2(moveInput.x * walkSpeed, rb.velocity.y);

        // Update animations
        animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);

        // Check if grounded
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        // Update moving state
        _isMoving = moveInput.x != 0;
        animator.SetBool(AnimationStrings.isMoving, _isMoving);

        // Update facing direction
        SetFacingDirection(moveInput);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded)
        {
            isCharging = true;
        }

        if (context.canceled && isCharging && isGrounded)
        {
            isCharging = false;

            // Perform jump
            rb.velocity = new Vector2(rb.velocity.x, jumpCharge);

            // Trigger jump animation
            animator.SetTrigger(AnimationStrings.jump);

            // Reset jump charge
            jumpCharge = 0f;
        }
    }

    private void Update()
    {
        // Handle jump charging
        if (isCharging && isGrounded)
        {
            jumpCharge += chargeRate * Time.deltaTime;
            jumpCharge = Mathf.Clamp(jumpCharge, 0, maxJumpCharge);
        }
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !_isFacingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && _isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        _isFacingRight = !_isFacingRight;
        transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            // Handle coin collection
            collision.gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize the ground check area in the Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}
