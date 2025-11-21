using UnityEngine;

[CreateAssetMenu(fileName = "QuickFuseModifier", menuName = "Skill Modifiers/Quick Fuse Modifier")]
public class QuickFuseModifier : ScriptableObject, ISkillModifier
{
    [Header("Generic")]
    [SerializeField] private SkillTypes skillType = SkillTypes.Mine;
    [SerializeField] private SkillModifiers modifier = SkillModifiers.QuickFuse;
    [SerializeField] private string displayName = "Quick Fuse";
    [SerializeField, TextArea] private string description = "Quick Fuse description";

    [Header("Specific")]
    public float triggerSpeedMultiplier = 2f;

    public SkillTypes SkillType => skillType;
    public SkillModifiers Modifier => modifier;
    public string DisplayName => displayName;
    public string Description => description;
}
