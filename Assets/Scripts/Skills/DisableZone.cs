using System.Collections.Generic;
using UnityEngine;

public class DisableZone : MonoBehaviour
{
    [SerializeField] private float disableRadius = 15f;
    [SerializeField] private float duration = 10f;
    [SerializeField] private EnemyStatusEffect disableBuffs;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private ParticleSystem disableExplosionVFX;
    [SerializeField] private ParticleSystem disableVFX;
    [SerializeField] private SphereCollider sphereCollider;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();

    public void Initialize()
    {
        sphereCollider.radius = disableRadius;

        disableExplosionVFX.Play();
        disableVFX.Play();

        Destroy(gameObject, duration);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IEnemy>(out var enemy)) return;
        int id = enemy.GetInstanceID();
        if (enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Add(id, enemy);

        enemy.ApplyEffect(disableBuffs);
    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IEnemy>(out var enemy)) return;
        int id = enemy.GetInstanceID();
        if (!enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Remove(id);

        enemy.RemoveEffect(disableBuffs.type);
    }

    private void OnDestroy()
    {
        foreach (var enemy in enemiesInRange.Values)
        {
            enemy?.RemoveEffect(disableBuffs.type);
        }
        enemiesInRange.Clear();
    }
}
