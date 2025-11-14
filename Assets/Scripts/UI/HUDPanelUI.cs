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
    [SerializeField] private SkillButton wallButton;
    [SerializeField] private SkillButton oilSpillButton;
    [SerializeField] private SkillButton mineButton;
    [SerializeField] private GameObject placementInfoPanel;
    [SerializeField] private TextMeshProUGUI placementObjectNameLabel;
    [SerializeField] private TextMeshProUGUI placementObjectCostLabel;
    [SerializeField] private TowerDataCatalog towerDataCatalog;
    [SerializeField] private SkillDataCatalog skillDataCatalog;
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;
    [SerializeField] private SkillPlacementSystem skillPlacementSystem;

    private void Start()
    {
        towerButtonsPanel.SetActive(true);
        placementInfoPanel.SetActive(false);
    }

    public void ShowPlacementInfo(TowerTypes towerType)
    {
        TowerData<TowerDataBase> towerData = towerDataCatalog.FromType(towerType);

        //TODO kili
        /*placementObjectNameLabel.text = towerData.displayName;
        placementObjectCostLabel.text = $"{towerData.cost} Gears";

        towerButtonsPanel.SetActive(false);
        placementInfoPanel.SetActive(true);*/
    }

    public void ShowPlacementInfo(SkillTypes skillType)
    {
        SkillData skillData = skillDataCatalog.FromType(skillType);

        placementObjectNameLabel.text = skillData.displayName;
        placementObjectCostLabel.text = $"{skillData.cost} Gears";

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
        skillPlacementSystem.CancelPlacement();
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

    public void AdjustSkillButton(SkillTypes type, bool enable)
    {
        switch (type)
        {
            case SkillTypes.Wall:
                wallButton.Enable(enable);
                break;
            case SkillTypes.OilSpill:
                oilSpillButton.Enable(enable);
                break;
            case SkillTypes.Mine:
                mineButton.Enable(enable);
                break;
        }
    }

    public void SetPassiveGearsIncomeProgress(float progress)
    {
        gearsPassiveFill.fillAmount = progress;
    }
}
