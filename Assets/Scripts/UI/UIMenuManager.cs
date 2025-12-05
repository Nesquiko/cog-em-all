using UnityEngine;

public class UIMenuManager : MonoBehaviour
{
    [Header("Content Panels")]
    [SerializeField] private GameObject factionsPanel;
    [SerializeField] private GameObject overviewPanel;
    [SerializeField] private GameObject towersPanel;
    [SerializeField] private GameObject skillTreePanel;

    private GameObject currentPanel;
    private Faction currentFaction = Faction.TheBrassArmy;
    private FactionSkillTreeUI factionSkillTreeUI;

    private void Awake()
    {
        factionSkillTreeUI = skillTreePanel.GetComponent<FactionSkillTreeUI>();
    }

    private void Start()
    {
        ShowPanel(overviewPanel);
    }

    public void HandleFactionCardClick(int index)
    {
        currentFaction = index switch
        {
            0 => Faction.TheBrassArmy,
            1 => Faction.TheValveboundSeraphs,
            2 => Faction.OverpressureCollective,
            _ => Faction.TheBrassArmy,
        };
        ShowPanel(skillTreePanel);
    }

    public void ShowPanel(GameObject panelToShow)
    {
        if (currentPanel == skillTreePanel && panelToShow != skillTreePanel)
            factionSkillTreeUI.SaveSkillTrees();

        if (currentPanel == panelToShow) return;

        factionsPanel.SetActive(panelToShow == factionsPanel);
        overviewPanel.SetActive(panelToShow == overviewPanel);
        towersPanel.SetActive(panelToShow == towersPanel);
        skillTreePanel.SetActive(panelToShow == skillTreePanel);

        if (panelToShow == skillTreePanel)
        {
            factionSkillTreeUI.Initialize(currentFaction);
        }

        currentPanel = panelToShow;
    }
}
