using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FactionCard : MonoBehaviour
{
    [SerializeField] private Faction faction;
    [SerializeField] private TMP_Text factionName;
    [SerializeField] private Image image;
    [SerializeField] private Image symbolImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Button button;
    [SerializeField] private UITexts uiTexts;

    public event Action<Faction> OnSelect;

    private void Awake()
    {
        factionName.text = uiTexts.GetFactionName(faction);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnSelect?.Invoke(faction));
    }

    public void SetLevel(int level)
    {
        levelText.text = $"Level {level}";
    }
}
