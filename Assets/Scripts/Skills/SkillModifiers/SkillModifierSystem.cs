using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillModifierSystem : MonoBehaviour
{
    [SerializeField] private TMP_Text modifierPointsText;
    [SerializeField] private SkillModifierButton[] modifierButtons;

    private int factionLevel;
    private int maxModifierPoints;
    private int assignedModifierPoints = 0;
    private int availableModifierPoints;
    private OperationDataDontDestroy operationData;

    public bool CanUseModifierPoint => availableModifierPoints > 0;
    public bool CanRefundModifierPoint => assignedModifierPoints > 0;

    private SaveContextDontDestroy saveContext;

    private void Awake()
    {
        saveContext = SaveContextDontDestroy.GetOrCreateDev();
        factionLevel = saveContext.LastFactionSaveState().Item2.level;
        operationData = OperationDataDontDestroy.GetOrReadDev();

        maxModifierPoints = GetMaxModifierPointsFromLevel(factionLevel);
        availableModifierPoints = maxModifierPoints;

        foreach (var modifierButton in modifierButtons)
        {
            modifierButton.Initialize();

            modifierButton.OnActivate += OnModifierButtonActivate;
            modifierButton.OnDeactivate += OnModifierButtonDeactivate;

            bool shouldBeActive = operationData.AbilityModifiers.Contains(modifierButton.SkillModifier);

            if (shouldBeActive && !modifierButton.Locked)
            {
                modifierButton.Activate(true);
                assignedModifierPoints++;
                availableModifierPoints--;
            }
            else if (modifierButton.Locked)
            {
                modifierButton.Activate(false);
            }
            else
            {
                modifierButton.Activate(false);
            }
        }

        UpdatePointsText();
    }

    private int GetMaxModifierPointsFromLevel(int factionLevel)
    {
        return Mathf.FloorToInt(factionLevel / 5);
    }

    private void UpdatePointsText()
    {
        modifierPointsText.text = $"Available skill modifier points:  {availableModifierPoints}";
    }

    private void OnModifierButtonActivate(SkillModifiers modifier)
    {
        availableModifierPoints--;
        assignedModifierPoints++;

        SaveActivatedModifiers();
        UpdatePointsText();
    }

    private void OnModifierButtonDeactivate(SkillModifiers modifier)
    {
        availableModifierPoints++;
        assignedModifierPoints--;

        SaveActivatedModifiers();
        UpdatePointsText();
    }

    public void SaveActivatedModifiers()
    {
        HashSet<SkillModifiers> result = new();
        foreach (var modifierButton in modifierButtons)
        {
            if (modifierButton.Activated) result.Add(modifierButton.SkillModifier);
        }

        operationData.SetAbilityModifiers(result);
    }

    private void OnDestroy()
    {
        foreach (var modifierButton in modifierButtons)
        {
            modifierButton.OnActivate -= OnModifierButtonActivate;
            modifierButton.OnDeactivate -= OnModifierButtonDeactivate;
        }
    }
}
