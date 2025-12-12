using System;
using UnityEngine.Splines;
using UnityEngine;


public enum EnemyType
{
    Bandit = 0,
    Dreadnought = 1,
    Bomber = 2,
}

public enum EnemyAttributes
{
    MovementSpeed = 0,
    Health = 1
}

public interface IEnemy
{
    EnemyType Type { get; }
    int OnKillGearsReward { get; }
    float HealthPointsNormalized { get; }
    float HealthPoints { get; }
    float Speed { get; set; }

    Transform Transform { get; }

    event Action<IEnemy> OnDeath;

    void Initialize(SplineContainer path, float startT, float lateralOffset, Action<IEnemy> onDeath);
    void TakeDamage(float damage, DamageSourceType sourceType, bool isCritical = false, EnemyStatusEffect effect = null);
    void EnterAttackRange(IDamageable damageable);
    void ExitAttackRange(IDamageable damageable);
    int GetInstanceID();
    void ApplyEffect(EnemyStatusEffect effect);
    void RemoveEffect(EffectType type);
    void ApplyHighlight(bool apply);
    bool Marked { get; }
    void Mark();
    void Unmark();

    void DEV_MakeUnkillable();
}

public enum EffectType
{
    Burning,
    Bleeding,
    Slowed,
    Accelerated,
    Oiled,
    OilBurned,
    DebrisSlowed,
    ArmorShredded,
    Frozen,
    DisabledBuffs,
    Stunned,
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
    public bool negative = true;
    public bool stackable = false;
    public int maxStacks = 1;
    public float damageMultiplierPerStack = 0f;

    public EnemyStatusEffect() { }

    public EnemyStatusEffect(
        EffectType type,
        float duration,
        float tickDamage,
        float tickInterval = 1f,
        float speedMultiplier = 1f,
        bool persistent = false,
        bool negative = true,
        bool stackable = false,
        int maxStacks = 1,
        float damageMultiplierPerStack = 0f
    )
    {
        this.type = type;
        this.duration = duration;
        this.tickDamage = tickDamage;
        this.tickInterval = tickInterval;
        this.speedMultiplier = speedMultiplier;
        this.persistent = persistent;
        this.negative = negative;
        this.stackable = stackable;
        this.maxStacks = maxStacks;
        this.damageMultiplierPerStack = damageMultiplierPerStack;
    }

    public static bool IsNegative(EffectType type)
    {
        return type switch
        {
            EffectType.Accelerated => false,
            _ => true,
        };
    }

    public const float BurnDefaultDuration = 5f;

    public static EnemyStatusEffect Burn(float duration = BurnDefaultDuration) => new(
        type: EffectType.Burning,
        duration: duration,
        tickDamage: 5f,
        tickInterval: 0.5f,
        persistent: false,
        negative: true
    );

    public const float BleedDefaultDuration = 4f;
    public static EnemyStatusEffect Bleed(float duration = BleedDefaultDuration) => new(
        type: EffectType.Bleeding,
        duration: duration,
        tickDamage: 2.5f,
        tickInterval: 0.5f,
        persistent: false,
        negative: true
    );

    public static EnemyStatusEffect Slow => new(
        type: EffectType.Slowed,
        duration: 3f,
        tickDamage: 0f,
        tickInterval: 0f,
        speedMultiplier: 0.5f,
        persistent: false,
        negative: true
    );

    public static EnemyStatusEffect Accelerate(
        float duration
    ) => new(
        type: EffectType.Accelerated,
        duration: duration,
        tickDamage: 0f,
        tickInterval: 0f,
        speedMultiplier: 1.5f,
        persistent: false,
        negative: false
    );

    public static EnemyStatusEffect Oiled(
       float speedMultiplier
    ) => new(
        type: EffectType.Oiled,
        duration: Mathf.Infinity,
        tickDamage: 0f,
        tickInterval: 0f,
        speedMultiplier: speedMultiplier,
        persistent: true,
        negative: true
    );

    public static EnemyStatusEffect OilBurn(
        float tickDamage = 5f,
        float tickInterval = 0.5f
    ) => new(
        type: EffectType.OilBurned,
        duration: Mathf.Infinity,
        tickDamage: tickDamage,
        tickInterval: tickInterval,
        persistent: true,
        negative: true
    );

    public static EnemyStatusEffect DebrisSlow(
        float speedMultiplier = 0.5f
    ) => new(
        type: EffectType.DebrisSlowed,
        duration: Mathf.Infinity,
        tickDamage: 0f,
        tickInterval: 0f,
        speedMultiplier: speedMultiplier,
        persistent: true,
        negative: true
    );

    public const int ArmorShredDefaultMaxStacks = 2;
    public const float DamageMultiplerPerStack = .05f;
    public static EnemyStatusEffect ArmorShred(int maxStacks = ArmorShredDefaultMaxStacks) => new(
        type: EffectType.ArmorShredded,
        duration: 5f,
        tickDamage: 0f,
        tickInterval: 0f,
        persistent: false,
        negative: true,
        stackable: true,
        maxStacks: ArmorShredDefaultMaxStacks,
        damageMultiplierPerStack: DamageMultiplerPerStack
    );

    public static EnemyStatusEffect Freeze => new(
        type: EffectType.Frozen,
        duration: Mathf.Infinity,
        tickDamage: 0f,
        tickInterval: 0f,
        speedMultiplier: 0f,
        persistent: true,
        negative: true
    );

    public static EnemyStatusEffect DisableBuffs => new(
        type: EffectType.DisabledBuffs,
        duration: Mathf.Infinity,
        tickDamage: 0f,
        tickInterval: 0f,
        speedMultiplier: 1f,
        persistent: true,
        negative: true
    );

    public static EnemyStatusEffect Stun => new(
        type: EffectType.Stunned,
        duration: 3f,
        tickDamage: 0f,
        tickInterval: 0f,
        speedMultiplier: 0f,
        persistent: false,
        negative: true
    );
}
