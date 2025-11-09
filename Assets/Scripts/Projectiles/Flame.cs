using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flame : MonoBehaviour
{
    [SerializeField] private float damagePerPulse = 20f;
    public float DamagePerPulse => damagePerPulse;
    [SerializeField] private float pulseInterval = 0.25f;
    [SerializeField] private float fireDuration = 3f;

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
    public float FireDuration => fireDuration;

    public void Initialize(FlamethrowerTower ownerTower, float flameRange)
    {
        owner = ownerTower;
        range = flameRange;
        transform.localScale = new(range, range, range);
        flameVFX.transform.localScale = new(range / flameVFXScaleFactor, range / flameVFXScaleFactor, range / flameVFXScaleFactor);
        var main = flameVFX.main;
        main.duration = Mathf.Max(0f, fireDuration - 1f);
    }

    public void StartFlame(Func<float, float> CalculateBaseFlameDamagePerPulse)
    {
        if (isActive) return;
        isActive = true;

        flameVFX.Play();

        fireRoutine = StartCoroutine(FlameRoutine(CalculateBaseFlameDamagePerPulse));

        foreach (var oil in oilsInRange)
            if (oil != null) oil.Ignite();
    }

    public void StopFlame()
    {
        if (!isActive) return;
        isActive = false;
        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
        }

        flameVFX.Stop(withChildren: true, stopBehavior: ParticleSystemStopBehavior.StopEmittingAndClear);

        foreach (var oil in oilsInRange)
            if (oil != null) oil.Extinguish();
    }

    private IEnumerator FlameRoutine(Func<float, float> CalculateBaseFlameDamagePerPulse)
    {
        yield return new WaitForSeconds(burnDelay);

        float duration = 0f;
        float tickTimer = 0f;

        var runTime = fireDuration - burnDelay;
        while (duration < runTime)
        {
            duration += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if (tickTimer >= pulseInterval)
            {
                DealDamage(CalculateBaseFlameDamagePerPulse?.Invoke(damagePerPulse) ?? damagePerPulse);
                tickTimer = 0f;
            }

            yield return null;
        }

        isActive = false;
    }

    private void DealDamage(float baseDamagePerPulse)
    {
        if (owner == null) return;

        List<Enemy> enemiesInRange = owner.GetCurrentEnemiesInRange();
        if (enemiesInRange == null || enemiesInRange.Count == 0) return;

        float critChance = owner.CritChance;
        float critMultiplier = owner.CritMultiplier;

        foreach (Enemy enemy in enemiesInRange)
        {
            bool isCritical = UnityEngine.Random.value < critChance;
            float damage = isCritical ? baseDamagePerPulse * critMultiplier : baseDamagePerPulse;
            enemy.TakeDamage(damage, isCritical, withEffect: EnemyStatusEffect.Burn);
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
