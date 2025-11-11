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
    }

    public void TakeDamage(float damage)
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
}
