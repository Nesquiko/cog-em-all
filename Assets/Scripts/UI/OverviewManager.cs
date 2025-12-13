using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class OverviewManager : MonoBehaviour
{
    [Header("Operation Info")]
    [SerializeField, Range(0f, 1f)] private float operationDifficulty;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI factionLevelLabel;
    [SerializeField] private Image factionSymbol;
    [SerializeField] private GameObject operationLayout;
    [SerializeField] private GameObject skillModifiers;
    [SerializeField] private GameObject factionDisplay;
    [SerializeField] private GameObject levelTree;
    [SerializeField] private TMP_Text toggleMainContentText;
    [SerializeField] private Image[] difficultyFillImages;

    [SerializeField] private FactionDataCatalog factionDataCatalog;
    [SerializeField] private ModifiersDatabase modifiersDatabase;

    private GameObject currentMainContent;

    private FactionData factionData;
    private SaveContextDontDestroy saveContext;


    public void Initialize(SaveContextDontDestroy ctx)
    {
        saveContext = ctx;
        var lastPlayedFaction = saveContext.CurrentSave.LastPlayedFaction;

        var factionData = factionDataCatalog.FromType(lastPlayedFaction);

        Assert.IsNotNull(factionData);
        this.factionData = factionData;

        currentMainContent = operationLayout;
        factionDisplay.SetActive(true);
        levelTree.SetActive(false);

        UpdateVisuals();
    }

    public void ToggleMainContent()
    {
        currentMainContent = currentMainContent == operationLayout ? skillModifiers : operationLayout;
        operationLayout.SetActive(currentMainContent == operationLayout);
        factionDisplay.SetActive(currentMainContent == operationLayout);
        skillModifiers.SetActive(currentMainContent == skillModifiers);
        levelTree.SetActive(currentMainContent == skillModifiers);

        toggleMainContentText.text = currentMainContent == operationLayout ? "Skill Modifiers" : "Operation Layout";
    }

    private void UpdateVisuals()
    {
        DisplayFaction();
        DisplayDifficulty();
    }

    private void DisplayFaction()
    {
        factionLevelLabel.text = $"Level {saveContext.LastFactionSaveState().level}";
        Assert.IsNotNull(factionSymbol);
        factionSymbol.sprite = factionData.symbol;
    }

    private void DisplayDifficulty()
    {
        float perSegment = 1f / difficultyFillImages.Length;
        float remaining = operationDifficulty;

        for (int i = 0; i < difficultyFillImages.Length; i++)
        {
            float fill = Mathf.Clamp01(remaining / perSegment);
            difficultyFillImages[i].fillAmount = fill;
            remaining -= perSegment;
        }
    }

    public void StartOperation()
    {
        var go = new GameObject("Operation data");
        var data = go.AddComponent<OperationDataDontDestroy>();

        var modifiers = modifiersDatabase.GetModifiersBySlugs(saveContext.LastFactionSaveState().SkillNodes(filtered: true));
        data.Initialize(saveContext.CurrentSave.LastPlayedFaction, modifiers);

        SceneLoader.LoadScene("GameScene");
    }
}
