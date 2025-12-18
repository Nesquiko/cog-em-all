using System.Collections.Generic;
using UnityEngine;

public class FreezeZone : MonoBehaviour
{
    [SerializeField] private float freezeRadius = 15f;
    [SerializeField] private float duration = 10f;
    [SerializeField, Range(1f, 10f)] private float mineDamageMultiplier = 3f;
    [SerializeField] private EnemyStatusEffect freeze;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private ParticleSystem freezeExplosionVFX;
    [SerializeField] private ParticleSystem freezeVFX;
    [SerializeField] private SphereCollider sphereCollider;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();
    private bool exploded = false;

    public void Initialize()
    {
        sphereCollider.radius = freezeRadius;

        CinemachineShake.Instance.Shake(ShakeIntensity.High, ShakeLength.Long);
        freezeExplosionVFX.Play();
        freezeVFX.Play();

        Destroy(gameObject, duration);
    }

    public void ShatterTheIce(float mineDamage)
    {
        if (exploded) return;
        exploded = true;

        CinemachineShake.Instance.Shake(ShakeIntensity.High, ShakeLength.Long);
        freezeExplosionVFX.Play();
        freezeVFX.Stop(withChildren: true);

        Collider[] hits = Physics.OverlapSphere(transform.position, freezeRadius, enemyMask);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IEnemy>(out var enemy))
            {
                float shatterDamage = mineDamage * mineDamageMultiplier;
                enemy.TakeDamage(shatterDamage, DamageSourceType.IceShatter, false);
            }
        }

        Destroy(gameObject, 0.5f);
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
