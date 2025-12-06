using System.Collections.Generic;
using UnityEngine;

public class FactionSkillTreeUI : MonoBehaviour
{
    [SerializeField] private List<Faction> factions;
    [SerializeField] private List<SkillTree> skillTrees;

    public void Initialize(Faction factionToDisplay, FactionSaveState factionSave)
    {
        for (int i = 0; i < factions.Count; i++)
        {
            var isInitialized = factions[i] == factionToDisplay;
            var skillTree = skillTrees[i];
            if (isInitialized)
            {
                skillTree.Initialize(factionSave);
            }
            skillTree.gameObject.SetActive(isInitialized);
        }
    }

    public void SaveSkillTrees()
    {
        foreach (var skillTree in skillTrees)
        {
            skillTree.SaveCurrentState();
        }
    }
}
