using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private SphereCollider attackTrigger;
    public float AttackDamage => attackDamage;
    [SerializeField] private float attackRate = 1f;
    public event Action<IDamageable> OnSuicide;

    [SerializeField] private int onKillGearsReward = 10;
    public int OnKillGearsReward => onKillGearsReward;

    [Header("Movement & Path")]
    [SerializeField] private SplineContainer path;
    [SerializeField] private float speed = 20f;
    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    [Header("Health")]
    [SerializeField] private float maxHealthPoints = 100f;
    public float MaxHealthPoints => maxHealthPoints;
    [SerializeField] private EnemyHealthBar healthBar;
    private float healthPoints;
    public float HealthPoints => healthPoints;
    public float HealthPointsNormalized => healthPoints / maxHealthPoints;
    public bool IsFullHealth => Mathf.Approximately(healthPoints, maxHealthPoints);
    public event Action OnDeath;

    [Header("Attack Animation")]
    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private float forwardDistance = 0.4f;
    [SerializeField] private float duration = 0.4f;

    [Header("UI")]
    private DamagePopupManager damagePopupManager;
    private GearDropManager gearDropManager;

    [Header("VFX")]
    [SerializeField] private ParticleSystem buffVFX;
    [SerializeField] private ParticleSystem debuffVFX;

    // Damaging target
    private IDamageable target;
    private float attackCooldown;
    private float originalSpeed;


    private readonly Dictionary<EffectType, Coroutine> activeEffects = new();
    private readonly Dictionary<EffectType, int> stackCounts = new();

    // Path following
    private float splinePathT = 0f;
    private float pathLength;
    private float lateralOffset;

    private void Awake()
    {
        damagePopupManager = FindFirstObjectByType<DamagePopupManager>();
        gearDropManager = FindFirstObjectByType<GearDropManager>();

        healthPoints = maxHealthPoints;
        originalSpeed = speed;
    }

    private void Start()
    {
        attackTrigger.radius = attackRange;
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

    public void SetSpline(SplineContainer pathContainer, float startT, float lateralOffset)
    {
        Assert.IsNotNull(pathContainer);
        path = pathContainer;
        pathLength = path.CalculateLength();
        splinePathT = Mathf.Clamp01(startT);
        this.lateralOffset = lateralOffset;
    }


    public void TakeDamage(float damage, DamageSourceType source, bool isCritical = false, EnemyStatusEffect withEffect = null)
    {
        if (healthPoints <= 0f) return;

        float totalMultiplier = 1f;
        if (source == DamageSourceType.Bullet && stackCounts.TryGetValue(EffectType.ArmorShredded, out int shredStacks))
        {
            totalMultiplier += shredStacks * EnemyStatusEffect.ArmorShred.damageMultiplierPerStack;
        }

        healthPoints -= damage * totalMultiplier;
        if (!healthBar.ActiveSelf) healthBar.SetActive(true);

        damagePopupManager.ShowPopup(transform.position, damage, isCritical);

        if (healthPoints <= 0)
        {
            Die();
        }
        else if (withEffect != null)
        {
            ApplyEffect(withEffect);
        }
    }

    private void FollowPath()
    {
        splinePathT = Mathf.Repeat(splinePathT + speed / pathLength * Time.deltaTime, 1f);

        Assert.IsNotNull(path);
        Vector3 position = path.EvaluatePosition(0, splinePathT);
        Vector3 tangent = path.EvaluateTangent(0, splinePathT);
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

        Vector3 direction = targetPosition - startPosition;
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

        if (OnSuicide != null)
        {
            OnSuicide.Invoke(damageableTarget);
            Die();
            yield break;
        }

        bool hasDealtDamage = false;
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            float normalized = t / half;
            transform.position = Vector3.Lerp(apexPosition, startPosition, normalized);

            if (!hasDealtDamage && normalized > 0.3f)
            {
                IEnemy attacker = GetComponent<IEnemy>();
                damageableTarget.TakeDamage(attackDamage, attacker);
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
        if (effect.stackable)
        {
            HandleStackableEffect(effect);
            return;
        }

        if (activeEffects.ContainsKey(effect.type) && activeEffects[effect.type] != null)
            StopCoroutine(activeEffects[effect.type]);

        if (effect.persistent)
        {
            HandlePersistentEffect(effect);
            return;
        }

        Coroutine routine = StartCoroutine(HandleEffect(effect));
        activeEffects[effect.type] = routine;
        UpdateVFXState();
    }

    private void HandlePersistentEffect(EnemyStatusEffect effect)
    {
        switch (effect.type)
        {
            case EffectType.Oiled:
            case EffectType.DebrisSlowed:
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

    private void HandleStackableEffect(EnemyStatusEffect effect)
    {
        stackCounts.TryGetValue(effect.type, out int stacks);
        stacks = Mathf.Min(stacks + 1, effect.maxStacks);
        stackCounts[effect.type] = stacks;

        if (activeEffects.ContainsKey(effect.type) && activeEffects[effect.type] != null)
            StopCoroutine(activeEffects[effect.type]);

        Coroutine routine = StartCoroutine(HandleStackableLifetime(effect));
        activeEffects[effect.type] = routine;

        UpdateVFXState();
    }

    private IEnumerator HandleStackableLifetime(EnemyStatusEffect effect)
    {
        yield return new WaitForSeconds(effect.duration);
        if (stackCounts.TryGetValue(effect.type, out int stacks))
        {
            stacks--;
            if (stacks <= 0)
            {
                stackCounts.Remove(effect.type);
                activeEffects.Remove(effect.type);
            }
            else
            {
                stackCounts[effect.type] = stacks;
                activeEffects[effect.type] = StartCoroutine(HandleStackableLifetime(effect));
            }
        }

        UpdateVFXState();
    }

    public void RemoveEffect(EffectType type)
    {
        if (!activeEffects.ContainsKey(type)) return;

        var routine = activeEffects[type];
        if (routine != null)
            StopCoroutine(routine);

        activeEffects.Remove(type);

        switch (type)
        {
            case EffectType.Oiled:
            case EffectType.DebrisSlowed:
                speed = originalSpeed;
                break;
        }

        UpdateVFXState();
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
                    TakeDamage(effect.tickDamage, DamageSourceType.Effect, isCritical: false);
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
        UpdateVFXState();
    }

    private IEnumerator IndefiniteBurn(EnemyStatusEffect effect)
    {
        while (true)
        {
            TakeDamage(effect.tickDamage, DamageSourceType.Effect, isCritical: false);
            yield return new WaitForSeconds(effect.tickInterval);
        }
    }

    private void UpdateVFXState()
    {
        if (debuffVFX == null || buffVFX == null) return;

        bool anyNegative = false;
        bool anyPositive = false;

        foreach (var type in activeEffects.Keys)
        {
            if (EnemyStatusEffect.IsNegative(type))
                anyNegative = true;
            else
                anyPositive = true;
        }

        if (anyNegative)
        {
            if (!debuffVFX.isPlaying) debuffVFX.Play();
        }
        else debuffVFX.Stop(withChildren: true);

        if (anyPositive)
        {
            if (!buffVFX.isPlaying) buffVFX.Play();
        }
        else buffVFX.Stop(withChildren: true);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        gearDropManager.SpawnGears(transform.position, 1);
        Destroy(gameObject);
    }
}
