using UnityEngine;

public class TeslaTower : MonoBehaviour
{
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float range = 300f;
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

    private void Update()
    {
        fireCooldown -= Time.deltaTime;
        Enemy target = FindClosestEnemy();

        if (target != null && fireCooldown <= 0f)
        {
            Shoot(target);
            fireCooldown = 1f / fireRate;
        }
    }

    private void Shoot(Enemy enemy)
    {
        if (beamPrefab == null || firePoint == null)
            return;

        GameObject beamGO = Instantiate(beamPrefab, Vector3.zero, Quaternion.identity);

        if (beamGO.TryGetComponent<Beam>(out var beam))
        {
            beam.Initialize(firePoint, enemy.transform);
        }
    }

    private Enemy FindClosestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy closest = null;
        float minDist = Mathf.Infinity;

        foreach (Enemy e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist && dist <= range)
            {
                minDist = dist;
                closest = e;
            }
        }

        return closest;
    }
}
