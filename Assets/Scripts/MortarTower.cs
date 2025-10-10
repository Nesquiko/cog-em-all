using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class MortarTower : MonoBehaviour
{
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private Transform basePivot;
    [SerializeField] private Transform cannonPivot;
    [SerializeField] private Transform firePoint;
    [SerializeField] private CapsuleCollider capsuleCollider;

    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float minRange = 20f;
    [SerializeField] private float maxRange = 60f;
    [SerializeField] private float rotationSpeed = 5f;

    [SerializeField] private float launchSpeed = 30f;
    [SerializeField] private float arcHeight = 15f;

    private readonly Dictionary<int, Enemy> enemiesInRange = new();
    private Enemy target;
    private float fireCooldown;

    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.cyan;
        Handles.DrawWireDisc(transform.position, Vector3.up, maxRange);

        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, minRange);
    }

    void Start()
    {
        capsuleCollider.radius = maxRange;
    }

    void Update()
    {
        if (target == null)
        {
            target = TowerMechanics.GetClosestEnemy(transform.position, enemiesInRange);
            if (target == null) return;
        }

        if (!IsTargetValid(target))
        {
            target = TowerMechanics.GetClosestEnemy(transform.position, enemiesInRange);
            if (!IsTargetValid(target)) return;
        }

        RotateTowardTarget(target.transform);

        fireCooldown -= Time.deltaTime;
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
        if (shellPrefab == null || firePoint == null || enemy == null) return;

        GameObject shellGO = Instantiate(shellPrefab, firePoint.position, firePoint.rotation);
        if (shellGO.TryGetComponent<Shell>(out var shell))
        {
            shell.Launch(enemy.transform.position, launchSpeed, arcHeight);
        }
    }

    private bool IsTargetValid(Enemy e)
    {
        if (e == null) return false;
        float distance = Vector3.Distance(transform.position, e.transform.position);
        return distance >= minRange && distance <= maxRange;
    }

    private void RotateTowardTarget(Transform targetTransform)
    {
        if (targetTransform == null) return;

        Vector3 flatDirection = targetTransform.position - basePivot.position;
        flatDirection.y = 0f;
        if (flatDirection.sqrMagnitude > 0.001f)
        {
            Quaternion baseRotation = Quaternion.LookRotation(flatDirection);
            basePivot.rotation = Quaternion.Lerp(
                basePivot.rotation,
                baseRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        Vector3 localTarget = basePivot.InverseTransformPoint(targetTransform.position);
        float pitch = Mathf.Atan2(localTarget.y, localTarget.z) * Mathf.Rad2Deg;
        Quaternion cannonRotation = Quaternion.Euler(pitch, 0f, 0f);

        cannonPivot.localRotation = Quaternion.Lerp(
            cannonPivot.localRotation,
            cannonRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void OnDestroy()
    {
        TowerMechanics.UnsubscribeAll(enemiesInRange, HandleEnemyDeath);
    }
}
