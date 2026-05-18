using UnityEngine;

public class SlimeEnemy : EnemyBase
{
    [Header("Slime Settings")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float jumpInterval = 1.5f;

    private float jumpTimer;
    private bool isGrounded;
    private float patrolDirection = 1f;
    private float patrolTimer;

    protected override void Awake()
    {
        base.Awake();
        patrolTimer = Random.Range(2f, 5f);
    }

    protected override void OnPatrol()
    {
        if (isKnockedBack) return;

        jumpTimer -= Time.deltaTime;
        patrolTimer -= Time.deltaTime;

        if (patrolTimer <= 0)
        {
            patrolDirection *= -1;
            patrolTimer = Random.Range(2f, 5f);
        }

        if (jumpTimer <= 0 && isGrounded)
        {
            rb.velocity = new Vector2(patrolDirection * moveSpeed, jumpForce);
            jumpTimer = jumpInterval;
            isGrounded = false;
        }

        spriteRenderer.flipX = patrolDirection < 0;
    }

    protected override void OnChase()
    {
        if (isKnockedBack) return;

        jumpTimer -= Time.deltaTime;

        if (jumpTimer <= 0 && isGrounded)
        {
            float dirX = (playerTransform.position.x - transform.position.x) > 0 ? 1f : -1f;
            rb.velocity = new Vector2(dirX * moveSpeed * 1.3f, jumpForce);
            jumpTimer = jumpInterval * 0.8f;
            isGrounded = false;
            spriteRenderer.flipX = dirX < 0;
        }
    }

    protected override void OnAttack()
    {
        // Slime has no dedicated attack — contact damage in OnCollisionEnter2D
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            isGrounded = true;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats.Instance?.TakeDamage(damage);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            isGrounded = false;
    }
}
