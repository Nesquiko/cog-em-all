using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image gearsFill;
    [SerializeField] private TextMeshProUGUI gearsLabel;

    public void UpdateGears(int amount)
    {
        gearsLabel.text = amount.ToString();
    }
}
