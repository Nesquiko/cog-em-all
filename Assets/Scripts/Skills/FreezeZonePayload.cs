using System.Collections;
using UnityEngine;

public class FreezeZonePayload : MonoBehaviour, IAirshipPayload
{
    [SerializeField] private GameObject freezeZonePrefab;
    [SerializeField] private float freezeRadius = 7.5f;
    [SerializeField] private float duration = 10f;
    [SerializeField] private LayerMask enemyMask;

    public void DropFromAirship(Vector3 targetPosition, float dropDuration)
    {
        StartCoroutine(Deliver(targetPosition, dropDuration));
    }

    private IEnumerator Deliver(Vector3 targetPosition, float dropDuration)
    {
        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / dropDuration;
            transform.position = Vector3.Lerp(start, targetPosition, t);
            yield return null;
        }

        OnArrive(targetPosition);

        Destroy(gameObject);
    }

    private void OnArrive(Vector3 target)
    {
        GameObject freezeZone = Instantiate(freezeZonePrefab, target, Quaternion.identity);
        Destroy(freezeZone, duration);

        Collider[] hits = Physics.OverlapSphere(target, freezeRadius, enemyMask);
        foreach (var h in hits)
        {
            if (h.TryGetComponent<IEnemy>(out var enemy))
            {
                // TODO: has to be freeze
                enemy.ApplyEffect(EnemyStatusEffect.Slow);
            }
        }
    }
}
