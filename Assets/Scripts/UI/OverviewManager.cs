using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using UnityEditor;

public class OverviewManager : MonoBehaviour
{
    private SerializableLevel level;
    [SerializeField] private TMP_Text operationName;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI factionLevelLabel;
    [SerializeField] private Image factionSymbol;
    [SerializeField] private GameObject operationLayout;
    [SerializeField] private SkillModifierSystem skillModifiers;
    [SerializeField] private GameObject factionDisplay;
    [SerializeField] private GameObject levelTree;
    [SerializeField] private TMP_Text toggleMainContentText;
    [SerializeField] private Image[] difficultyFillImages;

    [SerializeField] private FactionDataCatalog factionDataCatalog;
    [SerializeField] private ModifiersDatabase modifiersDatabase;
    [SerializeField] private OperationLevelCatalog operationLevelCatalog;

    private GameObject currentMainContent;

    private FactionData factionData;
    private SaveContextDontDestroy saveContext;
    private OperationDataDontDestroy operationData;

    private void Awake()
    {

        var go = new GameObject(nameof(OperationDataDontDestroy));
        operationData = go.AddComponent<OperationDataDontDestroy>();
    }

    public void Initialize(SaveContextDontDestroy ctx)
    {
        saveContext = ctx;
        var lastPlayedFaction = saveContext.CurrentSave.LastPlayedFaction;
        var factionData = factionDataCatalog.FromType(lastPlayedFaction);

        Assert.IsNotNull(factionData);
        this.factionData = factionData;

        LoadOperationData();
        var levelName = operationData.LevelFileName;

        string fullPath = Path.Combine(Application.streamingAssetsPath, "Levels", levelName);
        Assert.IsFalse(string.IsNullOrWhiteSpace(levelName), "OverviewManager.levelName is empty. Select a level json file.");
        Assert.IsTrue(File.Exists(fullPath), $"Level JSON not found at: {fullPath}. Make sure it exists under Assets/StreamingAssets/Levels/ and is included in the build.");

        string json = File.ReadAllText(fullPath);
        level = SerializableLevel.FromJson(json);
        Assert.IsNotNull(level, $"Failed to deserialize level JSON: {fullPath}");
        operationName.text = $"Operation {level.operationName}";

        currentMainContent = operationLayout;
        operationLayout.SetActive(true);
        factionDisplay.SetActive(true);
        levelTree.SetActive(false);
        skillModifiers.gameObject.SetActive(false);

        UpdateVisuals();
    }

    public void ToggleMainContent()
    {
        currentMainContent = currentMainContent == operationLayout ? skillModifiers.gameObject : operationLayout;

        operationLayout.SetActive(currentMainContent == operationLayout);
        factionDisplay.SetActive(currentMainContent == operationLayout);

        skillModifiers.gameObject.SetActive(currentMainContent == skillModifiers.gameObject);
        if (currentMainContent == skillModifiers.gameObject) skillModifiers.Initialize();

        levelTree.SetActive(currentMainContent == skillModifiers.gameObject);

        toggleMainContentText.text = currentMainContent == operationLayout ? "Skill Modifiers" : "Operation Layout";
    }

    private void UpdateVisuals()
    {
        DisplayFaction();
        DisplayDifficulty();
    }

    private void DisplayFaction()
    {
        factionLevelLabel.text = $"Level {saveContext.LastFactionSaveState().Item2.level}";
        Assert.IsNotNull(factionSymbol);
        factionSymbol.sprite = factionData.symbol;
    }

    private void DisplayDifficulty()
    {
        float perSegment = 1f / difficultyFillImages.Length;
        float remaining = level.operationDifficulty;

        for (int i = 0; i < difficultyFillImages.Length; i++)
        {
            float fill = Mathf.Clamp01(remaining / perSegment);
            difficultyFillImages[i].fillAmount = fill;
            remaining -= perSegment;
        }
    }

    public void StartOperation()
    {
        LoadOperationData();
        SceneLoader.LoadScene("GameScene");
    }

    private void LoadOperationData()
    {
        var (lastPlayedFaction, lastPlayedFactionSave) = saveContext.LastFactionSaveState();
        var modifiers = modifiersDatabase.GetModifiersBySlugs(lastPlayedFactionSave.SkillNodes(filtered: true));

        // there are no more than 2 levels...... sooooooo clamp to max 2
        var nextOperationIdx = Math.Clamp(lastPlayedFactionSave.highestClearedOperationIndex + 1, 1, 2);
        var levelName = operationLevelCatalog.GetLevelFileNameByOperationIndex(nextOperationIdx);

        operationData.Initialize(lastPlayedFaction, lastPlayedFactionSave.level, modifiers, lastPlayedFactionSave.LastActiveAbilitModifiers, levelName);
    }
}