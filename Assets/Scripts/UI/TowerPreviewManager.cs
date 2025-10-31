using UnityEngine;
using TMPro;
using UnityEngine.Assertions;

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
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text rangeText;
    [SerializeField] private TMP_Text fireRateText;
    [SerializeField] private TMP_Text costText;

    private int currentTowerIndex;
    private int currentLevelIndex;

    private GameObject currentTower;

    private void Start()
    {
        currentTowerIndex = Mathf.Clamp(defaultTowerIndex, 0, towerDataCatalog.TowerCount - 1);
        currentLevelIndex = Mathf.Clamp(defaultTowerLevelIndex, 0, towerDataCatalog.TowerLevelsCount - 1);
        ShowTowerAtIndexAndLevel(currentTowerIndex, currentLevelIndex);
    }

    public void ShowTowerAtIndexAndLevel(int index, int level)
    {
        if (currentTower != null) Destroy(currentTower);

        GameObject prefab = towerPrefabs[index];
        currentTower = Instantiate(prefab, towerAnchor.position, Quaternion.identity, towerAnchor);
        SetLayerRecursive(currentTower, LayerMask.NameToLayer("TowerPreview"));

        UpdateTowerStats(currentTowerIndex, currentLevelIndex);
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        foreach (var t in obj.GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = layer;
        }
    }

    private void UpdateTowerStats(int index, int level)
    {
        TowerData data = towerDataCatalog.FromIndex(index);
        Assert.IsNotNull(data);

        if (nameText) nameText.text = data.displayName;
        if (descriptionText) descriptionText.text = data.description;
        if (damageText) damageText.text = $"{data.damage}";
        if (rangeText) rangeText.text = $"{data.range}";
        if (fireRateText) fireRateText.text = $"{data.fireRate}";
        if (costText) costText.text = $"{data.cost}";
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
        Assert.IsTrue(level >= 0 && level <= towerDataCatalog.TowerLevelsCount - 1);
        ShowTowerAtIndexAndLevel(currentTowerIndex, level);
    }
}
