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

    [SerializeField] private MeshRenderer meshRenderer;

    private Coroutine fireRoutine;
    private bool isActive;
    private float range = 50f;

    private FlamethrowerTower owner;

    public float FireDuration => fireDuration;

    public void SetRange(float flameRange)
    {
        range = flameRange;
        transform.localScale = new Vector3(range, range, range);
    }

    public void SetOwner(FlamethrowerTower tower)
    {
        owner = tower;
    }

    public void StartFlame(Func<float, float> CalculateBaseFlameDamagePerPulse)
    {
        if (isActive) return;
        isActive = true;

        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }

        fireRoutine = StartCoroutine(FlameRoutine(CalculateBaseFlameDamagePerPulse));
    }

    public void StopFlame()
    {
        if (!isActive) return;
        isActive = false;
        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
        }
    }

    private IEnumerator FlameRoutine(Func<float, float> CalculateBaseFlameDamagePerPulse)
    {
        float duration = 0f;
        float tickTimer = 0f;

        while (duration < fireDuration)
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

        if (meshRenderer != null) meshRenderer.enabled = false;
        isActive = false;
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
            enemy.TakeDamage(damage, isCritical, effect: EnemyStatusEffect.Burn);
        }
    }
}
