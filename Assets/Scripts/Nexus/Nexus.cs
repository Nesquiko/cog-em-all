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
    public event Action<Nexus, float> OnHealthChanged;

    private void Awake()
    {
        healthPoints = maxHealthPoints;
        OnHealthChanged?.Invoke(this, 0);

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

        SoundManagersDontDestroy.GerOrCreate()?.SoundFX.PlaySoundFXClip(SoundFXType.NexusHit, transform);

        OnHealthChanged?.Invoke(this, damage);

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

        nexusExplosion.Play(withChildren: true);
        CinemachineShake.Instance.Shake(ShakeIntensity.Extreme, ShakeLength.Long);
        SoundManagersDontDestroy.GerOrCreate()?.SoundFX.PlaySoundFXClip(SoundFXType.BigExplosion, transform);

        OnDestroyed?.Invoke(this);

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
        OnHealthChanged?.Invoke(this, 0);
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
            OnHealthChanged?.Invoke(this, healingPerSecond);
        }
    }
}
