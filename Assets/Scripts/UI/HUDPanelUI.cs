using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum FactionSpecificSkill
{
    AirshipAirstrike,
    AirshipFreezeZone,
    AirshipDisableZone,
    MarkEnemy,
    SuddenDeath,
}

public class HUDPanelUI : MonoBehaviour
{
    [Header("Gears")]
    [SerializeField] private Image gearsFill;
    [SerializeField] private TextMeshProUGUI gearsLabel;
    [SerializeField] private Image gearsPassiveFill;

    [Header("Tower Buttons")]
    [SerializeField] private GameObject towerButtonsPanel;
    [SerializeField] private TowerButton gatlingButton;
    [SerializeField] private TowerButton teslaButton;
    [SerializeField] private TowerButton mortarButton;
    [SerializeField] private TowerButton flamethrowerButton;

    [Header("Skill Buttons")]
    [SerializeField] private SkillButton wallButton;
    [SerializeField] private SkillButton oilSpillButton;
    [SerializeField] private SkillButton mineButton;

    [Header("Faction Specific Skills")]
    [SerializeField] private GameObject airshipAirstrikeSkill;
    [SerializeField] private GameObject airshipFreezeZoneSkill;
    [SerializeField] private GameObject airshipDisableZoneSkill;
    [SerializeField] private GameObject markEnemySkill;
    [SerializeField] private GameObject suddenDeathSkill;

    [Header("Placement Info")]
    [SerializeField] private GameObject placementInfoPanel;
    [SerializeField] private TextMeshProUGUI placementInfoLabel;
    [SerializeField] private TextMeshProUGUI placementObjectNameLabel;
    [SerializeField] private TextMeshProUGUI placementObjectCostLabel;
    [SerializeField] private TextMeshProUGUI activeModifiersLabel;
    [SerializeField] private GameObject[] modifiers;

    [Header("References")]
    [SerializeField] private TowerDataCatalog towerDataCatalog;
    [SerializeField] private SkillDataCatalog skillDataCatalog;
    [SerializeField] private SkillModifierCatalog skillModifierCatalog;
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;
    [SerializeField] private SkillPlacementSystem skillPlacementSystem;
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private GameObject minimapImage;
    [SerializeField] private string minimapBackgroundTag;
    [SerializeField] private string maximizedMinimapTag;
    [SerializeField] private GameObject suddenDeathOverlay;
    [SerializeField, Range(1f, 3f)] private float suddenDeathOverlayDuration = 2.5f;

    private GameObject minimapBackground;
    private GameObject maximizedMinimap;
    private bool minimapMaximized = false;

    private SkillButton airshipAirstrikeButton;
    private SkillButton airshipFreezeZoneButton;
    private SkillButton airshipDisableZoneButton;
    private SkillButton markEnemyButton;
    private SkillButton suddenDeathButton;

    private bool airstrikeActive, freezeZoneActive, disableZoneActive, markEnemyActive, suddenDeathActive;

    private HashSet<FactionSpecificSkill> activeFactionSpecificSkills;
    private OperationDataDontDestroy operationData;

    private Dictionary<SkillTypes, int> usagePerAbility = new();

    private void Awake()
    {
        minimapBackground = GameObject.FindGameObjectWithTag(minimapBackgroundTag);
        maximizedMinimap = GameObject.FindGameObjectWithTag(maximizedMinimapTag);

        operationData = OperationDataDontDestroy.GetOrReadDev();
        // TODO kili you can use the usagePerAbility dictionary, it also contains airship things (they are set to const 1)
        usagePerAbility = ModifiersCalculator.UsagePerAbility(operationData.Modifiers);
        activeFactionSpecificSkills = operationData.GetFactionSpecificSkills();

        airshipAirstrikeButton = airshipAirstrikeSkill.GetComponentInChildren<SkillButton>();
        airshipFreezeZoneButton = airshipFreezeZoneSkill.GetComponentInChildren<SkillButton>();
        airshipDisableZoneButton = airshipDisableZoneSkill.GetComponentInChildren<SkillButton>();
        markEnemyButton = markEnemySkill.GetComponentInChildren<SkillButton>();
        suddenDeathButton = suddenDeathSkill.GetComponentInChildren<SkillButton>();

        airstrikeActive = activeFactionSpecificSkills.Contains(FactionSpecificSkill.AirshipAirstrike);
        freezeZoneActive = activeFactionSpecificSkills.Contains(FactionSpecificSkill.AirshipFreezeZone);
        disableZoneActive = activeFactionSpecificSkills.Contains(FactionSpecificSkill.AirshipDisableZone);
        markEnemyActive = activeFactionSpecificSkills.Contains(FactionSpecificSkill.MarkEnemy);
        suddenDeathActive = activeFactionSpecificSkills.Contains(FactionSpecificSkill.SuddenDeath);
    }

    private void Start()
    {
        towerButtonsPanel.SetActive(true);
        placementInfoPanel.SetActive(false);

        ShowAndEnableFactionSpecificSkills();
    }

    private void ShowAndEnableFactionSpecificSkills()
    {
        GameObject[] factionSpecificSkills = {
            airshipAirstrikeSkill,
            airshipFreezeZoneSkill,
            airshipDisableZoneSkill,
            markEnemySkill,
            suddenDeathSkill,
        };
        foreach (var skill in factionSpecificSkills)
            skill.SetActive(false);

        switch (operationData.Faction)
        {
            case Faction.TheBrassArmy:
                airshipAirstrikeSkill.SetActive(true);
                airshipAirstrikeButton.Enable(airstrikeActive, permanently: !airstrikeActive);
                break;
            case Faction.TheValveboundSeraphs:
                airshipFreezeZoneSkill.SetActive(true);
                airshipFreezeZoneButton.Enable(freezeZoneActive, permanently: !freezeZoneActive);
                markEnemySkill.SetActive(true);
                markEnemyButton.Enable(markEnemyActive, permanently: !markEnemyActive);
                break;
            case Faction.OverpressureCollective:
                airshipDisableZoneSkill.SetActive(true);
                airshipDisableZoneButton.Enable(disableZoneActive, permanently: !disableZoneActive);
                suddenDeathSkill.SetActive(true);
                suddenDeathButton.Enable(suddenDeathActive, permanently: !suddenDeathActive);
                break;
        }
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

        activeModifiersLabel.text = "";

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

            activeModifiersLabel.text = "Active modifiers:";
        }

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
            case SkillTypes.AirshipAirstrike:
                StartCoroutine(RunSkillCooldown(airshipAirstrikeButton, skill.GetCooldown()));
                break;
            case SkillTypes.AirshipFreezeZone:
                StartCoroutine(RunSkillCooldown(airshipFreezeZoneButton, skill.GetCooldown()));
                break;
            case SkillTypes.AirshipDisableZone:
                StartCoroutine(RunSkillCooldown(airshipDisableZoneButton, skill.GetCooldown()));
                break;
            case SkillTypes.MarkEnemy:
                StartCoroutine(RunSkillCooldown(markEnemyButton, skill.GetCooldown()));
                break;
            case SkillTypes.SuddenDeath:
                suddenDeathButton.Enable(false, permanently: true);
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
            case SkillTypes.AirshipAirstrike:
                airshipAirstrikeButton.Enable(enable);
                break;
            case SkillTypes.AirshipFreezeZone:
                airshipFreezeZoneButton.Enable(enable);
                break;
            case SkillTypes.AirshipDisableZone:
                airshipDisableZoneButton.Enable(enable);
                break;
            case SkillTypes.MarkEnemy:
                markEnemyButton.Enable(enable);
                break;
            case SkillTypes.SuddenDeath:
                suddenDeathButton.Enable(enable);
                break;
        }
    }

    public void SetPassiveGearsIncomeProgress(float progress)
    {
        gearsPassiveFill.fillAmount = progress;
    }

    private void Update()
    {
        if (pauseManager.Paused) return;
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

    public void ShowSuddenDeathOverlay()
    {
        StartCoroutine(SuddenDeathOverlay());
    }

    private IEnumerator SuddenDeathOverlay()
    {
        CanvasGroup cg = suddenDeathOverlay.GetComponent<CanvasGroup>();
        suddenDeathOverlay.SetActive(true);

        float totalDuration = suddenDeathOverlayDuration;
        float fadeDuration = totalDuration * 0.25f;
        float holdDuration = totalDuration - 2 * fadeDuration;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            yield return null;
        }

        cg.alpha = 1f;
        yield return new WaitForSeconds(holdDuration);

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        cg.alpha = 0f;
        suddenDeathOverlay.SetActive(false);
    }
}
