using System.Collections;
using UnityEngine;

public class AirshipController : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float flightDuration = 2f;
    [SerializeField] private float flightHeight = 150f;
    [SerializeField] private float flightStartX = -600f;
    [SerializeField] private float flightEndX = 1100f;

    [Header("Payload")]
    [SerializeField] private float payloadDropDuration = 1f;
    [SerializeField] private Transform payloadDropPoint;

    private bool isFlying;

    public void FlyAcross(
        Vector3 targetPosition,
        GameObject payloadPrefab
    )
    {
        if (isFlying) return;

        gameObject.SetActive(true);
        StartCoroutine(FlyRoutine(targetPosition, payloadPrefab));
    }

    private IEnumerator FlyRoutine(
        Vector3 targetPosition,
        GameObject payloadPrefab
    )
    {
        isFlying = true;

        Vector3 startPosition = new(
            flightStartX,
            targetPosition.y + flightHeight,
            targetPosition.z
        );

        Vector3 endPosition = new(
            flightEndX,
            startPosition.y,
            startPosition.z
        );

        float midX = (flightStartX + flightEndX) * 0.5f;

        transform.position = startPosition;
        float t = 0f;
        bool dropped = false;

        while (t < 1f)
        {
            t += Time.deltaTime / flightDuration;
            t = Mathf.Clamp01(t);

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            if (!dropped && transform.position.x >= midX)
            {
                dropped = true;
                DropPayload(targetPosition, payloadPrefab);
            }

            yield return null;
        }

        isFlying = false;
        gameObject.SetActive(false);
    }

    private void DropPayload(
        Vector3 targetPosition,
        GameObject payloadPrefab
    )
    {
        Vector3 startPosition = payloadDropPoint.position;
        Vector3 direction = (targetPosition - startPosition).normalized;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);

        GameObject payloadGO = Instantiate(
            payloadPrefab,
            payloadDropPoint.position,
            rotation
        );

        if (!payloadGO.TryGetComponent<IAirshipPayload>(out var payload)) return;

        SoundManagersDontDestroy.GerOrCreate()?.SoundFX.PlaySoundFXClip(SoundFXType.AirshipDrop, transform);

        payload.DropFromAirship(targetPosition, payloadDropDuration);
    }
}
