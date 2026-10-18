using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Tooltip("You can still jump this long after walking off an edge")]
    public float coyoteTime = 0.1f;
    [Tooltip("A jump pressed this long before landing still happens")]
    public float jumpBufferTime = 0.12f;

    public bool FacingRight { get; private set; }

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Health health;

    private float moveX;
    private bool isGrounded;
    private float lastGroundedTime = -10f;
    private float lastJumpPressedTime = -10f;
    private float knockbackUntil;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();
        FacingRight = true;

        rb.freezeRotation = true; // otherwise the player tips over when an enemy bumps into him
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private bool CanControl
    {
        get { return (health == null || !health.IsDead) && Time.timeScale > 0f; }
    }

    private void Update()
    {
        if (!CanControl)
        {
            moveX = 0f;
            return;
        }

        moveX = Input.GetAxisRaw("Horizontal");
        if (moveX > 0f) SetFacing(true);
        else if (moveX < 0f) SetFacing(false);

        if (JumpPressed()) lastJumpPressedTime = Time.time;

        bool recentlyGrounded = Time.time - lastGroundedTime <= coyoteTime;
        bool recentlyPressed = Time.time - lastJumpPressedTime <= jumpBufferTime;
        if (recentlyGrounded && recentlyPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            lastJumpPressedTime = -10f;
            lastGroundedTime = -10f;
            Sfx.Play("jump", 0.5f);
        }

        // Letting go of jump early gives a shorter hop
        if (JumpReleased() && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    private void FixedUpdate()
    {
        // isGrounded was set by the collision callbacks of the previous physics step
        if (isGrounded) lastGroundedTime = Time.time;
        isGrounded = false;

        if (Time.time < knockbackUntil) return; // let the knockback play out
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
    }

    public void Knockback(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
        knockbackUntil = Time.time + 0.2f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGround(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckGround(collision);
    }

    // Grounded = standing on top of something (contact normal pointing up). Walls don't count,
    // so there is no wall-jumping, and enemies don't count, so you can't bounce off their heads.
    private void CheckGround(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) return;
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.6f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void SetFacing(bool right)
    {
        if (right == FacingRight) return;
        FacingRight = right;
        if (sprite != null) sprite.flipX = !right;
    }

    private static bool JumpPressed()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
    }

    private static bool JumpReleased()
    {
        return Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow);
    }
}
