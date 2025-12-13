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
    private HashSet<SkillModifiers> activeSkillModifiers = new();

    public bool CanUseModifierPoint => availableModifierPoints > 0;
    public bool CanRefundModifierPoint => assignedModifierPoints > 0;

    private void Awake()
    {
        factionLevel = 15;  // TODO: luky -> tu chcem faction level
        activeSkillModifiers = new()  // TODO: luky -> tu mi musia dojst modifiery ktore mam aktivne na abilitach
        {
            SkillModifiers.SteelReinforcement,
            SkillModifiers.WideDestruction,
            SkillModifiers.GooeyGoo,
        };

        maxModifierPoints = GetMaxModifierPointsFromLevel(factionLevel);
        availableModifierPoints = maxModifierPoints;

        foreach (var modifierButton in modifierButtons)
        {
            modifierButton.Initialize();

            modifierButton.OnActivate += OnModifierButtonActivate;
            modifierButton.OnDeactivate += OnModifierButtonDeactivate;

            bool shouldBeActive = activeSkillModifiers.Contains(modifierButton.SkillModifier);

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
        Debug.Log($"Activated {modifier}");
        availableModifierPoints--;
        assignedModifierPoints++;

        UpdatePointsText();
    }

    private void OnModifierButtonDeactivate(SkillModifiers modifier)
    {
        Debug.Log($"Deactivated {modifier}");
        availableModifierPoints++;
        assignedModifierPoints--;

        UpdatePointsText();
    }

    public void SaveActivatedModifiers()
    {
        HashSet<SkillModifiers> result = new();
        foreach (var modifierButton in modifierButtons)
        {
            if (modifierButton.Activated) result.Add(modifierButton.SkillModifier);
        }

        // TODO: luky -> tu mas funkciu na aktivne vyklikane modifiery, uloz ich
        Debug.Log($"Modifiers to save: {result}");
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
