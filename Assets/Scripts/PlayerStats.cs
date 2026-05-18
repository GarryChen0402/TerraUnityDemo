using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("HP")]
    public int maxHP = 100;
    public int currentHP;
    [SerializeField] private float hpRegenRate = 0f;

    [Header("MP")]
    public int maxMP = 20;
    public int currentMP;
    [SerializeField] private float mpRegenRate = 1f;

    [Header("Combat")]
    public int baseAttack = 5;
    public int defense = 0;

    [Header("iFrame")]
    [SerializeField] private float invincibilityDuration = 0.5f;
    private float invincibilityTimer;
    public bool IsInvincible => invincibilityTimer > 0f;

    public event Action<int> OnHPChanged;
    public event Action<int> OnMPChanged;
    public event Action OnPlayerDeath;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        currentHP = maxHP;
        currentMP = maxMP;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
            float alpha = Mathf.PingPong(Time.time * 10f, 1f);
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(1, 1, 1, 0.3f + alpha * 0.7f);
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        if (currentMP < maxMP)
        {
            currentMP = (int)Mathf.Min(maxMP, currentMP + mpRegenRate * Time.deltaTime);
            OnMPChanged?.Invoke((int)currentMP);
        }

        if (currentHP < maxHP && hpRegenRate > 0)
        {
            currentHP = Mathf.Min(maxHP, currentHP + Mathf.RoundToInt(hpRegenRate * Time.deltaTime));
            OnHPChanged?.Invoke(currentHP);
        }
    }

    public void TakeDamage(int rawDamage)
    {
        if (IsInvincible) return;

        float randomMultiplier = UnityEngine.Random.Range(0.85f, 1.15f);
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(rawDamage * randomMultiplier) - defense);

        currentHP = Mathf.Max(0, currentHP - finalDamage);
        invincibilityTimer = invincibilityDuration;

        OnHPChanged?.Invoke(currentHP);
        Debug.Log($"Took {finalDamage} damage! HP: {currentHP}");

        if (currentHP <= 0)
        {
            OnPlayerDeath?.Invoke();
            Debug.Log("Player died!");
        }
    }

    public void HealHP(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        OnHPChanged?.Invoke(currentHP);
    }

    public bool ConsumeMP(int amount)
    {
        if (currentMP < amount) return false;
        currentMP -= amount;
        OnMPChanged?.Invoke(currentMP);
        return true;
    }

    public void IncreaseMaxHP(int amount)
    {
        maxHP = Mathf.Min(400, maxHP + amount);
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        OnHPChanged?.Invoke(currentHP);
    }

    public void IncreaseMaxMP(int amount)
    {
        maxMP = Mathf.Min(200, maxMP + amount);
        currentMP = Mathf.Min(currentMP + amount, maxMP);
        OnMPChanged?.Invoke(currentMP);
    }

    public void LoadState(int curHP, int curMP, int mHP, int mMP, int atk, int def)
    {
        currentHP = curHP;
        currentMP = curMP;
        maxHP = mHP;
        maxMP = mMP;
        baseAttack = atk;
        defense = def;
    }
}
