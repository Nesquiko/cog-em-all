using System.Collections.Generic;
using UnityEngine;

public enum SkillTypes
{
    Wall = 0,
    OilSpill = 1,
    Mine = 2,
    AirshipAirstrike = 3,
    AirshipFreezeZone = 4,
    AirshipDisableZone = 5,
    MarkEnemy = 6,
    SuddenDeath = 7,
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
    ShatterCharge = 9,
}

public enum SkillActivationMode
{
    Placement,  // Place directly on ground (wall, oilspill, mine)
    Airship,    // Pick a location, airship takes over (airstrike, freeze, disable)
    Raycast,    // Hover & click an enemy (mark)
    Instant,    // Immediate activation (sudden death)
}

public interface ISkill
{
    SkillTypes SkillType();
    SkillActivationMode ActivationMode();
    float GetCooldown();
}

public interface ISkillPlaceable : ISkill
{
    void Initialize();
    Quaternion PlacementRotationOffset();
}

public interface IAirshipPayload
{
    void DropFromAirship(Vector3 targetPosition, float dropDuration);
}

public static class SkillMechanics
{
    public static HashSet<SkillModifiers> ActiveModifiersFromSkillType(SkillTypes type)
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
}