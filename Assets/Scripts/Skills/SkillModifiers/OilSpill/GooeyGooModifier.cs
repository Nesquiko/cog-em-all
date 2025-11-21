using UnityEngine;

[CreateAssetMenu(fileName = "GooeyGooModifier", menuName = "Skill Modifiers/Gooey Goo Modifier")]
public class GooeyGooModifier : ScriptableObject, ISkillModifier
{
    [Header("Generic")]
    [SerializeField] private SkillTypes skillType = SkillTypes.OilSpill;
    [SerializeField] private SkillModifiers modifier = SkillModifiers.GooeyGoo;
    [SerializeField] private string displayName = "Gooey Goo";
    [SerializeField, TextArea] private string description = "Gooey Goo description";

    [Header("Specific")]
    public EnemyStatusEffect gooeySlow = EnemyStatusEffect.Slow;

    public SkillTypes SkillType => skillType;
    public SkillModifiers Modifier => modifier;
    public string DisplayName => displayName;
    public string Description => description;
}
