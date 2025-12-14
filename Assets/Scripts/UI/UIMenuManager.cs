using System;
using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuManager : MonoBehaviour
{
    public enum Panel
    {
        // skips 0, because on init the currentPanel will be 0, which means "dont show anything"
        Overview = 1,
        Factions = 2,
        Towers = 3,
        SkillTree = 4
    }

    private SaveContextDontDestroy saveContext;

    [Header("Content Panels")]
    [SerializeField] private FactionsPanel factionsPanel;
    [SerializeField] private OverviewManager overviewPanel;
    [SerializeField] private GameObject towersPanel;
    [SerializeField] private FactionSkillTreeUI skillTreeUI;

    private Panel currentPanel;

    [Header("Tab Indicator")]
    [SerializeField] private RectTransform indicator;
    [SerializeField] private RectTransform factionsTabRoot;
    [SerializeField] private RectTransform overviewTabRoot;
    [SerializeField] private RectTransform towersTabRoot;
    [SerializeField] private float indicatorMoveDuration = 0.25f;
    [SerializeField] private AnimationCurve indicatorEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine indicatorMoveRoutine;

    [Header("Experience")]
    [SerializeField] private Image experienceBar;
    [SerializeField] private TMP_Text expGained;
    [SerializeField] private TMP_Text expNeeded;
    [SerializeField] private ExperienceSystem experienceSystem;
    [SerializeField] private float expFillDuration = 0.4f;

    private Coroutine xpFillRoutine;

    private void Awake()
    {
        saveContext = SaveContextDontDestroy.GetOrCreateDev();
        experienceSystem.OnXPChanged += HandleXPChanged;
        experienceSystem.OnLevelUp += HandleLevelUp;

        experienceSystem.InitializeFromTotalXP(saveContext.LastFactionSaveState().Item2.totalXP);
    }

    private void OnEnable()
    {
        experienceSystem.InitializeFromTotalXP(saveContext.LastFactionSaveState().Item2.totalXP);
    }

    private void Start()
    {
        ShowOverview();
    }

    public void ShowFactions()
    {
        factionsPanel.Initialize(saveContext.CurrentSave, ShowSkillTree);
        ShowPanel(Panel.Factions);
    }

    public void ShowSkillTree(Faction faction)
    {
        skillTreeUI.Initialize(faction, saveContext.CurrentSave);
        saveContext.CurrentSave.lastPlayedFaction = (SaveData.PlayedFaction)faction;
        SaveSystem.UpdateSave(saveContext.CurrentSave);
        ShowPanel(Panel.SkillTree);
    }

    public void ShowOverview()
    {
        if (saveContext.CurrentSave.lastPlayedFaction == SaveData.PlayedFaction.None)
        {
            ShowFactions();
            return;
        }

        overviewPanel.Initialize(saveContext);
        ShowPanel(Panel.Overview);
    }

    public void ShowTowers() => ShowPanel(Panel.Towers);

    private void ShowPanel(Panel toShow)
    {
        if (currentPanel == Panel.SkillTree && toShow != Panel.SkillTree)
            skillTreeUI.SaveSkillTrees();

        if (currentPanel == toShow) return;

        factionsPanel.gameObject.SetActive(toShow == Panel.Factions);
        overviewPanel.gameObject.SetActive(toShow == Panel.Overview);
        towersPanel.SetActive(toShow == Panel.Towers);
        skillTreeUI.gameObject.SetActive(toShow == Panel.SkillTree);

        currentPanel = toShow;

        MoveIndicatorToPanel(toShow);
    }

    private void MoveIndicatorToPanel(Panel targetPanel)
    {
        RectTransform targetRoot = GetTabRoot(targetPanel);

        if (indicatorMoveRoutine != null)
            StopCoroutine(indicatorMoveRoutine);

        indicatorMoveRoutine = StartCoroutine(MoveIndicatorCoroutine(targetRoot));
    }

    private IEnumerator MoveIndicatorCoroutine(RectTransform target)
    {
        Vector3 startPosition = indicator.position;
        Vector3 endPosition = target.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / indicatorMoveDuration;
            float easedT = indicatorEase.Evaluate(t);
            indicator.position = Vector3.Lerp(startPosition, endPosition, easedT);
            yield return null;
        }

        indicator.position = endPosition;
        indicatorMoveRoutine = null;
    }

    private RectTransform GetTabRoot(Panel panel)
    {
        return panel switch
        {
            Panel.Overview => overviewTabRoot,
            Panel.Factions => factionsTabRoot,
            Panel.Towers => towersTabRoot,
            Panel.SkillTree => factionsTabRoot,
            _ => null,
        };
    }

    private void HandleXPChanged(float currentXP, float xpToNextLevel)
    {
        float progress = Mathf.Clamp01(currentXP / xpToNextLevel);

        if (xpFillRoutine != null) StopCoroutine(xpFillRoutine);

        xpFillRoutine = StartCoroutine(AnimateXPBar(progress));

        expGained.text = $"{currentXP:F0}";
        expNeeded.text = $"{xpToNextLevel:F0}";

        SaveFactionExperience();
    }

    private void SaveFactionExperience()
    {
        FactionSaveState factionSave = saveContext.LastFactionSaveState().Item2;

        factionSave.totalXP = experienceSystem.ComputeTotalXP();
        factionSave.level = experienceSystem.Level;
        SaveData save = saveContext.CurrentSave;
        SaveSystem.UpdateSave(save);
    }

    private IEnumerator AnimateXPBar(float targetFill)
    {
        float start = experienceBar.fillAmount;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / expFillDuration;
            experienceBar.fillAmount = Mathf.Lerp(start, targetFill, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        experienceBar.fillAmount = targetFill;
    }

    private void HandleLevelUp(int newLevel)
    {
        Debug.Log($"Leveled up, new level: {newLevel}");
    }

    private void OnDestroy()
    {
        experienceSystem.OnXPChanged -= HandleXPChanged;
        experienceSystem.OnLevelUp -= HandleLevelUp;
    }
}
