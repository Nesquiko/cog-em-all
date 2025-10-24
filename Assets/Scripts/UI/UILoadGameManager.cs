using UnityEngine;

public class UILoadGameManager : MonoBehaviour
{
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private GameObject menuPanel;

    public void HandleLoadGameClick(int gameID)
    {
        loadGamePanel.SetActive(false);
        menuPanel.SetActive(true);

        // TODO: LoadGameData(gameID);
    }
}
