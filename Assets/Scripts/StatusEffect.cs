using UnityEngine;

[System.Serializable]
public class StatusEffect
{
    public string effectName;
    public float duration;
    public float tickInterval;
    private float tickTimer;

    public bool IsExpired => duration <= 0f;

    public void Update(float deltaTime, PlayerStats target)
    {
        duration -= deltaTime;
        tickTimer -= deltaTime;
        if (tickTimer <= 0f && tickInterval > 0f)
        {
            tickTimer = tickInterval;
            OnTick(target);
        }
    }

    protected virtual void OnTick(PlayerStats target) { }
    public virtual void OnApply(PlayerStats target) { }
    public virtual void OnRemove(PlayerStats target) { }
}

public class PoisonEffect : StatusEffect
{
    public PoisonEffect() { effectName = "Poison"; duration = 10f; tickInterval = 1f; }
    protected override void OnTick(PlayerStats target) => target.TakeDamage(2);
}

public class BurnEffect : StatusEffect
{
    public BurnEffect() { effectName = "Burn"; duration = 5f; tickInterval = 0.5f; }
    protected override void OnTick(PlayerStats target) => target.TakeDamage(5);
}

public class SlowEffect : StatusEffect
{
    public SlowEffect() { effectName = "Slow"; duration = 3f; }
}

public class RegenEffect : StatusEffect
{
    public RegenEffect() { effectName = "Regen"; duration = 8f; tickInterval = 1f; }
    protected override void OnTick(PlayerStats target) => target.HealHP(2);
}
