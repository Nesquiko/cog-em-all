using UnityEngine;

public class MarkEnemy : MonoBehaviour, ISkill
{
    public SkillTypes SkillType() => SkillTypes.MarkEnemy;
    public SkillActivationMode ActivationMode() => SkillActivationMode.Raycast;
    public float GetCooldown() => 60f;
}
