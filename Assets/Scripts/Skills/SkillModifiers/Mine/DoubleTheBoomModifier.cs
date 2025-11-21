using UnityEngine;

[CreateAssetMenu(fileName = "DoubleTheBoomModifier", menuName = "Skill Modifiers/Double The Boom Modifier")]
public class DoubleTheBoomModifier : ScriptableObject, ISkillModifier
{
    [Header("Generic")]
    [SerializeField] private SkillTypes skillType = SkillTypes.Mine;
    [SerializeField] private SkillModifiers modifier = SkillModifiers.DoubleTheBoom;
    [SerializeField] private string displayName = "Double the Boom";
    [SerializeField, TextArea] private string description = "Double the Boom description";

    [Header("Specific")]
    public int explosionCount = 2;
    public float damageFractionPerExplosion = 0.75f;

    public SkillTypes SkillType => skillType;
    public SkillModifiers Modifier => modifier;
    public string DisplayName => displayName;
    public string Description => description;
}
