using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Bomber : MonoBehaviour, IEnemy
{
    public EnemyType Type => EnemyType.Bomber;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 100f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private SphereCollider sphereCollider;

    [Header("Movement & Health")]
    [SerializeField] private SplineContainer path;
    [SerializeField] private float speed = 25f;
    [SerializeField] private float maxHealthPoints = 100f;
    [SerializeField] private GameObject healthBar;

    [Header("Attack Animation")]
    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private float forwardDistance = 0.4f;
    [SerializeField] private float duration = 0.4f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem buffVFX;
    [SerializeField] private ParticleSystem debuffVFX;

    [SerializeField] private int onKillGearsReward = 10;

    public float Speed
    {
        get => speed;
        set => speed = value;
    }
    public Transform Transform => transform;

    public int OnKillGearsReward => onKillGearsReward;

    public event Action<IEnemy> OnDeath;
    private float healthPoints;
    public float HealthPointsNormalized => healthPoints / maxHealthPoints;
    public bool IsFullHealth => Mathf.Approximately(healthPoints, maxHealthPoints);

    private float t = 0f;

    private IDamageable target;
    private float originalSpeed;

    private bool hasAttacked = false;

    private readonly Dictionary<EffectType, Coroutine> activeEffects = new();

    private float pathLength;
    private float lateralOffset;

    private DamagePopupManager damagePopupManager;

    private void Awake()
    {
        damagePopupManager = FindFirstObjectByType<DamagePopupManager>();

        healthPoints = maxHealthPoints;
        originalSpeed = speed;
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
        sphereCollider.radius = attackRange;
    }

    public void TakeDamage(float damage, bool isCritical = false, EnemyStatusEffect withEffect = null)
    {
        if (healthPoints <= 0f) return;

        if (!healthBar.activeSelf) healthBar.SetActive(true);
        healthPoints -= damage;

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

    private void Die()
    {
        Debug.Log("dying right now");

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
            AttackNexus();
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

    private void AttackNexus()
    {
        if (hasAttacked || target == null) return;

        transform.LookAt(target.Transform(), Vector3.up);

        hasAttacked = true;
        StartCoroutine(AttackAnimation(target.Transform()));
    }

    private IEnumerator AttackAnimation(Transform target)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = target.position;

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

        Explode(target);

        yield return new WaitForSeconds(0.1f);

        Die();
    }

    private void Explode(Transform target)
    {
        if (target == null) return;
        this.target.TakeDamage(attackDamage);
    }


    public void EnterAttackRange(IDamageable damageable)
    {
        target = damageable;
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

        if (effect.negative) debuffVFX.Play();
        else buffVFX.Play();

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
        if (routine != null)
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

        bool negative = EnemyStatusEffect.IsNegative(type);
        if (negative) debuffVFX.Stop(withChildren: true);
        else buffVFX.Stop(withChildren: true);
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
            case EffectType.Accelerated:
            case EffectType.Slowed:
                speed = originalSpeed * effect.speedMultiplier;
                yield return new WaitForSeconds(effect.duration);
                speed = originalSpeed;
                break;
        }

        RemoveEffect(effect.type);
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
