using UnityEngine;

public class SuddenDeath : MonoBehaviour, ISkill
{
    [SerializeField, Range(1f, 2f)] private float gearRewardMultiplier = 1.25f;

    public SkillTypes SkillType() => SkillTypes.SuddenDeath;
    public SkillActivationMode ActivationMode() => SkillActivationMode.Instant;
    public float GetCooldown() => Mathf.Infinity;

    private bool canActivate = true;
    public float GearRewardMultiplier => gearRewardMultiplier;

    public bool Activate()
    {
        if (!canActivate) return false;
        canActivate = false;
        return true;
    }
}
