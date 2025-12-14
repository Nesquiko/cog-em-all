using System.Collections;
using UnityEngine;

public class AirstrikePayload : MonoBehaviour, IAirshipPayload
{
    [SerializeField] private GameObject airstrikePrefab;

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
        GameObject airstrike = Instantiate(airstrikePrefab, target, Quaternion.identity);
        airstrike.GetComponent<Airstrike>().Initialize();
        SoundManagersDontDestroy.GerOrCreate().SoundFX.PlaySoundFXClip(SoundFXType.AirshipHit, airstrike.transform);
    }
}
