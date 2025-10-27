using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float attackRange = 1f;

    [SerializeField] private SphereCollider sphereCollider;

    [SerializeField] private SplineContainer path;
    [SerializeField] private float speed = 100f;
    [SerializeField] private float maxHealthPoints = 100f;
    [SerializeField] private GameObject healthBarGO;

    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private float forwardDistance = 0.4f;
    [SerializeField] private float duration = 0.4f;

    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private float popupHeightOffset = 10f;

    [SerializeField] private int onKillGearsReward = 10;
    public int OnKillGearsReward => onKillGearsReward;

    public event Action<Enemy> OnDeath;
    private float healthPoints;
    public float HealthPointsNormalized => healthPoints / maxHealthPoints;
    private float t = 0f;

    private Nexus targetNexus;
    private float attackCooldown;
    private float originalSpeed;

    private float pathLength;
    private float lateralOffset;

    public void Initialize(SplineContainer pathContainer, float startT, float lateralOffset, Action<Enemy> onDeath)
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

    public bool IsFullHealth => Mathf.Approximately(healthPoints, maxHealthPoints);

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
        t = Mathf.Repeat(t + speed / pathLength * Time.deltaTime, 1f);

        Vector3 position = path.EvaluatePosition(0, t);
        Vector3 tangent = path.EvaluateTangent(0, t);
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(up, tangent).normalized;

        transform.position = position + right * lateralOffset;
        transform.rotation = Quaternion.LookRotation(tangent);
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
}
