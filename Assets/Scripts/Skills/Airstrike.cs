using UnityEngine;

public class Airstrike : AirshipBase
{
    [SerializeField] private GameObject airstrikePrefab;
    [SerializeField] private float damageRadius = 10f;

    public override SkillTypes SkillType() => SkillTypes.AirshipAirstrike;
    public override float GetCooldown() => 120f;

    protected override void OnAirshipSkillArrived(GameObject airship, Vector3 target)
    {
        Instantiate(airstrikePrefab, target, Quaternion.identity);
        Destroy(airship, 2f);
    }
}
