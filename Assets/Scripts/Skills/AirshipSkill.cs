using UnityEngine;

public enum AirshipSkillType
{
    Airstrike = 0,
    FreezeZone = 1,
    DisableZone = 2,
}

public class AirshipSkill : MonoBehaviour, ISkill
{
    [SerializeField] private SkillTypes skillType;

    public SkillTypes SkillType() => skillType;
    public float GetCooldown() => 120f;
    public SkillActivationMode ActivationMode() => SkillActivationMode.Airship;

    [Header("Prefabs")]
    [SerializeField] private GameObject payloadPrefab;

    [Header("Airship Settings")]
    [SerializeField] private float dropDuration = 2f;

    private Vector3 startPosition;
    private Quaternion rotation;
    private Vector3 targetPosition;

    public void Initialize(Vector3 startPos, Vector3 targetPos)
    {
        startPosition = startPos;
        targetPosition = targetPos;
        rotation = Quaternion.LookRotation((targetPosition - startPosition).normalized, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);

        GameObject airshipGO = Instantiate(payloadPrefab, startPosition, rotation);

        if (!airshipGO.TryGetComponent<IAirshipPayload>(out var payload)) return;
            
        payload.DropFromAirship(targetPosition, dropDuration);

        SoundManagersDontDestroy.GerOrCreate().SoundFX.PlaySoundFXClip(SoundFXType.AirshipDrop, airshipGO.transform);
    }
}
