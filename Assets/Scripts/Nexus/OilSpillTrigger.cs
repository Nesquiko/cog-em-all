using UnityEngine;

public class OilSpillTrigger : MonoBehaviour
{
    public OilSpill owner;

    public void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IEnemy>(out var e)) return;
        owner.RegisterInRange(e);
    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IEnemy>(out var e)) return;
        owner.UnregisterOutOfRange(e);
    }
}
