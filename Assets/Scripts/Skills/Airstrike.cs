using UnityEngine;

public class Airstrike : MonoBehaviour, IDamageSource
{
    [SerializeField] private float damage = 300f;
    [SerializeField] private float airstrikeRadius = 7.5f;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private ParticleSystem explosionVFX;

    public DamageSourceType Type() => DamageSourceType.Airstrike;

    public void Initialize()
    {
        explosionVFX.Play();

        Destroy(gameObject, 3);

        Collider[] hits = Physics.OverlapSphere(transform.position, airstrikeRadius, enemyMask);
        foreach (var h in hits)
        {
            if (h.TryGetComponent<IEnemy>(out var enemy))
            {
                enemy.TakeDamage(damage, Type());
            }
        }
    }
}
