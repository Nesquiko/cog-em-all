using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraShakeSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin perlin;

    [Header("Defaults")]
    [SerializeField] private float defaultFrequency = 2f;

    private Coroutine shakeRoutine;

    public void Shake(float duration, float amplitude, float frequency = -1f)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, amplitude, frequency > 0f ? frequency : defaultFrequency));
    }

    private IEnumerator ShakeRoutine(float duration, float amplitude, float frequency)
    {
        perlin.AmplitudeGain = amplitude;
        perlin.FrequencyGain = frequency;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        perlin.AmplitudeGain = 0f;
        perlin.FrequencyGain = 0f;
        shakeRoutine = null;
    }
}
