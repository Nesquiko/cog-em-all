using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

[RequireComponent(typeof(EnemyBehaviour))]
public class Bandit : MonoBehaviour, IEnemy
{
    private EnemyBehaviour behaviour;

    [Header("Leadership")]
    [SerializeField, Range(0f, 1f)] private float leaderChance = 0.1f;
    [SerializeField, Range(5f, 25f)] private float battlecryRange = 15f;
    [SerializeField] private float battlecryDuration = 3f;
    [SerializeField] private float battlecryCooldown = 15f;
    [SerializeField] private float vfxFactor = 7.5f;
    [SerializeField] private LayerMask enemyMask;

    [Header("VFX")]
    [SerializeField] private ParticleSystem leaderBattlecryVFX;

    [SerializeField] private Renderer[] highlightRenderers;

    private bool isLeader;

    // IEnemy fields
    public EnemyType Type => EnemyType.Bandit;
    public event Action<IEnemy> OnDeath;
    public int OnKillGearsReward => behaviour.OnKillGearsReward;
    public float HealthPointsNormalized => behaviour.HealthPointsNormalized;
    public float HealthPoints => behaviour.HealthPoints;
    public float Speed { get => behaviour.Speed; set => behaviour.Speed = value; }
    public Transform Transform => transform;

    public bool Marked => behaviour.Marked;

    public int SpawnedInWave { get; set; }

    private void Awake()
    {
        behaviour = GetComponent<EnemyBehaviour>();
        Assert.IsNotNull(behaviour);
        isLeader = UnityEngine.Random.value <= leaderChance;

        var main = leaderBattlecryVFX.main;
        main.duration = battlecryDuration;
        main.loop = false;
        main.startLifetime = battlecryDuration;
        leaderBattlecryVFX.gameObject.transform.localScale = new(
            battlecryRange / vfxFactor,
            battlecryRange / vfxFactor,
            battlecryRange / vfxFactor
        );
    }

    private void Start()
    {
        if (isLeader)
        {
            StartCoroutine(Leadership());
        }
    }

    private IEnumerator Leadership()
    {
        while (behaviour.HealthPoints > 0)
        {
            for (float t = 0f; t < battlecryCooldown; t += Time.deltaTime)
            {
                if (behaviour.BuffsDisabled)
                    break;
                yield return null;
            }

            if (behaviour.BuffsDisabled)
            {
                if (leaderBattlecryVFX.isPlaying)
                    leaderBattlecryVFX.Stop(withChildren: true);
                yield return null;
                continue;
            }

            leaderBattlecryVFX.Play();
            Battlecry();

            for (float t = 0f; t < battlecryDuration; t += Time.deltaTime)
            {
                if (behaviour.BuffsDisabled)
                    break;
                yield return null;
            }

            leaderBattlecryVFX.Stop(withChildren: true);
        }
    }

    private void Battlecry()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            battlecryRange / 2f,
            enemyMask
        );

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent<Bandit>(out var _)) return;
            if (!hit.TryGetComponent<EnemyBehaviour>(out var allyStats)) return;

            allyStats.ApplyEffect(EnemyStatusEffect.Accelerate(battlecryDuration));
        }
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
        if (!this || !transform) return;
        SoundManagersDontDestroy.GerOrCreate().SoundFX.PlaySoundFXClip(SoundFXType.BanditHit, transform);
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
        if (behaviour)
        {
            if (apply)
                behaviour.ApplyHighlight(highlightRenderers);
            else
                behaviour.ClearHighlight(highlightRenderers);
        }
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
