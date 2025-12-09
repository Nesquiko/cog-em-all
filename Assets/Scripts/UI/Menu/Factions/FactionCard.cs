using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class FactionCard : MonoBehaviour
{
    [SerializeField] private Faction faction;
    [SerializeField] private TMP_Text factionName;
    [SerializeField] private Image image;
    [SerializeField] private Image symbolImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Button button;
    [SerializeField] private FactionData factionData;

    public event Action<Faction> OnSelect;

    private void Awake()
    {
        Assert.AreEqual(factionData.faction, faction);
        factionName.text = factionData.displayName;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnSelect?.Invoke(faction));

        image.sprite = factionData.mainImage;
        symbolImage.sprite = factionData.symbol;
    }

    public void SetLevel(int level)
    {
        levelText.text = $"Level {level}";
    }
}
