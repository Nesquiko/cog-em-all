using UnityEngine;

public class EnemyAttackTrigger : MonoBehaviour
{
    [SerializeField] private GameObject ownerGO;
    private IEnemy owner;

    void Awake()
    {
        owner = ownerGO.GetComponent<IEnemy>();
    }

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
