using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CapsuleCollider))]
public class GatlingTower : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform gatlingHead;
    [SerializeField] private Transform gatlingGunL;
    [SerializeField] private Transform gatlingGunR;
    [SerializeField] private Transform gatlingFirePointL;
    [SerializeField] private Transform gatlingFirePointR;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float range = 30f;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private GameObject rangeIndicator;

    [SerializeField] private float recoilDistance = 0.2f;
    [SerializeField] private float recoilSpeed = 20f;
    [SerializeField] private float recoilReturnSpeed = 5f;

    private readonly Dictionary<int, Enemy> enemiesInRange = new();
    private Enemy target;
    private float fireCooldown = 0f;

    private Vector3 gunPositionL;
    private Vector3 gunPositionR;
    private Coroutine recoilRoutineL;
    private Coroutine recoilRoutineR;

    private bool shootFromLeftFirePoint = true;

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

        if (gatlingGunL != null) gunPositionL = gatlingGunL.localPosition;
        if (gatlingGunR != null) gunPositionR = gatlingGunR.localPosition;
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

        if (gatlingHead != null)
        {
            TowerMechanics.RotateTowardTarget(gatlingHead, target.transform, 10f);
        }
    }

    private void OnTriggerEnter(Collider other)
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
        Transform gun = shootFromLeftFirePoint ? gatlingGunL : gatlingGunR;
        Transform firePoint = shootFromLeftFirePoint ? gatlingFirePointL : gatlingFirePointR;
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        if (bulletGO.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.SetTarget(enemy.transform);
        }

        if (gun == null) return;

        if (shootFromLeftFirePoint)
        {
            if (recoilRoutineL != null) StopCoroutine(recoilRoutineL);
            recoilRoutineL = StartCoroutine(RecoilKick(gun, gunPositionL));
        }
        else
        {
            if (recoilRoutineR != null) StopCoroutine(recoilRoutineR);
            recoilRoutineR = StartCoroutine(RecoilKick(gun, gunPositionR));
        }

            shootFromLeftFirePoint = !shootFromLeftFirePoint;
    }

    private IEnumerator RecoilKick(Transform gun, Vector3 defaultLocalPosition)
    {
        Vector3 start = gun.localPosition;
        Vector3 back = defaultLocalPosition + gun.up * recoilDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            gun.localPosition = Vector3.Lerp(start, back, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilReturnSpeed;
            gun.localPosition = Vector3.Lerp(back, defaultLocalPosition, t);
            yield return null;
        }

        gun.localPosition = defaultLocalPosition;
    }

    private void OnDestroy()
    {
        TowerMechanics.UnsubscribeAll(enemiesInRange, HandleEnemyDeath);
    }
}
