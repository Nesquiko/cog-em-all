using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CapsuleCollider))]
public class TowerV2 : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private CapsuleCollider capsuleCollider;

    private float fireCooldown = 0f;
    // TODO xkilian make a list of enemies and in OnTriggerEnter and remove in OnTriggerExit,
    //  or use hash map with key being, key is id from `gameObject.GetInstanceID()`, value Enemy.
    private Enemy target;


    void OnDrawGizmosSelected()
    {
        Handles.color = Color.cyan;
        var center = new Vector3(transform.position.x, 0, transform.position.z);
        Handles.DrawWireDisc(center, Vector3.up, capsuleCollider.radius);
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (target == null) return;

        var distance = Vector3.Distance(transform.position, target.transform.position);

        if (fireCooldown <= 0f && distance <= capsuleCollider.radius)
        {
            Shoot(target);
            fireCooldown = 1f / fireRate;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (target != null) return;
        if (!other.TryGetComponent<Enemy>(out var enemy)) return;

        target = enemy;
    }

    void OnTriggerExit(Collider other)
    {
    }

    void Shoot(Enemy enemy)
    {
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        if (bulletGO.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.SetTarget(enemy.transform);
        }
    }
}
