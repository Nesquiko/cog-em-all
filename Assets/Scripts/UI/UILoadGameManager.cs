using UnityEngine;

public class UILoadGameManager : MonoBehaviour
{
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private GameObject homePanel;

    public void HandleLoadSelectedGameClick(int gameID)
    {
        loadGamePanel.SetActive(false);
        SceneLoader.LoadScene("MenuScene");

        // TODO: LoadGameData(gameID);
    }

    public void HandleBackToHomeClick()
    {
        loadGamePanel.SetActive(false);
        homePanel.SetActive(true);
    }
}
