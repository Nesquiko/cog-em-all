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

    public void Initialize(AirshipController airshipController, Vector3 targetPos)
    {
        airshipController.FlyAcross(targetPos, payloadPrefab);
    }
}
