using System;
using UnityEngine;

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

    private void Awake()
    {
        saveContext = SaveContextDontDestroy.GetOrCreateDev();
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
    }
}
