using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[Serializable]
public struct EnemyInfo
{
    public string name;
    public int count;
}

public class OverviewManager : MonoBehaviour
{
    [Header("Operation Info")]
    [SerializeField, Range(0f, 1f)] private float operationDifficulty;
    [SerializeField] private EnemyInfo[] operationEnemies;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI factionLevelLabel;
    [SerializeField] private Image factionSymbol;
    [SerializeField] private Image[] difficultyFillImages;
    [SerializeField] private GameObject enemiesPanel;
    [SerializeField] private GameObject enemyEntryPrefab;

    [SerializeField] private FactionDataCatalog factionDataCatalog;
    [SerializeField] private ModifiersDatabase modifiersDatabase;

    private FactionData factionData;
    private SaveContextDontDestroy saveContext;


    public void Initialize(SaveContextDontDestroy saveContext)
    {
        this.saveContext = saveContext;
        var lastPlayedFaction = saveContext.CurrentSave.LastPlayedFaction;

        var factionData = factionDataCatalog.FromType(lastPlayedFaction);

        Assert.IsNotNull(factionData);
        this.factionData = factionData;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        DisplayFaction();
        DisplayDifficulty();
        DisplayEnemies();
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

    private void DisplayEnemies()
    {
        foreach (Transform child in enemiesPanel.transform)
            Destroy(child.gameObject);

        foreach (EnemyInfo e in operationEnemies)
        {
            GameObject entry = Instantiate(enemyEntryPrefab, enemiesPanel.transform);
            TextMeshProUGUI[] labels = entry.GetComponentsInChildren<TextMeshProUGUI>();

            if (labels.Length == 2)
            {
                labels[0].text = e.name;
                labels[1].text = e.count.ToString();
            }
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
