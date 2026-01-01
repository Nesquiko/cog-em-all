using UnityEngine;
using UnityEngine.UI;

public class UIHomeManager : MonoBehaviour
{

    [SerializeField] private SaveContextDontDestroy saveContext;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private CursorPointer loadGameButtonCursorPointer;

    [Header("Content Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject loadGamePanel;

    private void Start()
    {
        var saveFilesCount = SaveSystem.CountSaveFiles();
        Debug.Log($"there are {saveFilesCount} save files");

        var areThereSaves = saveFilesCount > 0;
        loadGameButton.interactable = areThereSaves;

        ShowHomePanel();
    }

    private void ShowHomePanel()
    {
        loadGamePanel.SetActive(false);
        homePanel.SetActive(true);
    }

    public void HandleNewGameClick()
    {
        var newSaveData = SaveSystem.CreateNewSave();
        saveContext.SetCurrentSave(newSaveData);
        //SceneLoader.LoadScene("MenuScene");
        SceneTransition.GetOrCreate().TransitionToScene("MenuScene");
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
