using UnityEngine;

public class EnemyAttackTrigger : MonoBehaviour
{
    public Enemy owner;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;

        owner.EnterAttackRange(damageable);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;

        owner.ExitAttackRange(damageable);
    }
}
