using System;
using UnityEngine;

public enum EffectType
{
    Burning,
    Bleeding,
    Slowed,
    Oiled,
    OilBurned,
}

[Serializable]
public class EnemyStatusEffect
{
    public EffectType type;
    public float duration;
    public float tickDamage;
    public float tickInterval = 1f;
    public float speedMultiplier = 1f;
    public bool persistent = false;

    public EnemyStatusEffect() { }

    public EnemyStatusEffect(
        EffectType type,
        float duration,
        float tickDamage,
        float tickInterval = 1f,
        float speedMultiplier = 1f,
        bool persistent = false
    )
    {
        this.type = type;
        this.duration = duration;
        this.tickDamage = tickDamage;
        this.tickInterval = tickInterval;
        this.speedMultiplier = speedMultiplier;
        this.persistent = persistent;
    }

    public static EnemyStatusEffect Burn => 
        new(
            type: EffectType.Burning,
            duration: 5f,
            tickDamage: 5f,
            tickInterval: 0.5f
        );

    public static EnemyStatusEffect Bleed =>
        new(
            type: EffectType.Bleeding,
            duration: 4f,
            tickDamage: 2.5f,
            tickInterval: 0.5f
        );

    public static EnemyStatusEffect Slow => new(
            type: EffectType.Slowed,
            duration: 3f,
            tickDamage: 0f,
            tickInterval: 0f,
            speedMultiplier: 0.5f
        );

    public static EnemyStatusEffect Oiled(
        float speedMultiplier
    ) => new(
            type: EffectType.Oiled,
            duration: Mathf.Infinity,
            tickDamage: 0f,
            tickInterval: 0f,
            speedMultiplier: speedMultiplier,
            persistent: true
        );

    public static EnemyStatusEffect OilBurn(
        float tickDamage = 5f,
        float tickInterval = 0.5f
    ) => new(
            type: EffectType.OilBurned,
            duration: Mathf.Infinity,
            tickDamage: tickDamage,
            tickInterval: tickInterval,
            persistent: true
        );
}
