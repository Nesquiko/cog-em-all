using UnityEngine;

public class UIMenuManager : MonoBehaviour
{
    [Header("Content Panels")]
    [SerializeField] private GameObject factionsPanel;
    [SerializeField] private GameObject overviewPanel;
    [SerializeField] private GameObject towersPanel;

    private GameObject currentPanel;

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

        currentPanel = panelToShow;
    }
}
