using UnityEngine;

[CreateAssetMenu(fileName = "SkillModifierCatalog", menuName = "Scriptable Objects/Skill Modifier Catalog")]
public class SkillModifierCatalog : ScriptableObject
{
    [Header("Wall Modifiers")]
    [SerializeField] private SteelReinforcementModifier steelReinforcementModifier;
    [SerializeField] private SharpThornsModifier sharpThornsModifier;
    [SerializeField] private LeftoverDebrisModifier leftoverDebrisModifier;

    [Header("Oil Spill Modifiers")]
    [SerializeField] private SatansWrathModifier satansWrathModifier;
    [SerializeField] private GooeyGooModifier gooeyGooModifier;
    [SerializeField] private StickityStickModifier stickityStickModifier;

    [Header("Mine Modifiers")]
    [SerializeField] private DoubleTheBoomModifier doubleTheBoomModifier;
    [SerializeField] private WideDestructionModifier wideDestructionModifier;
    [SerializeField] private QuickFuseModifier quickFuseModifier;

    public ScriptableObject FromSkillAndModifier(SkillTypes type, SkillModifiers modifier)
    {
        return type switch
        {
            SkillTypes.Wall => modifier switch
            {
                SkillModifiers.SteelReinforcement => steelReinforcementModifier,
                SkillModifiers.SharpThorns => sharpThornsModifier,
                SkillModifiers.LeftoverDebris => leftoverDebrisModifier,
                _ => null,
            },
            SkillTypes.OilSpill => modifier switch
            {
                SkillModifiers.SatansWrath => satansWrathModifier,
                SkillModifiers.GooeyGoo => gooeyGooModifier,
                SkillModifiers.StickityStick => stickityStickModifier,
                _ => null,
            },
            SkillTypes.Mine => modifier switch
            {
                SkillModifiers.DoubleTheBoom => doubleTheBoomModifier,
                SkillModifiers.WideDestruction => wideDestructionModifier,
                SkillModifiers.QuickFuse => quickFuseModifier,
                _ => null,
            },
            _ => null,
        };
    }
}
