using UnityEngine;

public class UIHomeManager : MonoBehaviour
{
    [Header("Content Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject loadGamePanel;

    private void Start()
    {
        ShowHomePanel();
    }

    private void ShowHomePanel()
    {
        loadGamePanel.SetActive(false);
        homePanel.SetActive(true);
    }

    public void HandleNewGameClick()
    {
        SceneLoader.LoadScene("MenuScene");
    }

    public void HandleLoadGameClick()
    {
        homePanel.SetActive(false);
        loadGamePanel.SetActive(true);
    }

    public void HandleQuitClick()
    {
        Application.Quit();
    }
}
