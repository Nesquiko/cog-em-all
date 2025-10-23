using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class TowerV2 : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float range = 30f;
    [SerializeField] private CapsuleCollider capsuleCollider;

    private readonly Dictionary<int, Enemy> enemiesInRange = new();
    private Enemy target;
    private float fireCooldown = 0f;

    void OnDrawGizmosSelected()
    {
        Handles.color = Color.cyan;
        var center = new Vector3(transform.position.x, 0, transform.position.z);
        Handles.DrawWireDisc(center, Vector3.up, capsuleCollider.radius);
    }

    private void Start()
    {
        capsuleCollider.radius = range;
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (target == null)
        {
            target = TowerMechanics.GetClosestEnemy(transform.position, enemiesInRange);
            if (target == null) return;
        }

        if (!TowerMechanics.IsEnemyInRange(transform.position, target, range))
        {
            target = null;
            return;
        }

        if (fireCooldown <= 0f)
        {
            Shoot(target);
            fireCooldown = 1f / fireRate;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        TowerMechanics.HandleTriggerEnter(other, enemiesInRange, HandleEnemyDeath);
    }

    void OnTriggerExit(Collider other)
    {
        TowerMechanics.HandleTriggerExit(other, enemiesInRange, HandleEnemyDeath, target, out target);
    }

    private void HandleEnemyDeath(Enemy deadEnemy)
    {
        target = TowerMechanics.HandleEnemyRemoval(deadEnemy, enemiesInRange, target);
    }

    void Shoot(Enemy enemy)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        if (bulletGO.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.SetTarget(enemy.transform);
        }
    }

    private void OnDestroy()
    {
        TowerMechanics.UnsubscribeAll(enemiesInRange, HandleEnemyDeath);       
    }
}
