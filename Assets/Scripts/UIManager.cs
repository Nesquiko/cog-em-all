using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuUIManager : MonoBehaviour
{
    [Header("Content Panels")]
    [SerializeField] private GameObject generalSelectionPanel;
    [SerializeField] private GameObject skillTreePanel;
    [SerializeField] private GameObject towerWorkshopPanel;
    [SerializeField] private GameObject levelPanel;

    [Header("Buttons")]
    [SerializeField] private Button generalSelectionButton;
    [SerializeField] private Button skillTreeButton;
    [SerializeField] private Button towerWorkshopButton;
    [SerializeField] private Button levelButton;
    [SerializeField] private Button startButton;

    private GameObject currentPanel;

    private void Start()
    {
        ShowPanel(generalSelectionPanel);
    }

    public void ShowPanel(GameObject panelToShow)
    {
        if (currentPanel == panelToShow) return;

        generalSelectionPanel.SetActive(panelToShow == generalSelectionPanel);
        skillTreePanel.SetActive(panelToShow == skillTreePanel);
        towerWorkshopPanel.SetActive(panelToShow == towerWorkshopPanel);
        levelPanel.SetActive(panelToShow == levelPanel);

        currentPanel = panelToShow;
    }

    public void StartLevel()
    {
        Debug.Log("Starting level!");
    }
}
