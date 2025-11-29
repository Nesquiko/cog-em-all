using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUDPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image gearsFill;
    [SerializeField] private TextMeshProUGUI gearsLabel;
    [SerializeField] private Image gearsPassiveFill;
    [SerializeField] private GameObject towerButtonsPanel;
    [SerializeField] private TowerButton gatlingButton;
    [SerializeField] private TowerButton teslaButton;
    [SerializeField] private TowerButton mortarButton;
    [SerializeField] private TowerButton flamethrowerButton;
    [SerializeField] private SkillButton wallButton;
    [SerializeField] private SkillButton oilSpillButton;
    [SerializeField] private SkillButton mineButton;
    [SerializeField] private GameObject placementInfoPanel;
    [SerializeField] private TextMeshProUGUI placementInfoLabel;
    [SerializeField] private TextMeshProUGUI placementObjectNameLabel;
    [SerializeField] private TextMeshProUGUI placementObjectCostLabel;
    [SerializeField] private TextMeshProUGUI activeModifiersLabel;
    [SerializeField] private GameObject[] modifiers;
    [SerializeField] private TowerDataCatalog towerDataCatalog;
    [SerializeField] private SkillDataCatalog skillDataCatalog;
    [SerializeField] private SkillModifierCatalog skillModifierCatalog;
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;
    [SerializeField] private SkillPlacementSystem skillPlacementSystem;
    [SerializeField] private GameObject minimapImage;
    [SerializeField] private string minimapBackgroundTag;
    [SerializeField] private string maximizedMinimapTag;

    private GameObject minimapBackground;
    private GameObject maximizedMinimap;
    private bool minimapMaximized = false;

    private void Awake()
    {
        minimapBackground = GameObject.FindGameObjectWithTag(minimapBackgroundTag);
        maximizedMinimap = GameObject.FindGameObjectWithTag(maximizedMinimapTag);

        Debug.Log(minimapBackground);
        Debug.Log(maximizedMinimap);
    }

    private void Start()
    {
        towerButtonsPanel.SetActive(true);
        placementInfoPanel.SetActive(false);
    }

    public void ShowPlacementInfo(TowerTypes towerType)
    {
        TowerData<TowerDataBase> towerData = towerDataCatalog.FromType(towerType);
        TowerDataBase level1Data = towerDataCatalog.FromTypeAndLevel(towerType, 1);

        placementInfoLabel.text = "Placing tower:";
        placementObjectNameLabel.text = towerData.DisplayName;
        placementObjectCostLabel.text = $"{level1Data.Cost} Gears";

        activeModifiersLabel.text = "";

        towerButtonsPanel.SetActive(false);
        placementInfoPanel.SetActive(true);
    }

    public void ShowPlacementInfo(SkillTypes skillType)
    {
        SkillData skillData = skillDataCatalog.FromType(skillType);

        placementInfoLabel.text = "Placing skill:";
        placementObjectNameLabel.text = skillData.displayName;
        placementObjectCostLabel.text = $"{skillData.cost} Gears";

        foreach (var mod in modifiers)
            mod.SetActive(false);

        if (skillModifierCatalog.skillModifierIndices.TryGetValue(skillType, out int[] indices))
        {
            HashSet<SkillModifiers> activeModifiers = skillModifierCatalog.ActiveModifiersFromSkillType(skillType);
        
            foreach (int i in indices)
            {
                if (i < 0 || i >= modifiers.Length) continue;
                var go = modifiers[i];
                go.SetActive(true);

                SkillModifiers modifierEnum = skillModifierCatalog.ModifierEnumFromIndex(i);

                bool active = activeModifiers.Contains(modifierEnum);
                Image image = go.GetComponent<Image>();
                Color color = image.color;
                color.a = active ? 0.5f : 0.1f;
                image.color = color;
            }
        }

        activeModifiersLabel.text = "Active modifiers:";

        towerButtonsPanel.SetActive(false);
        placementInfoPanel.SetActive(true);
    }

    public void HidePlacementInfo()
    {
        towerButtonsPanel.SetActive(true);
        placementInfoPanel.SetActive(false);
    }

    public void OnCancelPlacement()
    {
        towerPlacementSystem.CancelPlacement();
        skillPlacementSystem.CancelPlacement();
    }

    public void UpdateGears(int amount)
    {
        gearsLabel.text = amount.ToString();
    }

    public void AdjustTowerButton(TowerTypes type, bool enable)
    {
        switch (type)
        {
            case TowerTypes.Gatling:
                gatlingButton.Enable(enable);
                break;
            case TowerTypes.Tesla:
                teslaButton.Enable(enable);
                break;
            case TowerTypes.Mortar:
                mortarButton.Enable(enable);
                break;
            case TowerTypes.Flamethrower:
                flamethrowerButton.Enable(enable);
                break;
        }
    }

    public void StartSkillCooldown(ISkill skill)
    {
        switch (skill.SkillType())
        {
            case SkillTypes.Wall:
                StartCoroutine(RunSkillCooldown(wallButton, skill.GetCooldown()));
                break;
            case SkillTypes.OilSpill:
                StartCoroutine(RunSkillCooldown(oilSpillButton, skill.GetCooldown()));
                break;
            case SkillTypes.Mine:
                StartCoroutine(RunSkillCooldown(mineButton, skill.GetCooldown()));
                break;
        }
    }

    private IEnumerator RunSkillCooldown(SkillButton button, float duration)
    {
        button.SetCoolingDown(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            button.UpdateCooldownVisual(t / duration);
            yield return null;
        }

        button.SetCoolingDown(false);
        button.UpdateCooldownVisual(1f);
        button.PlayPulse();
    }

    public void AdjustSkillButton(SkillTypes type, bool enable)
    {
        switch (type)
        {
            case SkillTypes.Wall:
                wallButton.Enable(enable);
                break;
            case SkillTypes.OilSpill:
                oilSpillButton.Enable(enable);
                break;
            case SkillTypes.Mine:
                mineButton.Enable(enable);
                break;
        }
    }

    public void SetPassiveGearsIncomeProgress(float progress)
    {
        gearsPassiveFill.fillAmount = progress;
    }

    private void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame) ToggleMaximizeMinimap();
    }

    public void ToggleMaximizeMinimap()
    {
        bool maximized = !minimapMaximized;

        minimapImage.SetActive(!maximized);
        minimapBackground.SetActive(!maximized);
        
        if (maximizedMinimap.transform.childCount > 0)
        {
            GameObject firstChild = maximizedMinimap.transform.GetChild(0).gameObject;
            firstChild.SetActive(maximized);
        }

        minimapMaximized = maximized;
    }
}
