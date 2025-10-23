using UnityEngine;

public class FlamethrowerRangeTrigger : MonoBehaviour
{
    public FlamethrowerTower owner;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Enemy>(out var e)) return;

        owner.RegisterInRange(e);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Enemy>(out var e)) return;

        owner.UnregisterOutOfRange(e);
    }
}
