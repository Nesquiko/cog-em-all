using UnityEngine;

[CreateAssetMenu(fileName = "StickityStickModifier", menuName = "Skill Modifiers/Stickity Stick Modifier")]
public class StickityStickModifier : ScriptableObject, ISkillModifier
{
    [Header("Generic")]
    [SerializeField] private SkillTypes skillType = SkillTypes.OilSpill;
    [SerializeField] private SkillModifiers modifier = SkillModifiers.StickityStick;
    [SerializeField] private string displayName = "Stickity Stick";
    [SerializeField, TextArea] private string description = "Stickity Stick description";

    [Header("Specific")]
    public float speedMultiplier = 0.5f;

    public SkillTypes SkillType => skillType;
    public SkillModifiers Modifier => modifier;
    public string DisplayName => displayName;
    public string Description => description;
}
