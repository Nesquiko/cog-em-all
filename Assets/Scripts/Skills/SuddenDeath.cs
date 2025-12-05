using UnityEngine;

public class SuddenDeath : MonoBehaviour, ISkill
{
    public SkillTypes SkillType() => SkillTypes.SuddenDeath;
    public SkillActivationMode ActivationMode() => SkillActivationMode.Instant;
    public float GetCooldown() => Mathf.Infinity;

    private void OnEnable()
    {
        // TODO: activate sudden death
    }

    private void OnDisable()
    {
        // TODO: cancel sudden death
    }
}
