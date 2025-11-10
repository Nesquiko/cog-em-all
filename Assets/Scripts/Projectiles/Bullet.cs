using UnityEngine;
using UnityEngine.Assertions;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;

    private Transform target;
    private Vector3 flightDirection;
    private float damage;
    private bool crit;

    public void Initialize(Transform enemyTarget, float dmg, bool isCrit)
    {
        Assert.IsNotNull(enemyTarget);
        target = enemyTarget;
        damage = dmg;
        crit = isCrit;
        flightDirection = (target.position - transform.position).normalized;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (target != null)
        {
            flightDirection = (target.position - transform.position).normalized;
        }

        transform.position += speed * Time.deltaTime * flightDirection;
        transform.rotation = Quaternion.LookRotation(flightDirection);
        transform.Rotate(90f, 0f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(damage, crit, withEffect: EnemyStatusEffect.Bleed);
            Destroy(gameObject);
        }

        if (other.TryGetComponent<TerrainCollider>(out _))
        {
            Destroy(gameObject);
        }

        if (other.CompareTag("TowerModel"))
        {
            Destroy(gameObject);
            return;
        }
    }
}
