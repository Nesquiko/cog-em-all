using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CapsuleCollider))]
public class TeslaTower : MonoBehaviour
{
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float range = 30f;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private GameObject rangeIndicator;

    private readonly Dictionary<int, Enemy> enemiesInRange = new();
    private Enemy target;
    private float fireCooldown = 0f;

    void OnDrawGizmosSelected()
    {
        TowerMechanics.DrawRangeGizmos(transform.position, Color.cyan, range);
    }

    private void Start()
    {
        Assert.IsNotNull(capsuleCollider);
        capsuleCollider.radius = range;

        Assert.IsNotNull(rangeIndicator);
        rangeIndicator.SetActive(false);
        rangeIndicator.transform.localScale = new(range * 2, rangeIndicator.transform.localScale.y, range * 2);
    }

    private void Update()
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

    private void OnTriggerEnter(Collider other)
    {
        TowerMechanics.HandleTriggerEnter(other, enemiesInRange, HandleEnemyDeath);
    }

    private void OnTriggerExit(Collider other)
    {
        TowerMechanics.HandleTriggerExit(other, enemiesInRange, HandleEnemyDeath, target, out target);
    }

    private void HandleEnemyDeath(Enemy deadEnemy)
    {
        target = TowerMechanics.HandleEnemyRemoval(deadEnemy, enemiesInRange, target);
    }

    private void Shoot(Enemy enemy)
    {
        if (beamPrefab == null || firePoint == null) return;

        GameObject beamGO = Instantiate(beamPrefab, firePoint.position, Quaternion.identity);

        if (beamGO.TryGetComponent<Beam>(out var beam))
        {
            beam.Initialize(firePoint, enemy.transform);
        }
    }

    private void OnDestroy()
    {
        TowerMechanics.UnsubscribeAll(enemiesInRange, HandleEnemyDeath);
    }
}
