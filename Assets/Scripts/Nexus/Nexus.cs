using System;
using System.Collections;
using UnityEngine;

public class Nexus : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealthPoints = 1_000_000f;
    [SerializeField] private GameObject nexusHealthBar;
    [SerializeField] private GameObject nexusModel;

    [Header("VFX")]
    [SerializeField] private ParticleSystem nexusExplosion;
    [SerializeField] private NexusVignette nexusVignette;

    [Header("Healing")]
    [SerializeField] private bool isHealing = false;
    [SerializeField] private float healingPerSecond = 10f;
    private Coroutine healingCoroutine;

    private bool isDying;

    private float healthPoints;
    public float HealthPointsNormalized() => healthPoints / maxHealthPoints;

    public bool IsDestroyed() => isDying || healthPoints <= 0f;

    public Transform Transform() => transform;

    public event Action<Nexus> OnDestroyed;
    public event Action<Nexus> OnHealthChanged;

    private void Awake()
    {
        healthPoints = maxHealthPoints;
        OnHealthChanged?.Invoke(this);

        nexusVignette.Initialize(this);

        healingCoroutine = StartCoroutine(HealingLoop());
    }

    public void TakeDamage(float damage, IEnemy attacker)
    {
        if (isDying) return;

        healthPoints -= damage;
        if (nexusHealthBar != null)
        {
            nexusHealthBar.SetActive(true);
        }

        OnHealthChanged?.Invoke(this);

        if (healthPoints <= 0f)
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        if (isDying) yield break;
        isDying = true;

        healthPoints = 0;
        OnDestroyed?.Invoke(this);

        nexusExplosion.Play(withChildren: true);

        yield return new WaitForSecondsRealtime(0.1f);

        Destroy(nexusModel);
        Destroy(nexusHealthBar);

        yield return new WaitForSecondsRealtime(2.1f);

        Destroy(gameObject);
    }

    public bool IsFullHealth => Mathf.Approximately(healthPoints, maxHealthPoints);

    public void MakeVolatile()
    {
        healthPoints = 1f;
        OnHealthChanged?.Invoke(this);
    }

    public void SetIsHealing(bool isHealing) => this.isHealing = isHealing;

    private IEnumerator HealingLoop()
    {
        var wait = new WaitForSecondsRealtime(1f);

        while (true)
        {
            yield return wait;

            if (isDying) continue;
            if (!isHealing) continue;
            if (IsFullHealth) continue;

            healthPoints = Mathf.Min(maxHealthPoints, healthPoints + healingPerSecond);
            OnHealthChanged?.Invoke(this);
        }
    }
}
