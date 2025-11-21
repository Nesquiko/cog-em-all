using UnityEngine;

[CreateAssetMenu(fileName = "SteelReinforcementModifier", menuName = "Skill Modifiers/Steel Reinforcement Modifier")]
public class SteelReinforcementModifier : ScriptableObject, ISkillModifier
{
    [Header("Generic")]
    [SerializeField] private SkillTypes skillType = SkillTypes.Wall;
    [SerializeField] private SkillModifiers modifier = SkillModifiers.SteelReinforcement;
    [SerializeField] private string displayName = "Steel Reinforcement";
    [SerializeField, TextArea] private string description = "Steel Reinforcement description";

    [Header("Specific")]
    public float healthPointsMultiplier = 2f;

    public SkillTypes SkillType => skillType;
    public SkillModifiers Modifier => modifier;
    public string DisplayName => displayName;
    public string Description => description;
}
