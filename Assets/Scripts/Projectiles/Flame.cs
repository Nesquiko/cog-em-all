using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flame : MonoBehaviour, IDamageSource
{
    [Header("VFX")]
    [SerializeField] private ParticleSystem flameVFX;
    [SerializeField] private float burnDelay = 0.5f;
    [SerializeField] private float flameVFXScaleFactor = 7f;

    private Coroutine fireRoutine;
    private bool isActive;
    private float range = 50f;

    private readonly HashSet<OilSpill> oilsInRange = new();

    private FlamethrowerTower owner;

    public bool IsActive => isActive;

    public DamageSourceType Type() => DamageSourceType.Flame;

    public void Initialize(FlamethrowerTower ownerTower, float flameRange)
    {
        owner = ownerTower;
        range = flameRange;
        transform.localScale = new(range, range, range);
        flameVFX.transform.localScale = new(range / flameVFXScaleFactor, range / flameVFXScaleFactor, range / flameVFXScaleFactor);
    }

    public void UpdateRange(float newRange)
    {
        range = newRange;
        transform.localScale = new(newRange, newRange, newRange);
        flameVFX.transform.localScale = new(newRange / flameVFXScaleFactor, newRange / flameVFXScaleFactor, newRange / flameVFXScaleFactor);
    }

    public void StartFlame(Func<float, float> CalculateBaseFlameDamagePerPulse)
    {
        if (isActive)
            return;

        isActive = true;

        flameVFX.Play();

        if (fireRoutine != null)
            StopCoroutine(fireRoutine);

        fireRoutine = StartCoroutine(FlameRoutine(CalculateBaseFlameDamagePerPulse));

        foreach (var oil in oilsInRange)
            if (oil != null)
                oil.Ignite();
    }

    public void StopFlame()
    {
        if (!isActive)
            return;

        isActive = false;

        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;
        }

        flameVFX.Stop(withChildren: true);

        foreach (var oil in oilsInRange)
            if (oil != null)
                oil.Extinguish();
    }

    private IEnumerator FlameRoutine(Func<float, float> CalculateBaseFlameDamagePerPulse)
    {
        yield return new WaitForSeconds(burnDelay);

        float tickTimer = 0f;

        while (isActive)
        {
            tickTimer += Time.deltaTime;

            if (tickTimer >= owner.FlamePulseInterval)
            {
                DealDamage(CalculateBaseFlameDamagePerPulse?.Invoke(owner.DamagePerPulse) ?? owner.DamagePerPulse);
                tickTimer = 0f;
            }

            yield return null;
        }

        fireRoutine = null;
    }

    private void DealDamage(float baseDamagePerPulse)
    {
        if (owner == null) return;

        var enemiesInRange = owner.GetCurrentEnemiesInRange();
        if (enemiesInRange == null || enemiesInRange.Count == 0) return;

        float critChance = owner.CritChance;
        float critMultiplier = owner.CritMultiplier;

        foreach (var enemy in enemiesInRange)
        {
            bool isCritical = UnityEngine.Random.value < critChance;
            float damage = isCritical ? baseDamagePerPulse * critMultiplier : baseDamagePerPulse;
            enemy.TakeDamage(damage, Type(), isCritical, effect: owner.BurnOnHitActive ? EnemyStatusEffect.Burn : null);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<OilSpillTrigger>(out var oilTrigger)) return;
        OilSpill oil = oilTrigger.GetComponentInParent<OilSpill>();
        if (oil == null) return;
        oilsInRange.Add(oil);

        if (isActive)
            oil.Ignite();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<OilSpillTrigger>(out var oilTrigger)) return;
        OilSpill oil = oilTrigger.GetComponentInParent<OilSpill>();
        oilsInRange.Remove(oil);

        if (isActive && oil != null)
            oil.Extinguish();
    }
}
