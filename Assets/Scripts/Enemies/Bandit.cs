using System;
using System.Collections;
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

    private bool isLeader;

    // IEnemy fields
    public EnemyType Type => EnemyType.Bandit;
    public event Action<IEnemy> OnDeath;
    public int OnKillGearsReward => behaviour.OnKillGearsReward;
    public float HealthPointsNormalized => behaviour.HealthPointsNormalized;
    public float Speed { get => behaviour.Speed; set => behaviour.Speed = value; }
    public Transform Transform => transform;

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
            yield return new WaitForSeconds(battlecryCooldown);
            leaderBattlecryVFX.Play();
            Battlecry();
            yield return new WaitForSeconds(battlecryDuration);
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
}
