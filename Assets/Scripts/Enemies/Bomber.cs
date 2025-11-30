using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

[RequireComponent(typeof(EnemyBehaviour))]
public class Bomber : MonoBehaviour, IEnemy
{
    private EnemyBehaviour behaviour;

    // IEnemy fields
    public EnemyType Type => EnemyType.Bomber;
    public event Action<IEnemy> OnDeath;
    public int OnKillGearsReward => behaviour.OnKillGearsReward;
    public float HealthPointsNormalized => behaviour.HealthPointsNormalized;
    public float Speed { get => behaviour.Speed; set => behaviour.Speed = value; }
    public Transform Transform => transform;

    private void Awake()
    {
        behaviour = GetComponent<EnemyBehaviour>();
        Assert.IsNotNull(behaviour);
        behaviour.OnSuicide += Explode;
    }

    private void Explode(IDamageable target)
    {
        target.TakeDamage(behaviour.AttackDamage, this);
    }

    // IEnemy functions
    public void Initialize(SplineContainer pathContainer, float startT, float lateralOffset, Action<IEnemy> onDeath)
    {
        behaviour.SetSpline(pathContainer, startT, lateralOffset);
        behaviour.OnDeath += () =>
        {
            onDeath(this);
            OnDeath?.Invoke(this);
        };
    }

    public void TakeDamage(float damage, DamageSourceType sourceType, bool isCritical = false, EnemyStatusEffect effect = null)
    {
        behaviour.TakeDamage(damage, sourceType, isCritical, effect);
    }

    public void EnterAttackRange(IDamageable damageable)
    {
        behaviour.EnterAttackRange(damageable);
    }

    public void ExitAttackRange(IDamageable damageable)
    {
        behaviour.ExitAttackRange(damageable);
    }

    public void ApplyEffect(EnemyStatusEffect effect)
    {
        behaviour.ApplyEffect(effect);
    }

    public void RemoveEffect(EffectType type)
    {
        behaviour.RemoveEffect(type);
    }
}
