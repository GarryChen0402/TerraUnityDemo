using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Melee Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Attack Cooldown")]
    [SerializeField] private float attackCooldown = 0.5f;
    private float lastAttackTime;

    [Header("Crit")]
    [Range(0, 1)] public float critChance = 0.04f;
    public float critMultiplier = 2.0f;

    private PlayerStats playerStats;
    private PlayerEquipment equipment;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        equipment = GetComponent<PlayerEquipment>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time - lastAttackTime >= attackCooldown)
        {
            MeleeAttack();
        }
    }

    private void MeleeAttack()
    {
        lastAttackTime = Time.time;

        int weaponDamage = equipment?.weapon?.damage ?? 5;
        int baseDamage = playerStats.baseAttack + weaponDamage;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position, attackRadius, enemyLayer);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            float randomMultiplier = Random.Range(0.85f, 1.15f);
            bool isCrit = Random.value < critChance;
            float critMod = isCrit ? critMultiplier : 1f;
            int finalDamage = Mathf.RoundToInt(baseDamage * randomMultiplier * critMod);

            enemyCollider.GetComponent<EnemyBase>()?.TakeDamage(finalDamage);
            Debug.Log($"{(isCrit ? "CRIT! " : "")}Hit {enemyCollider.name} for {finalDamage} damage");
        }
    }

    public void MagicAttack(int spellDamage, int mpCost)
    {
        if (!playerStats.ConsumeMP(mpCost))
        {
            Debug.Log("Not enough MP!");
            return;
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position, attackRadius * 2f, enemyLayer);
        foreach (Collider2D hit in hitEnemies)
            hit.GetComponent<EnemyBase>()?.TakeDamage(spellDamage);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
