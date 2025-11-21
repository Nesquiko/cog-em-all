using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "SkillDataCatalog", menuName = "Scriptable Objects/Skill Data Catalog")]
public class SkillDataCatalog : ScriptableObject
{
    [SerializeField, Tooltip("List of skills available in the game.")]
    private List<SkillData> skills = new();

    private readonly Dictionary<SkillTypes, SkillData> catalog = new();

    public int SkillCount => skills.Count;

    private void OnEnable()
    {
        RebuildCatalog();
    }

    private void OnValidate()
    {
        RebuildCatalog();
    }

    private void RebuildCatalog()
    {
        catalog.Clear();

        foreach (var s in skills)
        {
            if (catalog.ContainsKey(s.type))
            {
                Debug.LogWarning($"duplicate SkillType detected: {s.type}");
                continue;
            }

            catalog[s.type] = s;
        }
    }

    public SkillData FromIndex(int i)
    {
        Assert.IsTrue(System.Enum.IsDefined(typeof(SkillTypes), i), $"invalid SkillType value: {i}");
        return FromType((SkillTypes)i);
    }

    public SkillData FromType(SkillTypes type)
    {
        Assert.IsNotNull(catalog);
        Assert.IsTrue(catalog.ContainsKey(type));
        var skillData = catalog[type];
        Assert.IsNotNull(skillData);
        return skillData;
    }

    public (HashSet<SkillTypes>, HashSet<SkillTypes>) AdjustSkills(int gears)
    {
        HashSet<SkillTypes> toEnable = new();
        HashSet<SkillTypes> toDisable = new();

        foreach (SkillData skill in skills)
        {
            if (gears >= skill.cost)
            {
                toEnable.Add(skill.type);
            }
            else
            {
                toDisable.Add(skill.type);
            }
        }

        return (toEnable, toDisable);
    }
}
