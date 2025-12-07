using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

[RequireComponent(typeof(EnemyBehaviour))]
public class Bomber : MonoBehaviour, IEnemy
{
    private EnemyBehaviour behaviour;

    [Header("Dash")]
    [SerializeField] private float dashCooldown = 5f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashSpeedMultiplier = 4f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem dashVFX;

    [SerializeField] private Renderer[] highlightRenderers;

    // IEnemy fields
    public EnemyType Type => EnemyType.Bomber;
    public event Action<IEnemy> OnDeath;
    public int OnKillGearsReward => behaviour.OnKillGearsReward;
    public float HealthPointsNormalized => behaviour.HealthPointsNormalized;
    public float Speed { get => behaviour.Speed; set => behaviour.Speed = value; }
    public Transform Transform => transform;
    public bool Marked
    {
        get => behaviour.Marked;
    }

    private bool isDashing;

    private void Awake()
    {
        behaviour = GetComponent<EnemyBehaviour>();
        Assert.IsNotNull(behaviour);
        behaviour.OnSuicide += Explode;
    }

    private void Start()
    {
        StartCoroutine(DashLoop());
    }

    private IEnumerator DashLoop()
    {
        float firstDelay = UnityEngine.Random.Range(1f, dashCooldown);
        yield return new WaitForSeconds(firstDelay);

        while (behaviour.HealthPoints > 0)
        {
            if (behaviour.BuffsDisabled)
            {
                yield return null;
                continue;
            }

            StartCoroutine(Dash());

            float elapsed = 0f;
            while (elapsed < dashCooldown)
            {
                if (behaviour.BuffsDisabled)
                    break;

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    private IEnumerator Dash()
    {
        if (isDashing) yield break;
        isDashing = true;
        float dashSpeed = behaviour.OriginalSpeed * dashSpeedMultiplier;
        behaviour.Speed = dashSpeed;

        dashVFX.Play();

        float t = 0f;
        while (t < dashDuration)
        {
            if (behaviour.BuffsDisabled)
                break;
            t += Time.deltaTime;
            yield return null;
        }

        behaviour.Speed = behaviour.OriginalSpeed;
        isDashing = false;
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

    public void ApplyHighlight(bool apply)
    {
        Debug.Log($"Applying highlight to {Type}, {apply}");
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
