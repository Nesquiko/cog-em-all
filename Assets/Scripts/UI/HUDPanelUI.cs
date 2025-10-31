using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image gearsFill;
    [SerializeField] private TextMeshProUGUI gearsLabel;
    [SerializeField] private Image gearsPassiveFill;
    [SerializeField] private GameObject towerButtonsPanel;
    [SerializeField] private TowerButton gatlingButton;
    [SerializeField] private TowerButton teslaButton;
    [SerializeField] private TowerButton mortarButton;
    [SerializeField] private TowerButton flamethrowerButton;
    [SerializeField] private GameObject placementInfoPanel;
    [SerializeField] private TextMeshProUGUI placementTowerNameLabel;
    [SerializeField] private TextMeshProUGUI placementTowerCostLabel;
    [SerializeField] private TowerDataCatalog towerDataCatalog;
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;

    private void Start()
    {
        towerButtonsPanel.SetActive(true);
        placementInfoPanel.SetActive(false);
    }

    public void ShowPlacementInfo(TowerTypes towerType)
    {
        TowerData towerData = towerDataCatalog.FromType(towerType);

        placementTowerNameLabel.text = towerData.displayName;
        placementTowerCostLabel.text = $"{towerData.cost} Gears";

        towerButtonsPanel.SetActive(false);
        placementInfoPanel.SetActive(true);
    }

    public void HidePlacementInfo()
    {
        towerButtonsPanel.SetActive(true);
        placementInfoPanel.SetActive(false);
    }

    public void OnCancelPlacement()
    {
        towerPlacementSystem.CancelPlacement();
    }

    public void UpdateGears(int amount)
    {
        gearsLabel.text = amount.ToString();
    }

    public void AdjustTowerButton(TowerTypes type, bool enable)
    {
        switch (type)
        {
            case TowerTypes.Gatling:
                gatlingButton.Enable(enable);
                break;
            case TowerTypes.Tesla:
                teslaButton.Enable(enable);
                break;
            case TowerTypes.Mortar:
                mortarButton.Enable(enable);
                break;
            case TowerTypes.Flamethrower:
                flamethrowerButton.Enable(enable);
                break;
        }
    }

    public void SetPassiveGearsIncomeProgress(float progress)
    {
        gearsPassiveFill.fillAmount = progress;
    }
}
