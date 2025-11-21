using UnityEngine;

[CreateAssetMenu(fileName = "SharpThornsModifier", menuName = "Skill Modifiers/Sharp Thorns Modifier")]
public class SharpThornsModifier : ScriptableObject, ISkillModifier
{
    [Header("Generic")]
    [SerializeField] private SkillTypes skillType = SkillTypes.Wall;
    [SerializeField] private SkillModifiers modifier = SkillModifiers.SharpThorns;
    [SerializeField] private string displayName = "Sharp Thorns";
    [SerializeField, TextArea] private string description = "Sharp Thorns description";

    [Header("Specific")]
    public float fractionToReturn = 0.1f;

    public SkillTypes SkillType => skillType;
    public SkillModifiers Modifier => modifier;
    public string DisplayName => displayName;
    public string Description => description;
}
