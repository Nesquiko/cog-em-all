using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FactionCard : MonoBehaviour
{
    [SerializeField] private FactionData factionData;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image mainImage;
    [SerializeField] private Image symbolImage;
    [SerializeField] private TMP_Text levelText;

    private int level;

    private void Awake()
    {
        level = 5;  // TODO: luky -> get faction level from somewhere

        titleText.text = factionData.displayName;
        mainImage.sprite = factionData.mainImage;
        symbolImage.sprite = factionData.symbol;
        levelText.text = $"Level {level}";
    }
}
