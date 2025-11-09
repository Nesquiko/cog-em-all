using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraShakeSystem : MonoBehaviour
{
    [Header("Impulse Settings")]
    [SerializeField] private float defaultDuration = 0.5f;
    [SerializeField] private float defaultAmplitude = 2f;
    [SerializeField] private float defaultFrequency = 2f;

    private CinemachineImpulseDefinition impulseDef;

    private void Awake()
    {
        impulseDef = new CinemachineImpulseDefinition
        {
            ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion,
            ImpulseDuration =       defaultDuration,
            AmplitudeGain =         defaultAmplitude,
            FrequencyGain =         defaultFrequency,
            DissipationDistance =   500f,
            ImpactRadius =          500f,
            ImpulseType =           CinemachineImpulseDefinition.ImpulseTypes.Legacy
        };
    }

    private void Start()
    {
        Debug.Log("Shaking camera via Cinemachine impulse");
        Shake(defaultDuration, defaultAmplitude, defaultFrequency);
    }

    public void Shake(float duration, float amplitude, float frequency)
    {
        GameObject temporaryImpulseSource = new("TemporaryImpulseSource");
        var source = temporaryImpulseSource.AddComponent<CinemachineImpulseSource>();

        impulseDef.ImpulseDuration = duration;
        impulseDef.AmplitudeGain = amplitude;
        impulseDef.FrequencyGain = frequency;

        source.ImpulseDefinition = impulseDef;

        const float scalar = 1f;
        impulseDef.CreateEvent(Vector3.zero, Vector3.down * scalar);

        Destroy(temporaryImpulseSource, duration + 0.25f);
    }
}
