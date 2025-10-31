using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Content Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private GameObject menuPanel;

    private GameObject currentPanel;

    private void Start()
    {
        ShowPanel(homePanel);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if (currentPanel == panelToShow) return;

        homePanel.SetActive(panelToShow == homePanel);
        loadGamePanel.SetActive(panelToShow == loadGamePanel);
        menuPanel.SetActive(panelToShow == menuPanel);

        currentPanel = panelToShow;
    }

    public void HandleNewGameClick()
    {
        ShowPanel(menuPanel);
    }

    public void HandleLoadGameClick()
    {
        ShowPanel(loadGamePanel);
    }

    public void HandleBackToHomeClick()
    {
        ShowPanel(homePanel);
    }

    public void HandleQuitClick()
    {
        Application.Quit();
    }
}
