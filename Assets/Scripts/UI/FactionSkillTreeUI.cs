using System;
using System.Collections.Generic;
using UnityEngine;

public class FactionSkillTreeUI : MonoBehaviour
{
    [Serializable]
    private class Entry
    {
        public Faction faction;
        public SkillTree skillTree;
    }

    [SerializeField] private List<Entry> entries = new();

    private Dictionary<Faction, SkillTree> skillTreesByFaction;
    private Faction currentFaction;
    private SaveData saveData;

    public void Initialize(Faction factionToDisplay, SaveData saveData)
    {
        BuildCache();
        this.saveData = saveData;
        this.currentFaction = factionToDisplay;

        foreach (var kvp in skillTreesByFaction)
        {
            kvp.Value.gameObject.SetActive(false);
        }

        if (!skillTreesByFaction.TryGetValue(factionToDisplay, out var skillTree))
        {
            Debug.LogError($"no SkillTree configured for faction {factionToDisplay} in {name}");
            return;
        }

        FactionSaveState factionSave = factionToDisplay switch
        {
            Faction.TheBrassArmy => saveData.brassArmySave,
            Faction.TheValveboundSeraphs => saveData.seraphsSave,
            Faction.OverpressureCollective => saveData.overpressuSave,
            _ => throw new ArgumentOutOfRangeException(nameof(factionToDisplay), factionToDisplay, "Unhandled faction"),
        };

        skillTree.Initialize(factionSave);
        skillTree.gameObject.SetActive(true);
    }

    public void SaveSkillTrees()
    {
        switch (currentFaction)
        {
            case Faction.TheBrassArmy:
                var brassArmySkillTree = skillTreesByFaction[Faction.TheBrassArmy];
                saveData.brassArmySave = new FactionSaveState(brassArmySkillTree.Level, brassArmySkillTree.SkillNodes);
                break;
            case Faction.TheValveboundSeraphs:
                var seraphsSkillTree = skillTreesByFaction[Faction.TheValveboundSeraphs];
                saveData.seraphsSave = new FactionSaveState(seraphsSkillTree.Level, seraphsSkillTree.SkillNodes);
                break;
            case Faction.OverpressureCollective:
                var overpressureSkillTree = skillTreesByFaction[Faction.OverpressureCollective];
                saveData.overpressuSave = new FactionSaveState(overpressureSkillTree.Level, overpressureSkillTree.SkillNodes);
                break;

        }

        SaveSystem.UpdateSave(saveData);
    }

    private void BuildCache()
    {
        skillTreesByFaction = new Dictionary<Faction, SkillTree>();

        foreach (var entry in entries)
        {
            skillTreesByFaction[entry.faction] = entry.skillTree;
        }
    }
}


// public void SaveCurrentState()
// {
//     // TODO: luky -> tu mi uloz skillNodes do jsonu (toto sa vola ked odidem zo skilltree panelu)
//     Debug.Log($"[{faction}] Save current state: {skillNodes}");
// }