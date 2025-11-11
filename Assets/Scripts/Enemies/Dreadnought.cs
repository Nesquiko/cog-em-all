using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Dreadnought : MonoBehaviour, IEnemy
{
    public EnemyType Type => EnemyType.Dreadnought;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 50f;
    [SerializeField] private float attackRate = 3f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private SphereCollider sphereCollider;

    [Header("Movement & Path")]
    [SerializeField] private SplineContainer path;
    [SerializeField] private float speed = 10f;
    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    public Transform Transform => transform;

    [SerializeField] private float maxHealthPoints = 500f;
    [SerializeField] private GameObject healthBar;

    [Header("Attack Animation")]
    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private float forwardDistance = 0.4f;
    [SerializeField] private float duration = 0.4f;

    [Header("UI")]
    [SerializeField] private float popupHeightOffset = 10f;

    [Header("Shield")]
    [SerializeField, Range(0.01f, 0.3f)] private float shieldHealthFraction = 0.1f;
    [SerializeField] private float shieldCooldown = 10f;
    [SerializeField] private GameObject shield;

    [SerializeField] private int onKillGearsReward = 10;

    public int OnKillGearsReward => onKillGearsReward;

    public event Action<IEnemy> OnDeath;
    private float healthPoints;
    public float HealthPointsNormalized => healthPoints / maxHealthPoints;
    public bool IsFullHealth => Mathf.Approximately(healthPoints, maxHealthPoints);

    private float t = 0f;

    private Nexus targetNexus;
    private float attackCooldown;
    private float originalSpeed;

    private readonly Dictionary<EffectType, Coroutine> activeEffects = new();

    private float pathLength;
    private float lateralOffset;

    private float shieldHealthPoints;
    private float nextShieldTimer;
    private bool shieldActive;

    private DamagePopupManager damagePopupManager;

    private void Awake()
    {
        damagePopupManager = FindFirstObjectByType<DamagePopupManager>();

        healthPoints = maxHealthPoints;
        originalSpeed = speed;
        sphereCollider.radius = attackRange;
    }

    public void Initialize(SplineContainer pathContainer, float startT, float lateralOffset, Action<IEnemy> onDeath)
    {
        SetSpline(pathContainer, startT, lateralOffset);
        OnDeath += onDeath;
    }

    private void Start()
    {
        ActivateShield();
    }

    private void SetSpline(SplineContainer pathContainer, float startT, float lateralOffset)
    {
        Assert.IsNotNull(pathContainer);
        path = pathContainer;
        pathLength = path.CalculateLength();
        t = Mathf.Clamp01(startT);
        this.lateralOffset = lateralOffset;
    }

    private void ActivateShield()
    {
        shieldHealthPoints = maxHealthPoints * shieldHealthFraction;
        shieldActive = true;
        // shield.SetActive(true);
    }

    private void BreakShield()
    {
        shieldActive = false;
        nextShieldTimer = 0f;
        // shield.SetActive(false);
    }

    public void TakeDamage(float damage, bool isCritical = false, EnemyStatusEffect withEffect = null)
    {
        if (healthPoints <= 0f) { return; }

        if (shieldActive)
        {
            shieldHealthPoints -= damage;
            if (shieldHealthPoints <= 0f)
            {
                BreakShield();
            }
            return;
        }

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
        if (!shieldActive)
        {
            nextShieldTimer += Time.deltaTime;
            if (nextShieldTimer >= shieldCooldown)
            {
                ActivateShield();
                nextShieldTimer = 0f;
            }
        }

        if (targetNexus == null)
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
        if (targetNexus == null) return;

        transform.LookAt(targetNexus.transform, Vector3.up);

        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            StartCoroutine(AttackAnimation(targetNexus.transform));
            attackCooldown = attackRate;
        }
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

        bool hasDealtDamage = false;
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            float normalized = t / half;
            transform.position = Vector3.Lerp(apexPosition, startPosition, normalized);

            if (!hasDealtDamage && normalized > 0.3f)
            {
                targetNexus.TakeDamage(attackDamage);
                hasDealtDamage = true;
            }

            yield return null;
        }

        transform.position = startPosition;
    }

    public void EnterAttackRange(Nexus nexus)
    {
        targetNexus = nexus;
        attackCooldown = 0f;
        speed = 0f;
    }

    public void ExitAttackRange(Nexus nexus)
    {
        if (targetNexus == nexus)
        {
            targetNexus = null;
            speed = originalSpeed;
        }
    }

    public void ApplyEffect(EnemyStatusEffect effect)
    {
        if (activeEffects.ContainsKey(effect.type))
        {
            StopCoroutine(activeEffects[effect.type]);
        }

        Coroutine routine = StartCoroutine(HandleEffect(effect));
        activeEffects[effect.type] = routine;
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
}
