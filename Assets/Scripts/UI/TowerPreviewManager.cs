using UnityEngine;
using TMPro;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

public class TowerPreviewManager : MonoBehaviour
{
    [Header("Preview Setup")]
    [SerializeField] private Camera previewCamera;
    [SerializeField] private Transform towerAnchor;

    [Header("Tower Prefabs")]
    [SerializeField] private GameObject[] towerPrefabs;
    [SerializeField] private int defaultTowerIndex = 0;
    [SerializeField] private int defaultTowerLevelIndex = 0;
    [SerializeField] private TowerDataCatalog towerDataCatalog;

    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text sellPriceText;

    [Header("Stats Display")]
    [SerializeField] private Transform statsContainer;
    [SerializeField] private GameObject statsEntryPrefab;

    private int currentTowerIndex;
    private int currentLevelIndex;

    private GameObject currentTower;

    private readonly List<GameObject> activeStatRows = new();

    private void Start()
    {
        currentTowerIndex = Mathf.Clamp(defaultTowerIndex, 0, towerDataCatalog.TowersCount - 1);
        currentLevelIndex = Mathf.Clamp(defaultTowerLevelIndex, 1, towerDataCatalog.TowerLevelsCount);
        ShowTowerAtIndexAndLevel(currentTowerIndex, currentLevelIndex);
    }

    public void ShowTowerAtIndexAndLevel(int index, int level)
    {
        if (currentTower != null) Destroy(currentTower);

        GameObject prefab = towerPrefabs[index];
        currentTower = Instantiate(prefab, towerAnchor.position, Quaternion.identity, towerAnchor);
        SetLayerRecursive(currentTower, LayerMask.NameToLayer("TowerPreview"));

        UpdateTowerStats(index, level);
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        foreach (var t in obj.GetComponentsInChildren<Transform>())
            t.gameObject.layer = layer;
    }

    private void UpdateTowerStats(int index, int level)
    {
        TowerData<TowerDataBase> towerData = towerDataCatalog.FromIndex(index);
        TowerDataBase towerLevelData = towerDataCatalog.FromIndexAndLevel(index, level);
        if (towerLevelData == null) return;

        towerLevelData.RebuildDisplayStats();

        nameText.text = towerData.DisplayName;
        descriptionText.text = towerData.Description;
        costText.text = $"{towerLevelData.Cost}";
        sellPriceText.text = $"{towerLevelData.SellPrice}";

        foreach (var row in activeStatRows) Destroy(row);
        activeStatRows.Clear();

        var stats = towerLevelData.DisplayStats;
        if (stats == null) return;

        foreach (var stat in stats)
        {
            GameObject entry = Instantiate(statsEntryPrefab, statsContainer);
            if (entry.TryGetComponent<RectTransform>(out var entryRect))
                entryRect.sizeDelta = new(550f, 60f);
            TMP_Text[] texts = entry.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = stat.label;
                texts[1].text = stat.value;

                foreach (var text in texts)
                {
                    if (!text.TryGetComponent<RectTransform>(out var tRect)) continue;
                    var sizeDelta = tRect.sizeDelta;
                    sizeDelta.y = 60f;
                    tRect.sizeDelta = sizeDelta;
                }
            }

            activeStatRows.Add(entry);
        }
    }

    private string FormatStatValue(object value)
    {
        return value switch
        {
            float f => $"{f:0.###}",
            int i => i.ToString(),
            bool b => b ? "Yes" : "No",
            _ => value?.ToString() ?? "-",
        };
    }

    public void NextTower()
    {
        currentTowerIndex++;
        if (currentTowerIndex > towerPrefabs.Length - 1) currentTowerIndex = 0;
        ShowTowerAtIndexAndLevel(currentTowerIndex, currentLevelIndex);
    }

    public void PreviousTower()
    {
        currentTowerIndex--;
        if (currentTowerIndex < 0) currentTowerIndex = towerPrefabs.Length - 1;
        ShowTowerAtIndexAndLevel(currentTowerIndex, currentLevelIndex);
    }

    public void ShowTowerLevel(int level)
    {
        Assert.IsTrue(level >= 1 && level <= towerDataCatalog.TowerLevelsCount);
        ShowTowerAtIndexAndLevel(currentTowerIndex, level);
    }
}
