using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillModifierSystem : MonoBehaviour
{
    [SerializeField] private TMP_Text modifierPointsText;
    [SerializeField] private SkillModifierButton[] modifierButtons;
    [SerializeField] private ModifiersDatabase modifiersDatabase;

    private int maxModifierPoints;
    private int assignedModifierPoints = 0;
    private int availableModifierPoints;

    public bool CanUseModifierPoint => availableModifierPoints > 0;
    public bool CanRefundModifierPoint => assignedModifierPoints > 0;

    private SaveContextDontDestroy saveContext;
    private FactionSaveState lastPlayedFaction;

    public void Initialize()
    {
        saveContext = SaveContextDontDestroy.GetOrCreateDev();
        (_, lastPlayedFaction) = saveContext.LastFactionSaveState();
        var modifiers = modifiersDatabase.GetModifiersBySlugs(lastPlayedFaction.SkillNodes(filtered: true));

        maxModifierPoints = GetMaxModifierPointsFromLevel(lastPlayedFaction.level);
        availableModifierPoints = maxModifierPoints;

        foreach (var modifierButton in modifierButtons)
        {
            modifierButton.Initialize(modifiers);

            modifierButton.ResetOnActivate();
            modifierButton.OnActivate += OnModifierButtonActivate;
            modifierButton.ResetOnDeactivate();
            modifierButton.OnDeactivate += OnModifierButtonDeactivate;

            bool shouldBeActive = lastPlayedFaction.LastActiveAbilitModifiers.Contains(modifierButton.SkillModifier);

            if (shouldBeActive && !modifierButton.Locked)
            {
                modifierButton.Activate(true);
                assignedModifierPoints++;
                availableModifierPoints--;
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

        lastPlayedFaction.SetLastActiveAbilityModifier(result);
        saveContext.Save();
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
