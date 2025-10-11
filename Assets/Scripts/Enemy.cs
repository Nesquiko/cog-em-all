using System;
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

    public event Action<Enemy> OnDeath;
    private float healthPoints;
    public float HealthPointsNormalized => healthPoints / maxHealthPoints;
    private float t = 0f;

    private Nexus targetNexus;
    private float attackCooldown;
    private float originalSpeed;

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

    public void TakeDamage(float damage)
    {
        healthPoints -= damage;
        if (healthBarGO != null)
        {
            healthBarGO.SetActive(true);
        }

        if (healthPoints <= 0f)
        {
            Die();
        }
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
            targetNexus.TakeDamage(attackDamage);
            attackCooldown = attackRate;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Nexus nexus)) return;
        
        targetNexus = nexus;
        attackCooldown = 0;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Nexus nexus) && nexus == targetNexus)
        {
            targetNexus = null;
        }
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
