using UnityEngine;
using UnityEngine.Assertions;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float baseDamage = 50f;
    [SerializeField] private float lifetime = 3f;

    private Transform target;
    private Vector3 flightDirection;
    private float damage;
    private bool crit;

    public float Damage => baseDamage;

    public void Initialize(Transform enemyTarget, float dmg, bool isCrit)
    {
        Assert.IsNotNull(enemyTarget);
        target = enemyTarget;
        damage = dmg;
        crit = isCrit;
        flightDirection = (target.position - transform.position).normalized;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (target != null)
        {
            flightDirection = (target.position - transform.position).normalized;
        }

        transform.position += speed * Time.deltaTime * flightDirection;
        transform.rotation = Quaternion.LookRotation(flightDirection);
        transform.Rotate(90f, 0f, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(damage, crit);
            Destroy(gameObject);
        }
    }
}
