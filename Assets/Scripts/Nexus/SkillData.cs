using System;
using UnityEngine;

[Serializable]
public class SkillData
{
    public SkillTypes type;
    public string displayName;
    [TextArea] public string description;
    public int cost;

    public SkillData(SkillTypes type, string displayName, string description, int cost)
    {
        this.type = type;
        this.displayName = displayName;
        this.description = description;
        this.cost = cost;
    }
}
