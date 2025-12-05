using UnityEngine;

public class FreezeZone : AirshipBase
{
    [SerializeField] private GameObject freezeZonePrefab;
    [SerializeField] private float freezeDuration = 10f;

    public override SkillTypes SkillType() => SkillTypes.AirshipFreezeZone;
    public override float GetCooldown() => 120f;

    protected override void OnAirshipSkillArrived(GameObject airship, Vector3 target)
    {
        Instantiate(freezeZonePrefab, target, Quaternion.identity);
        Destroy(airship, 2f);
    }
}
