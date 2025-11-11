using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Bandit : MonoBehaviour, IEnemy
{
    public EnemyType Type => EnemyType.Bandit;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private SphereCollider sphereCollider;

    [Header("Movement & Path")]
    [SerializeField] private SplineContainer path;
    [SerializeField] private float speed = 20f;
    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    public Transform Transform => transform;
    [SerializeField] private float maxHealthPoints = 100f;
    [SerializeField] private GameObject healthBar;

    [Header("Attack Animation")]
    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private float forwardDistance = 0.4f;
    [SerializeField] private float duration = 0.4f;

    [Header("UI")]
    [SerializeField] private float popupHeightOffset = 10f;

    [SerializeField] private int onKillGearsReward = 10;

    [Header("Leadership")]
    [SerializeField, Range(0f, 1f)] private float leaderChance = 0.1f;
    [SerializeField, Range(5f, 25f)] private float battlecryRange = 15f;
    [SerializeField] private float battlecryDuration = 3f;
    [SerializeField] private float battlecryCooldown = 10f;

    public int OnKillGearsReward => onKillGearsReward;

    public event Action<IEnemy> OnDeath;
    private float healthPoints;
    public float HealthPointsNormalized => healthPoints / maxHealthPoints;
    public bool IsFullHealth => Mathf.Approximately(healthPoints, maxHealthPoints);

    private float t = 0f;

    private IDamageable target;
    private float attackCooldown;
    private float originalSpeed;

    private readonly Dictionary<EffectType, Coroutine> activeEffects = new();

    private float pathLength;
    private float lateralOffset;

    private DamagePopupManager damagePopupManager;

    private bool isLeader;

    private void Awake()
    {
        damagePopupManager = FindFirstObjectByType<DamagePopupManager>();

        healthPoints = maxHealthPoints;
        originalSpeed = speed;

        isLeader = UnityEngine.Random.value <= leaderChance;
    }

    public void Initialize(SplineContainer pathContainer, float startT, float lateralOffset, Action<IEnemy> onDeath)
    {
        SetSpline(pathContainer, startT, lateralOffset);
        OnDeath += onDeath;
    }

    private void SetSpline(SplineContainer pathContainer, float startT, float lateralOffset)
    {
        Assert.IsNotNull(pathContainer);
        path = pathContainer;
        pathLength = path.CalculateLength();
        t = Mathf.Clamp01(startT);
        this.lateralOffset = lateralOffset;
    }

    private void Start()
    {
        if (isLeader)
        {
            StartCoroutine(Leadership());
        }
    }

    public void TakeDamage(float damage, bool isCritical = false, EnemyStatusEffect withEffect = null)
    {
        if (healthPoints <= 0f) { return; }

        healthPoints -= damage;
        if (!healthBar.activeSelf) healthBar.SetActive(true);

        Vector3 popupSpawnPosition = transform.position + Vector3.up * popupHeightOffset;
        damagePopupManager.ShowPopup(popupSpawnPosition, damage, isCritical);

        if (healthPoints <= 0)
        {
            Die();
        }
        else if (withEffect != null)
        {
            ApplyEffect(withEffect);
        }
    }

    private void Die()
    {
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (target == null)
        {
            FollowPath();
        }
        else
        {
            AttackTarget();
        }
    }

    private IEnumerator Leadership()
    {
        while (healthPoints > 0)
        {
            yield return new WaitForSeconds(battlecryDuration + battlecryCooldown);
            Battlecry();
        }
    }

    private void Battlecry()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            battlecryRange,
            LayerMask.GetMask("Enemies")
        );

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<Bandit>(out var ally))
            {
                ally.ApplyEffect(EnemyStatusEffect.Accelerate(battlecryDuration));
            }
        }
    }

    private void FollowPath()
    {
        t = Mathf.Repeat(t + speed / pathLength * Time.deltaTime, 1f);

        Vector3 position = path.EvaluatePosition(0, t);
        Vector3 tangent = path.EvaluateTangent(0, t);
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(up, tangent).normalized;

        transform.SetPositionAndRotation(position + right * lateralOffset, Quaternion.LookRotation(tangent));
    }

    private void AttackTarget()
    {
        if (target == null || target.IsDestroyed())
        {
            target = null;
            speed = originalSpeed;
            return;
        }

        transform.LookAt(target.Transform(), Vector3.up);

        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            StartCoroutine(AttackAnimation(target));
            attackCooldown = attackRate;
        }
    }

    private IEnumerator AttackAnimation(IDamageable damageableTarget)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = damageableTarget.Transform().position;

        Vector3 direction = (targetPosition - startPosition);
        direction.y = 0f;
        direction.Normalize();

        Vector3 apexPosition = startPosition + direction * forwardDistance + Vector3.up * jumpHeight;

        float half = duration * 0.5f;
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            float normalized = t / half;
            transform.position = Vector3.Lerp(startPosition, apexPosition, normalized);
            yield return null;
        }

        bool hasDealtDamage = false;
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            float normalized = t / half;
            transform.position = Vector3.Lerp(apexPosition, startPosition, normalized);

            if (!hasDealtDamage && normalized > 0.3f)
            {
                damageableTarget.TakeDamage(attackDamage);
                hasDealtDamage = true;
            }

            yield return null;
        }

        transform.position = startPosition;
    }

    public void EnterAttackRange(IDamageable damageable)
    {
        target = damageable;
        attackCooldown = 0f;
        speed = 0f;
    }

    public void ExitAttackRange(IDamageable damageable)
    {
        if (target == damageable)
        {
            target = null;
            speed = originalSpeed;
        }
    }

    public void ApplyEffect(EnemyStatusEffect effect)
    {
        if (activeEffects.ContainsKey(effect.type) && activeEffects[effect.type] != null)
            StopCoroutine(activeEffects[effect.type]);

        if (effect.persistent)
        {
            ApplyPersistentEffect(effect);
            return;
        }

        Coroutine routine = StartCoroutine(HandleEffect(effect));
        activeEffects[effect.type] = routine;
    }

    private void ApplyPersistentEffect(EnemyStatusEffect effect)
    {
        switch (effect.type)
        {
            case EffectType.Oiled:
                speed = originalSpeed * effect.speedMultiplier;
                if (!activeEffects.ContainsKey(effect.type))
                    activeEffects[effect.type] = null;
                break;
            case EffectType.OilBurned:
                if (!activeEffects.ContainsKey(effect.type))
                {
                    Coroutine routine = StartCoroutine(IndefiniteBurn(effect));
                    activeEffects[effect.type] = routine;
                }
                break;
        }
    }

    public void RemoveEffect(EffectType type)
    {
        if (!activeEffects.ContainsKey(type)) return;

        var routine = activeEffects[type];
        if (routine !=  null)
            StopCoroutine(routine);
        
        activeEffects.Remove(type);

        switch (type)
        {
            case EffectType.Oiled:
                speed = originalSpeed;
                break;
            case EffectType.OilBurned:
                break;
        }
    }

    private IEnumerator HandleEffect(EnemyStatusEffect effect)
    {
        float elapsed = 0f;

        switch (effect.type)
        {
            case EffectType.Burning:
            case EffectType.Bleeding:
                while (elapsed < effect.duration)
                {
                    TakeDamage(effect.tickDamage, isCritical: false);
                    yield return new WaitForSeconds(effect.tickInterval);
                    elapsed += effect.tickInterval;
                }
                break;
            case EffectType.Slowed:
                speed = originalSpeed * effect.speedMultiplier;
                yield return new WaitForSeconds(effect.duration);
                speed = originalSpeed;
                break;
        }

        activeEffects.Remove(effect.type);
    }

    private IEnumerator IndefiniteBurn(EnemyStatusEffect effect)
    {
        while (true)
        {
            TakeDamage(effect.tickDamage, isCritical: false);
            yield return new WaitForSeconds(effect.tickInterval);
        }
    }
}
