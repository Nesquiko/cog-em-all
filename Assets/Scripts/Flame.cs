using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Flame : MonoBehaviour
{
    [SerializeField] private float damagePerPulse = 20f;
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

    public void StartFlame()
    {
        if (isActive) return;
        isActive = true;

        if (meshRenderer != null) 
        {
            meshRenderer.enabled = true;  
        }
        
        fireRoutine = StartCoroutine(FlameRoutine());
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

    private IEnumerator FlameRoutine()
    {
        float duration = 0f;
        float tickTimer = 0f;

        while (duration < fireDuration)
        {
            duration += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if (tickTimer >= pulseInterval)
            {
                DealDamage();
                tickTimer = 0f;
            }

            yield return null;
        }

        if (meshRenderer != null) meshRenderer.enabled = false;
        isActive = false;
    }

    private void DealDamage()
    {
        if (owner == null) return;

        List<Enemy> enemiesInRange = owner.GetCurrentEnemiesInRange();
        if (enemiesInRange == null || enemiesInRange.Count == 0) return;

        foreach (Enemy enemy in enemiesInRange)
        {
            enemy.TakeDamage(damagePerPulse);
        }
    }
}
