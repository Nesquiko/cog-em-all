using System;

[Serializable]
public class EnemyStatusEffect
{
    public EffectType type;
    public float duration;
    public float tickDamage;
    public float tickInterval = 1f;
    public float speedMultiplier = 1f;

    public EnemyStatusEffect() { }

    public EnemyStatusEffect(
        EffectType type,
        float duration,
        float tickDamage,
        float tickInterval = 1f,
        float speedMultiplier = 1f
    )
    {
        this.type = type;
        this.duration = duration;
        this.tickDamage = tickDamage;
        this.tickInterval = tickInterval;
        this.speedMultiplier = speedMultiplier;
    }

    public static EnemyStatusEffect Burn =>
        new(
            EffectType.Burning,
            duration: 5f,
            tickDamage: 5f,
            tickInterval: 0.5f
        );

    public static EnemyStatusEffect Bleed =>
        new(
            EffectType.Bleeding,
            duration: 4f,
            tickDamage: 2.5f,
            tickInterval: 0.5f
        );

    public static EnemyStatusEffect Slow =>
        new(
            EffectType.Slowed,
            duration: 3f,
            tickDamage: 0f,
            tickInterval: 0f,
            speedMultiplier: 0.5f
        );
}

public enum EffectType
{
    Burning,
    Bleeding,
    Slowed
}
