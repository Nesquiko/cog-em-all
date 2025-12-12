using UnityEngine;
using UnityEngine.Assertions;

public class Bullet : MonoBehaviour, IDamageSource
{
    private GatlingTower owner;
    private Transform target;
    private Vector3 flightDirection;
    private float damage;
    private bool crit;

    public DamageSourceType Type() => DamageSourceType.Bullet;

    public void Initialize(GatlingTower ownerTower, Transform enemyTarget, float dmg, bool isCrit)
    {
        Assert.IsNotNull(enemyTarget);
        owner = ownerTower;
        target = enemyTarget;
        damage = dmg;
        crit = isCrit;
        flightDirection = (target.position - transform.position).normalized;

        float maxDistance = owner.InfiniteRange ? owner.ManualBulletRange : owner.TowerRange;
        float lifetime = maxDistance / owner.BulletSpeed;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (target != null)
            flightDirection = (target.position - transform.position).normalized;

        transform.position += owner.BulletSpeed * Time.deltaTime * flightDirection;
        transform.rotation = Quaternion.LookRotation(flightDirection);
        transform.Rotate(90f, 0f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IEnemy>(out var enemy))
        {
            EnemyStatusEffect effect = null;
            if (owner.SlowOnHitActive) effect = EnemyStatusEffect.Slow;
            else if (owner.ArmorRendingActive) effect = EnemyStatusEffect.ArmorShred(owner.MaxArmorRendingStacks);

            enemy.TakeDamage(damage, Type(), crit, effect);
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
