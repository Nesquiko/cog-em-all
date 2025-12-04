using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTree : MonoBehaviour
{
    [SerializeField] private TMP_Text skillPointsText;
    [SerializeField] private Button resetSkillPointsButton;

    private int availableSkillPoints = 0;

    private void Awake()
    {
        skillPointsText.text = availableSkillPoints.ToString();
    }
}
