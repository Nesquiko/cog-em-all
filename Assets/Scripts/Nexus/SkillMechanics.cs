using UnityEngine;

public enum SkillTypes
{
    Wall = 0,
    OilSpill = 1,
    Mine = 2,
}

public interface ISkillPlaceable
{
    void Initialize();
    SkillTypes SkillType { get; }
    Quaternion PlacementRotationOffset { get; }
}