using UnityEngine;

public class EnemyAttackTrigger : MonoBehaviour
{
    private Enemy owner;

    private void Awake()
    {
        owner = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Nexus nexus)) return;

        owner.EnterAttackRange(nexus);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out Nexus nexus)) return;

        owner.ExitAttackRange(nexus);
    }
}
