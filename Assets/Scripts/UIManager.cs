using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject generalSelectionPanel;

    public void StartGame(int generalIndex)
    {
        PlayerPrefs.SetInt("GeneralSelected", generalIndex);

        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowHome()
    {
        homePanel.SetActive(true);
        generalSelectionPanel.SetActive(false);
    }

    public void ShowGeneralSelection()
    {
        generalSelectionPanel.SetActive(true);
        homePanel.SetActive(false);
    }
}
