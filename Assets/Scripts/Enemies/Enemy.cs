using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Enemy : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private SphereCollider sphereCollider;

    [Header("Movement & Path")]
    [SerializeField] private SplineContainer path;
    [SerializeField] private float speed = 100f;
    [SerializeField] private float maxHealthPoints = 100f;
    [SerializeField] private GameObject healthBarGO;

    [Header("Attack Animation")]
    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private float forwardDistance = 0.4f;
    [SerializeField] private float duration = 0.4f;

    [Header("UI")]
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private float popupHeightOffset = 10f;

    public event Action<Enemy> OnDeath;
    private float healthPoints;
    public float HealthPointsNormalized => healthPoints / maxHealthPoints;
    public bool IsFullHealth => Mathf.Approximately(healthPoints, maxHealthPoints);

    private float t = 0f;

    private Nexus targetNexus;
    private float attackCooldown;
    private float originalSpeed;

    private readonly Dictionary<EffectType, Coroutine> activeEffects = new();

    public void SetSpline(SplineContainer pathContainer, float startT = 0f)
    {
        path = pathContainer;
        t = Mathf.Clamp01(startT);

        Assert.IsNotNull(path);
        transform.position = path.EvaluatePosition(0, t);
    }

    public void Start()
    {
        sphereCollider.radius = attackRange;
    }

    public void TakeDamage(float damage, bool isCritical = false)
    {
        healthPoints -= damage;
        healthBarGO.SetActive(true);

        Vector3 spawnPosition = transform.position + Vector3.up * popupHeightOffset;
        DamagePopupManager.Instance.ShowPopup(spawnPosition, damage, isCritical);
        
        if (healthPoints <= 0f) Die();
    }

    private void Die()
    {
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }

    void Awake()
    {
        healthPoints = maxHealthPoints;
        originalSpeed = speed;
    }

    private void Update()
    {
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
        Assert.IsNotNull(path);

        float length = path.CalculateLength();
        if (length <= 0.001f) return;

        t += speed / length * Time.deltaTime;
        if (t > 1f) t -= 1f;

        Vector3 position = path.EvaluatePosition(0, t);
        Vector3 tangent = path.EvaluateTangent(0, t);

        transform.position = new Vector3(position.x, position.y, position.z);
        if (tangent != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(tangent);
        }
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
                while(elapsed < effect.duration)
                {
                    TakeDamage(effect.tickDamage);
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
