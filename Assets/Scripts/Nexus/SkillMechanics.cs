using UnityEngine;

public enum SkillTypes
{
    Wall = 0,
    OilSpill = 1,
    Mine = 2,
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