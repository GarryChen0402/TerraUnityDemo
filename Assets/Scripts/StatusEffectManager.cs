using UnityEngine;
using System.Collections.Generic;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance { get; private set; }

    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    private PlayerStats playerStats;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].Update(Time.deltaTime, playerStats);
            if (activeEffects[i].IsExpired)
            {
                activeEffects[i].OnRemove(playerStats);
                activeEffects.RemoveAt(i);
            }
        }
    }

    public void ApplyEffect(StatusEffect effect)
    {
        var existing = activeEffects.Find(e => e.effectName == effect.effectName);
        if (existing != null)
            existing.duration = effect.duration;
        else
        {
            effect.OnApply(playerStats);
            activeEffects.Add(effect);
        }
    }

    public bool HasEffect(string effectName)
    {
        return activeEffects.Exists(e => e.effectName == effectName);
    }
}
