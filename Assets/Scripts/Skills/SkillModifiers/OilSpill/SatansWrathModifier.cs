using UnityEngine;

[CreateAssetMenu(fileName = "SatansWrathModifier", menuName = "Skill Modifiers/Satan's Wrath Modifier")]
public class SatansWrathModifier : ScriptableObject, ISkillModifier
{
    [Header("Generic")]
    [SerializeField] private SkillTypes skillType = SkillTypes.OilSpill;
    [SerializeField] private SkillModifiers modifier = SkillModifiers.SatansWrath;
    [SerializeField] private string displayName = "Satan's Wrath";
    [SerializeField, TextArea] private string description = "Satan's Wrath description";

    [Header("Specific")]
    public EnemyStatusEffect oilBurn = EnemyStatusEffect.OilBurn();

    public SkillTypes SkillType => skillType;
    public SkillModifiers Modifier => modifier;
    public string DisplayName => displayName;
    public string Description => description;
}
