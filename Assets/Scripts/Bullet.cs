using NUnit.Framework;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float lifetime = 1f;

    private Transform target;
    private Vector3 flightDirection;

    public void SetTarget(Transform enemyTarget)
    {
        target = enemyTarget;
        Assert.IsNotNull(target);
        flightDirection = (target.position - transform.position).normalized;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        Assert.IsNotNull(target);
        flightDirection = (target.position - transform.position).normalized;

        transform.position += speed * Time.deltaTime * flightDirection;
        transform.rotation = Quaternion.LookRotation(flightDirection);
        transform.Rotate(90f, 0f, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
