using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct EnemyInfo
{
    public string name;
    public int count;
}

public class OverviewManager : MonoBehaviour
{
    [Header("Faction Info")]
    [SerializeField] private int factionLevel;

    [Header("Operation Info")]
    [SerializeField, Range(0f, 1f)] private float operationDifficulty;
    [SerializeField] private EnemyInfo[] operationEnemies;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI factionLevelLabel;
    [SerializeField] private Image[] difficultyFillImages;
    [SerializeField] private GameObject enemiesPanel;
    [SerializeField] private GameObject enemyEntryPrefab;

    [SerializeField] private OperationModifiers operationModifiers;

    public void Initialize(FactionSaveState lastPlayedFaction)
    {
        factionLevel = lastPlayedFaction.level;
    }

    private void Start()
    {
        DisplayFaction();
        DisplayDifficulty();
        DisplayEnemies();
    }

    private void DisplayFaction()
    {
        factionLevelLabel.text = $"Level {factionLevel}";
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
        // TODO use the operationModifiers.LoadModifications to load skill tree effects and other buffs/debuffs
        SceneLoader.LoadScene("GameScene");
    }
}
