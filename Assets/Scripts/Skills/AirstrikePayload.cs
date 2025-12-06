using System.Collections;
using UnityEngine;

public class AirstrikePayload : MonoBehaviour, IAirshipPayload, IDamageSource
{
    [SerializeField] private float explosionRadius = 7.5f;
    [SerializeField] private float damage = 400f;
    [SerializeField] private LayerMask enemyMask;

    public DamageSourceType Type() => DamageSourceType.Airstrike;

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

        Destroy(gameObject, 2f);
    }

    private void OnArrive(Vector3 target)
    {
        Collider[] hits = Physics.OverlapSphere(target, explosionRadius, enemyMask);
        foreach (var h in hits)
        {
            if (h.TryGetComponent<IEnemy>(out var enemy))
            {
                enemy.TakeDamage(damage, Type());
            }
        }
    }
}
