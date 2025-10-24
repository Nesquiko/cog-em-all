using UnityEngine;

public class EnemyAttackTrigger : MonoBehaviour
{
    public Enemy owner;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Nexus>(out var nexus)) return;

        owner.EnterAttackRange(nexus);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Nexus>(out var nexus)) return;

        owner.ExitAttackRange(nexus);
    }
}
