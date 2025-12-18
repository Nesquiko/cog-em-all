using Unity.Cinemachine;
using UnityEngine;

public enum ShakeLength
{
    Short,
    Medium,
    Long,
}

public enum ShakeIntensity
{
    Low,
    Medium,
    High,
    Extreme,
}

[RequireComponent(typeof(CinemachineCamera))]
public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake Instance { get; private set; }

    private CinemachineCamera cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin perlin;
    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        cinemachineCamera = GetComponent<CinemachineCamera>();
        perlin = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(ShakeIntensity intensity, ShakeLength length)
    {
        perlin.FrequencyGain = GetFrequency(intensity);
        Shake(GetIntensity(intensity), GetLength(length));
    }

    public void Shake(float intensity, float time)
    {
        startingIntensity = intensity;
        shakeTimerTotal = time;
        shakeTimer = time;

        perlin.AmplitudeGain = Mathf.Clamp(intensity, 0f, GetIntensity(ShakeIntensity.Extreme));
        perlin.FrequencyGain = 2.5f;
    }

    private void Update()
    {
        if (shakeTimer <= 0f) return;

        shakeTimer -= Time.deltaTime;

        float progress = 1f - (shakeTimer / shakeTimerTotal);
        perlin.AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, progress);

        if (shakeTimer <= 0f)
            perlin.AmplitudeGain = 0f;
    }

    private static float GetLength(ShakeLength length) => length switch
    {
        ShakeLength.Short => 0.3f,
        ShakeLength.Medium => 0.8f,
        ShakeLength.Long => 1.5f,
        _ => 0.3f,
    };

    private static float GetIntensity(ShakeIntensity intensity) => intensity switch
    {
        ShakeIntensity.Low => 1f,
        ShakeIntensity.Medium => 2f,
        ShakeIntensity.High => 3f,
        ShakeIntensity.Extreme => 4f,
        _ => 0.5f,
    };

    private static float GetFrequency(ShakeIntensity intensity) => intensity switch
    {
        ShakeIntensity.Low => 2.5f,
        ShakeIntensity.Medium => 4f,
        ShakeIntensity.High => 5.5f,
        _ => 2.5f,
    };
}
