using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class SkillTree : MonoBehaviour
{
    [SerializeField] private Faction faction;
    [SerializeField] private TMP_Text skillPointsText;
    [SerializeField] private Button resetSkillPointsButton;
    [SerializeField] private GameObject[] ranks;

    private int availableSkillPoints = 0;
    private int assignedSkillPoints = 0;

    private bool suppressEvents = false;

    public bool CanAssignSkillPoint => availableSkillPoints > 0;
    public bool CanRemoveSkillPoint => assignedSkillPoints > 0;

    private Dictionary<string, int> skillNodes = new();
    private int level = 15;

    private void Awake()
    {
        // skillNodes =  // TODO: luky -> sem musi prist dictionary <skillSlug, activeRanks>
        // level =  // TODO: luky -> sem musi prist faction level
        assignedSkillPoints = AssignSkillPoints(addActions: true);
        availableSkillPoints = CalculateAvailableSkillPoints();
    }

    private void Start()
    {
        UpdateVisual();
    }

    private int AssignSkillPoints(bool addActions)
    {
        int assigned = 0;

        foreach (var rank in ranks)
        {
            if (rank == null) continue;

            for (int i = 0; i < rank.transform.childCount; i++)
            {
                var nodeTransform = rank.transform.GetChild(i);
                
                if (!nodeTransform.TryGetComponent<SkillTreeNodeButton>(out var button)) continue;

                int activeRanks = 0;
                skillNodes.TryGetValue(button.SkillSlug, out activeRanks);
                button.SetActiveRanks(activeRanks);
                assigned += button.ActiveRanks;

                if (addActions)
                    button.OnActiveRanksChanged += UpdateSkillPoints;

                skillNodes[button.SkillSlug] = button.ActiveRanks;
            }
        }

        return assigned;
    }

    private int CalculateAvailableSkillPoints()
    {
        Assert.IsTrue(level >= assignedSkillPoints, $"Assigned more points than level (Level ({level}) is not greater than assigned skill points ({assignedSkillPoints}))");
        return level - assignedSkillPoints;
    }

    private void UpdateSkillPoints(SkillTreeNodeButton node, int delta)
    {
        if (suppressEvents) return;

        assignedSkillPoints += delta;
        availableSkillPoints -= delta;

        if (delta != 0)
            skillNodes[node.SkillSlug] = node.ActiveRanks;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        skillPointsText.text = $"Skill points:  {availableSkillPoints}";
        resetSkillPointsButton.interactable = assignedSkillPoints > 0;
    }

    public void ResetSkillPoints()
    {
        suppressEvents = true;
        assignedSkillPoints = 0;

        foreach (var rank in ranks)
        {
            if (rank == null) continue;

            for (int i = 0; i < rank.transform.childCount; i++)
            {
                var node = rank.transform.GetChild(i);

                if (!node.TryGetComponent<SkillTreeNodeButton>(out var button)) continue;

                button.ResetActiveRanks();
                skillNodes[button.SkillSlug] = 0;
            }
        }

        suppressEvents = false;
        availableSkillPoints = CalculateAvailableSkillPoints();
        UpdateVisual();
    }

    public void SaveCurrentState()
    {
        // TODO: luky -> tu mi uloz skillNodes do jsonu (toto sa vola ked odidem zo skilltree panelu)
        Debug.Log($"[{faction}] Save current state: {skillNodes}");
    }
}
