using UnityEngine;

public class MortarRangeTrigger : MonoBehaviour
{
    public bool isInnerZone;
    public MortarTower owner;

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IEnemy>(out var e)) return;

        if (isInnerZone)
            owner.RegisterTooClose(e);
        else
            owner.RegisterInRange(e);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IEnemy>(out var e)) return;

        if (isInnerZone)
            owner.UnregisterTooClose(e);
        else
            owner.UnregisterOutOfRange(e);
    }
}
