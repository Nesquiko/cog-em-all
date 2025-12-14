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
    [SerializeField] private string levelName;
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

    private GameObject currentMainContent;

    private FactionData factionData;
    private SaveContextDontDestroy saveContext;
    private OperationDataDontDestroy operationData;

    private void Awake()
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, "Levels", levelName);
        Assert.IsFalse(string.IsNullOrWhiteSpace(levelName), "OverviewManager.levelName is empty. Select a level json file.");
        Assert.IsTrue(File.Exists(fullPath), $"Level JSON not found at: {fullPath}. Make sure it exists under Assets/StreamingAssets/Levels/ and is included in the build.");

        string json = File.ReadAllText(fullPath);
        level = SerializableLevel.FromJson(json);
        Assert.IsNotNull(level, $"Failed to deserialize level JSON: {fullPath}");

        operationName.text = $"Operation {level.operationName}";

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
        var modifiers = modifiersDatabase.GetModifiersBySlugs(saveContext.LastFactionSaveState().Item2.SkillNodes(filtered: true));

        var (lastPlayedFaction, lastPlayedFactionSave) = saveContext.LastFactionSaveState();

        operationData.Initialize(lastPlayedFaction, lastPlayedFactionSave.level, modifiers, lastPlayedFactionSave.LastActiveAbilitModifiers, levelName);

        SceneLoader.LoadScene("GameScene");
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(OverviewManager))]
public class OverviewManagerEditor : Editor
{
    private string[] levelOptions = new string[0];

    private SerializedProperty levelNameProp;

    private void OnEnable()
    {
        levelNameProp = serializedObject.FindProperty("levelName");
        RefreshOptions();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw everything except levelName (we replace it with a popup)
        DrawPropertiesExcluding(serializedObject, "levelName");

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Level JSON", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh Level List"))
        {
            RefreshOptions();
        }

        DrawLevelNamePopup();

        serializedObject.ApplyModifiedProperties();
    }

    private void RefreshOptions()
    {
        string dir = Path.Combine(Application.streamingAssetsPath, "Levels");

        if (!Directory.Exists(dir))
        {
            levelOptions = new string[0];
            return;
        }

        levelOptions = Directory
            .GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName) // store just "file.json"
            .OrderBy(x => x)
            .ToArray();
    }

    private void DrawLevelNamePopup()
    {
        if (levelOptions == null || levelOptions.Length == 0)
        {
            EditorGUILayout.HelpBox(
                $"No .json files found in:\n{Path.Combine(Application.streamingAssetsPath, "Levels")}\n\n" +
                "Create Assets/StreamingAssets/Levels and put your level json files there.",
                MessageType.Warning
            );

            // fallback: still allow manual entry
            EditorGUILayout.PropertyField(levelNameProp);
            return;
        }

        string current = levelNameProp.stringValue ?? "";
        int currentIndex = Array.IndexOf(levelOptions, current);
        if (currentIndex < 0) currentIndex = 0;

        int newIndex = EditorGUILayout.Popup("Level File", currentIndex, levelOptions);
        levelNameProp.stringValue = levelOptions[newIndex];
    }
}
#endif
