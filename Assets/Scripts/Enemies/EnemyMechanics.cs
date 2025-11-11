using System;
using UnityEngine.Assertions;
using UnityEngine.Splines;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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
    float Speed { get; set; }

    Transform Transform { get; }

    event Action<IEnemy> OnDeath;

    void Initialize(SplineContainer path, float startT, float lateralOffset, Action<IEnemy> onDeath);
    void TakeDamage(float damage, bool isCritical = false, EnemyStatusEffect effect = null);
    void EnterAttackRange(Nexus nexus);
    void ExitAttackRange(Nexus nexus);
    int GetInstanceID();
}

public enum EffectType
{
    Burning,
    Bleeding,
    Slowed,
    Accelerated,
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

    public static EnemyStatusEffect Accelerate(float duration) =>
        new(
            type: EffectType.Accelerated,
            duration: duration,
            tickDamage: 0f,
            tickInterval: 0f,
            speedMultiplier: 1.20f
        );
}

public static class EnemyMechanics
{
    #region Movement

    public static void FollowPath(
        Transform enemy,
        SplineContainer path,
        ref float t,
        float pathLength,
        ref float speed,
        float lateralOffset
    )
    {
        if (path == null || pathLength <= 0f) return;

        t = Mathf.Repeat(t + speed / pathLength * Time.deltaTime, 1f);

        Vector3 position = path.EvaluatePosition(0, t);
        Vector3 tangent = path.EvaluateTangent(0, t);
        Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

        enemy.transform.SetPositionAndRotation(
            position + right * lateralOffset,
            Quaternion.LookRotation(tangent)
        );
    }

    public static float CalculatePathLength(SplineContainer path) => path != null ? path.CalculateLength() : 0f;

    #endregion

    #region Attack

    private static IEnumerator AttackAnimation(
        MonoBehaviour enemy,
        Nexus targetNexus,
        float attackDamage,
        float jumpHeight,
        float forwardDistance,
        float duration
    )
    {
        Vector3 start = enemy.transform.position;
        Vector3 targetPosition = targetNexus.transform.position;
        Vector3 direction = (targetPosition - start);
        direction.y = 0f;
        direction.Normalize();
        Vector3 apex = start + direction * forwardDistance + Vector3.up * jumpHeight;

        float half = duration * 0.5f;

        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float n = t / half;
            enemy.transform.position = Vector3.Lerp(start, apex, n);
            yield return null;
        }

        bool hit = false;
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float n = t / half;
            enemy.transform.position = Vector3.Lerp(apex, start, n);

            if (!hit && n > 0.3f)
            {
                if (targetNexus != null)
                    targetNexus.TakeDamage(attackDamage);
                hit = true;
            }
            yield return null;
        }

        enemy.transform.position = start;
    }

    public static void AttackNexus(
        MonoBehaviour enemy,
        Nexus targetNexus,
        ref float cooldown,
        float attackRate,
        float attackDamage,
        float jumpHeight,
        float forwardDistance,
        float animationDuration,
        ref Coroutine refRoutine
    )
    {
        if (targetNexus == null) return;
        enemy.transform.LookAt(targetNexus.transform, Vector3.up);
        cooldown -= Time.deltaTime;

        if (cooldown <= 0f)
        {
            refRoutine = enemy.StartCoroutine(
                AttackAnimation(
                    enemy,
                    targetNexus,
                    attackDamage,
                    jumpHeight,
                    forwardDistance,
                    animationDuration)
            );
            cooldown = attackRate;
        }
    }

    #endregion

    #region Damage & Status Effects

    public static void ApplyEffect(
        MonoBehaviour enemy,
        IDictionary<EffectType, Coroutine> active,
        EnemyStatusEffect effect,
        Action<float> takeDamageAction,
        Action<float> setSpeedAction,
        float originalSpeed
    )
    {
        if (active == null) return;

        if (active.TryGetValue(effect.type, out var c))
            enemy.StopCoroutine(c);

        Coroutine routine = enemy.StartCoroutine(
            HandleEffect(
                effect,
                takeDamageAction,
                setSpeedAction,
                originalSpeed
            )
        );
        active[effect.type] = routine;
    }

    private static IEnumerator HandleEffect(
        EnemyStatusEffect effect,
        Action<float> takeDamageAction,
        Action<float> setSpeedAction,
        float originalSpeed
    )
    {
        float elapsed = 0f;
        switch (effect.type)
        {
            case EffectType.Burning:
            case EffectType.Bleeding:
                while (elapsed < effect.duration)
                {
                    takeDamageAction?.Invoke(effect.tickDamage);
                    yield return new WaitForSeconds(effect.tickInterval);
                    elapsed += effect.tickInterval;
                }
                break;
            case EffectType.Slowed:
                setSpeedAction?.Invoke(originalSpeed * effect.speedMultiplier);
                yield return new WaitForSeconds(effect.duration);
                setSpeedAction?.Invoke(originalSpeed);
                break;
        }
    }

    #endregion

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
