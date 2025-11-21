using UnityEngine;

[CreateAssetMenu(fileName = "WideDestructionModifier", menuName = "Skill Modifiers/Wide Destruction Modifier")]
public class WideDestructionModifier : ScriptableObject, ISkillModifier
{
    [Header("Generic")]
    [SerializeField] private SkillTypes skillType = SkillTypes.Mine;
    [SerializeField] private SkillModifiers modifier = SkillModifiers.WideDestruction;
    [SerializeField] private string displayName = "Wide Destruction";
    [SerializeField, TextArea] private string description = "Wide Destruction description";

    [Header("Specific")]
    public float explosionRadius = 25f;

    public SkillTypes SkillType => skillType;
    public SkillModifiers Modifier => modifier;
    public string DisplayName => displayName;
    public string Description => description;
}
