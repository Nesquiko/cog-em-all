using System.Collections;
using UnityEngine;

public class DisableZonePayload : MonoBehaviour, IAirshipPayload
{
    [SerializeField] private GameObject disableZonePrefab;

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
        GameObject disableZone = Instantiate(disableZonePrefab, target, Quaternion.identity);
        disableZone.GetComponent<DisableZone>().Initialize();
    }
}
