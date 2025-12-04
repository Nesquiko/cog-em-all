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

    private void Start()
    {
        ShowPanel(overviewPanel);
    }

    public void ShowPanel(GameObject panelToShow)
    {
        if (currentPanel == panelToShow) return;

        factionsPanel.SetActive(panelToShow == factionsPanel);
        overviewPanel.SetActive(panelToShow == overviewPanel);
        towersPanel.SetActive(panelToShow == towersPanel);
        skillTreePanel.SetActive(panelToShow == skillTreePanel);

        if (panelToShow == skillTreePanel)
        {
            skillTreePanel.GetComponent<FactionSkillTreeUI>().Initialize(currentFaction);
        }

        currentPanel = panelToShow;
    }
}
