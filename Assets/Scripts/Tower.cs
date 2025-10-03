using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float range = 7f;

    [Header("Range Indicator")]
    [SerializeField] private Transform rangeIndicator;

    private float fireCooldown = 0f;

    private void Start()
    {
        if (rangeIndicator != null)
        {
            float scale = range * 2f;
            rangeIndicator.localScale = new Vector3(scale, 0.01f, scale);
        }
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        Enemy targetEnemy = FindClosestEnemy();
        Debug.Log(targetEnemy != null && fireCooldown <= 0f);

        Debug.Log(targetEnemy);

        Debug.Log(fireCooldown);

        if (targetEnemy != null && fireCooldown <= 0f)
        {
            Debug.Log("Shooting an enemy");
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
        Debug.Log("Tower sees " + enemies.Length + " enemies alive.");
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
        if (enemies.Length > 0)
        {
            return enemies[0];
        }
        return null;
    }
}
