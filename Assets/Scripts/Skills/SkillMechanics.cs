using UnityEngine;

public enum SkillTypes
{
    Wall = 0,
    OilSpill = 1,
    Mine = 2,
}

public interface ISkillModifier
{
    SkillTypes SkillType { get; }
    SkillModifiers Modifier { get; }
    string DisplayName { get; }
    string Description { get; }
}

public enum SkillModifiers
{
    SteelReinforcement = 0,
    SharpThorns = 1,
    LeftoverDebris = 2,
    SatansWrath = 3,
    GooeyGoo = 4,
    StickityStick = 5,
    DoubleTheBoom = 6,
    WideDestruction = 7,
    QuickFuse = 8,
}

public interface ISkill
{
    SkillTypes SkillType();
    float GetCooldown();
}

public interface ISkillPlaceable : ISkill
{
    void Initialize();
    Quaternion PlacementRotationOffset();
}