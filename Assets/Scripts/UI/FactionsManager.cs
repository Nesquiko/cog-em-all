using UnityEngine;

public class FactionsManager : MonoBehaviour
{
    [SerializeField] private GameObject factionSelectionPanel;
    [SerializeField] private GameObject skillTreePanel;

    private GameObject currentPanel;

    private void Start()
    {
        ShowPanel(factionSelectionPanel);
    }

    public void ShowPanel(GameObject panelToShow)
    {
        if (currentPanel == panelToShow) return;

        factionSelectionPanel.SetActive(panelToShow == factionSelectionPanel);
        skillTreePanel.SetActive(panelToShow == skillTreePanel);

        currentPanel = panelToShow;
    }
}
