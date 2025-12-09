using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeZone : MonoBehaviour
{
    [SerializeField] private float freezeRadius = 15f;
    [SerializeField] private float duration = 10f;
    [SerializeField] private EnemyStatusEffect freeze;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private ParticleSystem freezeExplosionVFX;
    [SerializeField] private ParticleSystem freezeVFX;
    [SerializeField] private SphereCollider sphereCollider;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();

    public void Initialize()
    {
        sphereCollider.radius = freezeRadius;

        freezeExplosionVFX.Play();
        freezeVFX.Play();

        Destroy(gameObject, duration);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IEnemy>(out var enemy)) return;
        int id = enemy.GetInstanceID();
        if (enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Add(id, enemy);

        enemy.ApplyEffect(freeze);
    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IEnemy>(out var enemy)) return;
        int id = enemy.GetInstanceID();
        if (!enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Remove(id);

        enemy.RemoveEffect(freeze.type);
    }

    private void OnDestroy()
    {
        foreach (var enemy in enemiesInRange.Values)
        {
            enemy?.RemoveEffect(freeze.type);
        }
        enemiesInRange.Clear();
    }
}
