using UnityEngine;

public class FactionSkillTreeUI : MonoBehaviour
{
    [SerializeField] private Faction[] factions;
    [SerializeField] private GameObject[] skillTrees;

    public void Initialize(Faction factionToDisplay)
    {
        for (int i = 0; i < factions.Length; i++)
        {
            skillTrees[i].SetActive(factions[i] == factionToDisplay);
        }
    }

    public void SaveSkillTrees()
    {
        foreach (var skillTreeGO in skillTrees)
        {
            var skillTree = skillTreeGO.GetComponent<SkillTree>();
            skillTree.SaveCurrentState();
        }
    }
}
