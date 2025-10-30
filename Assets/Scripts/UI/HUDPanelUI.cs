using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image gearsFill;
    [SerializeField] private TextMeshProUGUI gearsLabel;
    [SerializeField] private TowerButton gatlingButton;
    [SerializeField] private TowerButton teslaButton;
    [SerializeField] private TowerButton mortarButton;
    [SerializeField] private TowerButton flamethrowerButton;

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
}
