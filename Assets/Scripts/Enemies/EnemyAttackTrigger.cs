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
        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;

        owner.EnterAttackRange(damageable);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;

        owner.ExitAttackRange(damageable);
    }
}
