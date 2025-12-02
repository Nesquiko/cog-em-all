using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SkillNodeType
{
    Minor,
    Major,
    FactionSpecific,
}

public enum SkillNodeState
{
    Locked = 0,
    Unlocked = 1,
    Active = 2,
}

public class SkillTreeNodeButton : MonoBehaviour
{
    [SerializeField] private string skillSlug;
    [SerializeField] private Faction faction;
    [SerializeField] private Button button;
    [SerializeField] private GameObject unlockedOverlay;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Image borderImage;
    [SerializeField] private SkillNodeState state;
    [SerializeField] private SkillNodeType type;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private SkillTreeNodeButton[] prerequisities;
    [SerializeField] private ModifiersDatabase modifiersDatabase;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private RectTransform connectionLayer;
    [SerializeField] private string linePrefix;

    Modifier modifier;

    public RectTransform RectTransform => rectTransform;

    public SkillNodeState State
    {
        get => state;
        set
        {
            state = value;
            UpdateVisual();
        }
    }

    private void Awake()
    {
        modifier = GetModifier();
        UpdateVisual();
    }

    private void Start()
    {
        StartCoroutine(DeferredConnection());
    }

    private Modifier GetModifier()
    {
        return (type) switch
        {
            SkillNodeType.Minor => modifiersDatabase.GetGenericMinorModifierBySlug(skillSlug),
            SkillNodeType.Major => modifiersDatabase.GetGenericMajorModifierBySlug(skillSlug),
            SkillNodeType.FactionSpecific => (faction) switch
            {
                Faction.TheBrassArmy => modifiersDatabase.GetTheBrassArmyModifierBySlug(skillSlug),
                Faction.TheValveboundSeraphs => modifiersDatabase.GetValveboundSeraphsModifierBySlug(skillSlug),
                Faction.OverpressureCollective => modifiersDatabase.GetOverpressureCollectiveModifierBySlug(skillSlug),
                _ => null,
            },
            _ => null,
        };
    }

    private void UpdateVisual()
    {
        lockedOverlay.SetActive(state == SkillNodeState.Locked);
        unlockedOverlay.SetActive(state == SkillNodeState.Unlocked);
        button.interactable = state != SkillNodeState.Locked;

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

    private IEnumerator DeferredConnection()
    {
        yield return null;
        GenerateConnections();
    }

    [ContextMenu("Generate Connections")]
    public void GenerateConnections()
    {
        foreach (var pr in prerequisities)
        {
            DrawConnection(rectTransform, pr.RectTransform);
        }
    }

    private void DrawConnection(RectTransform from, RectTransform to)
    {
        var lineObj = Instantiate(connectionPrefab, connectionLayer);
        lineObj.name = $"{linePrefix}_{from.name}_{to.name}";

        var line = lineObj.GetComponent<Image>();
        var rt = line.rectTransform;

        Vector2 start = connectionLayer.InverseTransformPoint(from.position);
        Vector2 end = connectionLayer.InverseTransformPoint(to.position);

        Vector2 direction = end - start;
        float length = direction.magnitude;

        rt.sizeDelta = new(length, 4f);
        rt.anchoredPosition = start + direction * 0.5f;
        rt.rotation = Quaternion.Euler(0, 0,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    [ContextMenu("Clear Connections")]
    public void ClearConnections()
    {
        for (int i = connectionLayer.childCount - 1; i >= 0; i--)
        {
            var c = connectionLayer.GetChild(i);
            if (c.name.StartsWith(linePrefix))
                DestroyImmediate(c.gameObject);
        }
    }
}
