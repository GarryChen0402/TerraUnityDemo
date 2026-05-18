using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Jump Feel")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private bool IsGrounded => Physics2D.OverlapCircle(
        groundCheck.position, groundCheckRadius, groundLayer);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (groundCheck == null)
            Debug.LogError("GroundCheck Transform not assigned! Drag the GroundCheck child into the Inspector.");
        if (groundLayer.value == 0)
            Debug.LogError("Ground Layer not set! Set the Ground layer in the Inspector.");
    }

    private void Update()
    {
        if (IsGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
            Debug.Log($"Space pressed | IsGrounded={IsGrounded}, coyoteTimer={coyoteTimeCounter:F2}, jumpBuffer={jumpBufferCounter:F2}");
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            Debug.Log("Jump executed!");
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    private void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        if (horizontal > 0)
            spriteRenderer.flipX = false;
        else if (horizontal < 0)
            spriteRenderer.flipX = true;
    }
}
