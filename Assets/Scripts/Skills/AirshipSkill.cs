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

    public void Initialize(AirshipSkillType type, Vector3 startPos, Quaternion rot, Vector3 targetPos)
    {
        startPosition = startPos;
        rotation = rot;
        targetPosition = targetPos;

        GameObject airshipGO = Instantiate(payloadPrefab, startPosition, rotation);

        if (!airshipGO.TryGetComponent<IAirshipPayload>(out var payload)) return;
            
        payload.DropFromAirship(targetPosition, dropDuration);
    }
}
