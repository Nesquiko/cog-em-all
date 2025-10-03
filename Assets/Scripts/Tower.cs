using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float range = 300f;

    [Header("Range Indicator")]
    [SerializeField] private Transform rangeIndicator;

    private float fireCooldown = 0f;

    private void Start()
    {
        if (rangeIndicator != null)
        {
            float scale = range * 2;
            rangeIndicator.localScale = new Vector3(scale - (range / 10), 0.01f, scale - (range / 10));
        }
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        Enemy targetEnemy = FindClosestEnemy();

        if (targetEnemy != null && fireCooldown <= 0f)
        {
            Shoot(targetEnemy);
            fireCooldown = 1f / fireRate;
        }
    }

    void Shoot(Enemy enemy)
    {
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        if (bulletGO.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.SetTarget(enemy.transform);
        }
    }

    Enemy FindClosestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Enemy e in enemies)
        {
            float distance = Vector3.Distance(transform.position, e.transform.position);

            if (distance < minDistance && distance <= range)
            {
                minDistance = distance;
                closest = e;
            }
        }

        return closest;
    }
}
