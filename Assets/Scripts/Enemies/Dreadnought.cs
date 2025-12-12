using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;


[RequireComponent(typeof(EnemyBehaviour))]
public class Dreadnought : MonoBehaviour, IEnemy
{
    private EnemyBehaviour behaviour;

    [Header("Shield")]
    [SerializeField, Range(0.01f, 0.3f)] private float shieldHealthFraction = 0.1f;
    [SerializeField] private float shieldCooldown = 10f;
    private float shieldHealthPoints;
    private float nextShieldTimer;
    private bool shieldActive;

    [Header("VFX")]
    [SerializeField] private ParticleSystem shieldVFX;

    [SerializeField] private Renderer[] highlightRenderers;

    // IEnemy fields
    public EnemyType Type => EnemyType.Dreadnought;
    public event Action<IEnemy> OnDeath;
    public int OnKillGearsReward => behaviour.OnKillGearsReward;
    public float HealthPointsNormalized => behaviour.HealthPointsNormalized;
    public float HealthPoints => behaviour.HealthPoints;
    public float Speed { get => behaviour.Speed; set => behaviour.Speed = value; }
    public Transform Transform => transform;
    public bool Marked
    {
        get => behaviour.Marked;
    }

    private void Awake()
    {
        behaviour = GetComponent<EnemyBehaviour>();
        Assert.IsNotNull(behaviour);
    }

    private void Start()
    {
        ActivateShield();
    }

    private void Update()
    {
        if (behaviour.BuffsDisabled)
        {
            if (shieldActive)
                BreakShield();
            return;
        }

        if (!shieldActive)
        {
            nextShieldTimer += Time.deltaTime;
            if (nextShieldTimer >= shieldCooldown)
            {
                ActivateShield();
                nextShieldTimer = 0f;
            }
        }
    }

    private void ActivateShield()
    {
        if (behaviour.BuffsDisabled) return;
        shieldHealthPoints = behaviour.MaxHealthPoints * shieldHealthFraction;
        shieldActive = true;
        shieldVFX.Play();
    }

    private void BreakShield()
    {
        shieldActive = false;
        nextShieldTimer = 0f;
        shieldVFX.Stop(withChildren: true);
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

    public void DEV_MakeUnkillable()
    {
        behaviour.DEV_isUnkillable = true;
    }

    public void TakeDamage(float damage, DamageSourceType sourceType, bool isCritical = false, EnemyStatusEffect effect = null)
    {
        if (shieldActive)
        {
            shieldHealthPoints -= damage;
            if (shieldHealthPoints <= 0f)
            {
                BreakShield();
            }
            return;
        }

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

        if (effect.type == EffectType.DisabledBuffs)
        {
            if (shieldActive)
                BreakShield();

            nextShieldTimer = 0f;
        }
    }

    public void RemoveEffect(EffectType type)
    {
        behaviour.RemoveEffect(type);

        if (type == EffectType.DisabledBuffs)
            nextShieldTimer = 0f;
    }

    public void ApplyHighlight(bool apply)
    {
        if (apply)
            behaviour.ApplyHighlight(highlightRenderers);
        else
            behaviour.ClearHighlight(highlightRenderers);
    }

    public void Mark()
    {
        behaviour.Mark();
    }

    public void Unmark()
    {
        behaviour.Unmark();
    }
}
