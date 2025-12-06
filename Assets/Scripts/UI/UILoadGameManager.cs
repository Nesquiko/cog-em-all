using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILoadGameManager : MonoBehaviour
{
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private GameObject homePanel;
    [SerializeField] private VerticalLayoutGroup savedGamesButtonsParent;
    [SerializeField] private SaveContextDontDestroy saveContext;
    [SerializeField] private SavedGameButton savedGameButtonPrefab;

    private void Start()
    {
        List<SaveData> saves = SaveSystem.LoadAllSaves();
        foreach (var saveData in saves)
        {
            var savedGameButton = Instantiate(savedGameButtonPrefab, savedGamesButtonsParent.transform);
            savedGameButton.SetLabel($"Saved game {SaveSystem.SaveFileNumber(saveData.name)}");
            savedGameButton.SetButtonOnClick(() => HandleLoadSelectedGameClick(saveData));
        }
    }

    public void HandleLoadSelectedGameClick(SaveData save)
    {
        Debug.Log($"Loading save '{save}'");
        loadGamePanel.SetActive(false);
        saveContext.SetCurrentSave(save);
        SceneLoader.LoadScene("MenuScene");
    }

    public void HandleBackToHomeClick()
    {
        loadGamePanel.SetActive(false);
        homePanel.SetActive(true);
    }
}
