using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("NPC Identity")]
    public string npcName = "Merchant";
    public ShopData shopData;

    [Header("Interaction")]
    public float interactionRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Wander")]
    [SerializeField] private float wanderSpeed = 1.5f;
    [SerializeField] private float wanderChangeInterval = 3f;
    [SerializeField] private bool enableWander = true;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    [Header("Night Behavior")]
    [TextArea] public string nightMessage = "The merchant looks nervous. Dangerous creatures roam at night...";

    private bool playerInRange;
    private bool hasShownNightMessage;
    private Rigidbody2D rb;
    private float wanderTimer;
    private float wanderDirection = 1f;
    private float interactCooldown;
    private bool isGrounded;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        wanderTimer = Random.Range(1f, wanderChangeInterval);
    }

    private void Update()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        playerInRange = dist <= interactionRange;

        interactCooldown -= Time.deltaTime;

        bool shopOpen = ShopUI.Instance != null && ShopUI.IsOpen;

        // Highlight
        if (spriteRenderer != null)
            spriteRenderer.color = playerInRange ? Color.yellow : Color.white;

        // Wandering or idle
        if (!shopOpen)
        {
            Wander();
        }
        else if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        // Interaction
        if (playerInRange && Input.GetKeyDown(interactKey) && interactCooldown <= 0f)
        {
            if (DayNightCycle.Instance != null && DayNightCycle.Instance.IsNight)
            {
                if (!hasShownNightMessage)
                {
                    Debug.Log($"[{npcName}]: {nightMessage}");
                    hasShownNightMessage = true;
                }
            }
            else
            {
                hasShownNightMessage = false;
            }

            if (shopData != null && ShopUI.Instance != null)
            {
                ShopUI.Instance.OpenShop(this);
                interactCooldown = 0.5f;
            }
        }
    }

    private void Wander()
    {
        if (!enableWander || rb == null) return;

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            wanderDirection *= -1f;
            wanderTimer = Random.Range(1f, wanderChangeInterval);
            if (spriteRenderer != null)
                spriteRenderer.flipX = wanderDirection < 0;
        }

        rb.velocity = new Vector2(wanderDirection * wanderSpeed, rb.velocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            isGrounded = false;
    }

    public bool IsPlayerInRange() => playerInRange;

    // Save/Load accessors
    public float WanderDirection => wanderDirection;
    public float WanderTimer => wanderTimer;
    public bool HasShownNightMessage => hasShownNightMessage;
    public bool FlipX => spriteRenderer != null && spriteRenderer.flipX;

    public void LoadState(float posX, float posY, float wDir, float wTimer, bool nightMsg, bool flipX)
    {
        transform.position = new Vector3(posX, posY, 0);
        wanderDirection = wDir;
        wanderTimer = wTimer;
        hasShownNightMessage = nightMsg;
        if (spriteRenderer != null) spriteRenderer.flipX = flipX;
    }
}
