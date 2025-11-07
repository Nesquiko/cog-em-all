using UnityEngine;
using System.Collections;

public class CameraShakeSystem : MonoBehaviour
{
    private Vector3 originalPosition;
    private Coroutine currentShakeRoutine;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        if (currentShakeRoutine != null)
        {
            StopCoroutine(currentShakeRoutine);
            transform.localPosition = originalPosition;
        }

        currentShakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = originalPosition + new Vector3(x, y, 0);
            yield return null;
        }

        transform.localPosition = originalPosition;
        currentShakeRoutine = null;
    }
}
