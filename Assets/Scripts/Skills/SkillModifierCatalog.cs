using System.Collections.Generic;
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

    public HashSet<SkillModifiers> ModifiersFromSkillType(SkillTypes type)
    {
        return type switch
        {
            SkillTypes.Wall => new()
            {
                SkillModifiers.SteelReinforcement,
                SkillModifiers.SharpThorns,
                SkillModifiers.LeftoverDebris,
            },
            SkillTypes.OilSpill => new()
            {
                SkillModifiers.SatansWrath,
                SkillModifiers.GooeyGoo,
                SkillModifiers.StickityStick,
            },
            SkillTypes.Mine => new()
            {
                SkillModifiers.DoubleTheBoom,
                SkillModifiers.WideDestruction,
                SkillModifiers.QuickFuse,
            },
            _ => null,
        };
    }

    // TODO: luky -> fill this with actual active modifiers
    public HashSet<SkillModifiers> ActiveModifiersFromSkillType(SkillTypes type)
    {
        return type switch
        {
            SkillTypes.Wall => new()
            {
                SkillModifiers.SteelReinforcement,
                SkillModifiers.SharpThorns,
                SkillModifiers.LeftoverDebris,
            },
            SkillTypes.OilSpill => new()
            {
                SkillModifiers.SatansWrath,
                SkillModifiers.GooeyGoo,
                SkillModifiers.StickityStick,
            },
            SkillTypes.Mine => new()
            {
                SkillModifiers.DoubleTheBoom,
                SkillModifiers.WideDestruction,
                SkillModifiers.QuickFuse,
            },
            _ => null,
        };
    }

    public readonly Dictionary<SkillTypes, int[]> skillModifierIndices = new()
    {
        { SkillTypes.Wall, new[] { 0, 1, 2 } },
        { SkillTypes.OilSpill, new[] { 3, 4, 5 } },
        { SkillTypes.Mine, new[] { 6, 7, 8 } },
    };

    public SkillModifiers ModifierEnumFromIndex(int i)
    {
        return i switch
        {
            0 => SkillModifiers.SteelReinforcement,
            1 => SkillModifiers.SharpThorns,
            2 => SkillModifiers.LeftoverDebris,
            3 => SkillModifiers.SatansWrath,
            4 => SkillModifiers.GooeyGoo,
            5 => SkillModifiers.StickityStick,
            6 => SkillModifiers.DoubleTheBoom,
            7 => SkillModifiers.WideDestruction,
            8 => SkillModifiers.QuickFuse,
            _ => default,
        };
    }
}
