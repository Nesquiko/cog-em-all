using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image gearsFill;
    [SerializeField] private TextMeshProUGUI gearsLabel;

    [Header("Settings")]
    [SerializeField, Tooltip("Time in seconds for one full fill cycle.")]
    private float fillDuration = 5f;

    [Header("Passive Earning")]
    [SerializeField, Tooltip("How much currency to add each cycle.")]
    private int earnPerCycle = 10;

    private int gears = 1000;

    private float timer;

    private void Start()
    {
        UpdateResources();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float fraction = timer / fillDuration;
        gearsFill.fillAmount = fraction;

        if (timer >= fillDuration)
        {
            timer -= fillDuration;
            gearsFill.fillAmount = 0f;

            AddGears(earnPerCycle);
        }
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
        gearsLabel.text = gears.ToString();
    }
}
