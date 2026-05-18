using UnityEngine;

public enum EnemyState { Idle, Patrol, Chase, Attack, Hurt, Dead }

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHP = 10;
    public int currentHP;
    public int damage = 5;
    public float moveSpeed = 2f;
    public float detectionRange = 6f;
    public float attackRange = 1f;

    [Header("Hit Effects")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    public float flashDuration = 0.15f;
    public Color flashColor = Color.white;

    protected bool isKnockedBack;
    protected float knockbackTimer;

    [Header("Drops")]
    [SerializeField] protected GameObject itemDropPrefab;
    public ItemData[] dropItems;
    public int[] dropAmounts;
    [Range(0, 1)] public float[] dropChances;

    protected EnemyState currentState = EnemyState.Patrol;
    protected Transform playerTransform;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHP = maxHP;
    }

    protected virtual void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    protected virtual void Update()
    {
        if (currentState == EnemyState.Dead) return;

        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0)
                isKnockedBack = false;
        }

        switch (currentState)
        {
            case EnemyState.Patrol: OnPatrol(); break;
            case EnemyState.Chase: OnChase(); break;
            case EnemyState.Attack: OnAttack(); break;
        }

        UpdateStateTransitions();
    }

    protected abstract void OnPatrol();
    protected abstract void OnChase();
    protected abstract void OnAttack();

    protected virtual void UpdateStateTransitions()
    {
        if (playerTransform == null) return;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (currentState != EnemyState.Attack && distanceToPlayer <= attackRange)
            ChangeState(EnemyState.Attack);
        else if (currentState == EnemyState.Attack && distanceToPlayer > attackRange * 1.5f)
            ChangeState(EnemyState.Chase);
        else if (currentState == EnemyState.Patrol && distanceToPlayer <= detectionRange)
            ChangeState(EnemyState.Chase);
        else if (currentState == EnemyState.Chase && distanceToPlayer > detectionRange * 1.5f)
            ChangeState(EnemyState.Patrol);
    }

    protected void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }

    public virtual void TakeDamage(int dmg, Vector2 attackerPosition)
    {
        currentHP -= dmg;

        Vector2 knockbackDir = ((Vector2)transform.position - attackerPosition).normalized;
        rb.velocity = new Vector2(knockbackDir.x * knockbackForce, rb.velocity.y + knockbackForce * 0.5f);
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        if (spriteRenderer != null)
            StartCoroutine(HitFlash());

        if (currentHP <= 0)
            Die();
    }

    private System.Collections.IEnumerator HitFlash()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = Color.white;
    }

    protected virtual void Die()
    {
        ChangeState(EnemyState.Dead);
        DropItems();
        PlayerCurrency.Instance?.EarnFromEnemy(1);
        Destroy(gameObject, 0.5f);
    }

    private void DropItems()
    {
        if (itemDropPrefab == null) return;

        for (int i = 0; i < dropItems.Length && i < dropChances.Length && i < dropAmounts.Length; i++)
        {
            if (Random.value <= dropChances[i])
            {
                Vector3 dropPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 0.5f), 0);
                GameObject drop = Instantiate(itemDropPrefab, dropPos, Quaternion.identity);
                ItemDrop itemDrop = drop.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    itemDrop.itemData = dropItems[i];
                    itemDrop.amount = dropAmounts[i];
                }
            }
        }
    }
}
