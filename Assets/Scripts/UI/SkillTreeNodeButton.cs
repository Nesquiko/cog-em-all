using UnityEngine;
using UnityEngine.UI;

public enum SkillNodeType
{
    Minor,
    Major,
    FactionSpecific,
}

public class SkillTreeNodeButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Image borderImage;
    [SerializeField] private bool unlocked = true;
    [SerializeField] private SkillNodeType type;
    [SerializeField] private Faction faction;

    public bool Unlocked
    {
        get => unlocked;
        set
        {
            unlocked = value;
            UpdateVisual();
        }
    }

    private void Awake()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        lockedOverlay.SetActive(!unlocked);
        button.interactable = unlocked;

        if (type == SkillNodeType.FactionSpecific)
            borderImage.color = FactionAccent(faction);
    }

    private static Color FactionAccent(Faction f)
    {
        return f switch
        {
            Faction.TheBrassArmy => Color.red,
            Faction.TheValveboundSeraphs => Color.green,
            Faction.OverpressureCollective => Color.yellow,
            _ => Color.black,
        };
    }
}
