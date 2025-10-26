using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerPreviewManager : MonoBehaviour
{
    [Header("Preview Setup")]
    [SerializeField] private Camera previewCamera;
    [SerializeField] private Transform towerAnchor;

    [Header("Tower Prefabs")]
    [SerializeField] private GameObject[] towerPrefabs;
    [SerializeField] private int defaultTowerIndex = 0;

    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text rangeText;
    [SerializeField] private TMP_Text fireRateText;
    [SerializeField] private TMP_Text costText;

    private int currentIndex;
    private GameObject currentTower;

    private void Start()
    {
        currentIndex = Mathf.Clamp(defaultTowerIndex, 0, towerPrefabs.Length - 1);
        ShowTowerAtIndex(currentIndex);
    }

    public void ShowTowerAtIndex(int index)
    {
        if (currentTower != null) Destroy(currentTower);

        GameObject prefab = towerPrefabs[index];
        currentTower = Instantiate(prefab, towerAnchor.position, Quaternion.identity, towerAnchor);
        SetLayerRecursive(currentTower, LayerMask.NameToLayer("TowerPreview"));

        UpdateTowerStats(currentIndex);
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        foreach (var t in obj.GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = layer;
        }
    }

    private void UpdateTowerStats(int index)
    {
        TowerData data = TowerCatalog.FromIndex(index);
        if (data == null) return;

        if (nameText) nameText.text = data.displayName;
        if (descriptionText) descriptionText.text = data.description;
        if (damageText) damageText.text = $"{data.damage}";
        if (rangeText) rangeText.text = $"{data.range}";
        if (fireRateText) fireRateText.text = $"{data.fireRate}";
        if (costText) costText.text = $"{data.cost}";
    }

    public void NextTower()
    {
        currentIndex++;
        if (currentIndex > towerPrefabs.Length - 1) currentIndex = 0;
        ShowTowerAtIndex(currentIndex);
    }

    public void PreviousTower()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = towerPrefabs.Length - 1;
        ShowTowerAtIndex(currentIndex);
    }
}
