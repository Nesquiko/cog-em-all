using TMPro;
using UnityEngine;

public class HUDPanelUI : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField] private TextMeshProUGUI gearsLabel;

    private int gears = 1000;

    private void Start()
    {
        UpdateResources();
    }

    public void AddGears(int amount)
    {
        gears += amount;
        UpdateResources();
    }

    public void SpendGears(int amount)
    {
        gears -= amount;
        UpdateResources();
    }

    private void UpdateResources()
    {
        gearsLabel.text = $"Gearss: {gears}";
    }
}
