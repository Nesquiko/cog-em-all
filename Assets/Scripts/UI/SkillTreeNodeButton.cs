using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
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

public class SkillTreeNodeButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string skillSlug;
    [SerializeField] private Faction faction;
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private SkillNodeType type;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private SkillTreeNodeButton[] prerequisities;
    [SerializeField] private SkillTreeNodeButton[] postrequisities;
    [SerializeField] private ModifiersDatabase modifiersDatabase;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private RectTransform connectionLayer;
    [SerializeField] private string linePrefix;
    [SerializeField] private GameObject rankState;
    [SerializeField] private GameObject rankIndicatorPrefab;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private SkillTree skillTree;

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltip;
    [SerializeField] private TMP_Text tooltipTitle;
    [SerializeField] private TMP_Text tooltipDescription;
    [SerializeField] private TMP_Text tooltipActiveRanksLabel;
    [SerializeField] private TMP_Text tooltipActiveRanks;
    [SerializeField] private TMP_Text tooltipMaxRanksLabel;
    [SerializeField] private TMP_Text tooltipMaxRanks;
    [SerializeField] private TMP_Text tooltipRanksSeparator;

    public string SkillSlug => skillSlug;

    private int activeRanks = 0;  // 0 -> unlocked/locked (based on prerequisities), 1+ -> active
    public int ActiveRanks => activeRanks;
    private SkillNodeState state = SkillNodeState.Unlocked;

    private int maxRanks = 1;

    private readonly List<GameObject> rankIndicators = new();

    Modifier modifier;

    public RectTransform RectTransform => rectTransform;

    public SkillNodeState State => state;

    private ScaleOnHover scaleOnHover;

    public event Action<SkillTreeNodeButton, int> OnActiveRanksChanged;

    private void Awake()
    {
        modifier = GetModifier();

        scaleOnHover = button.GetComponent<ScaleOnHover>();

        tooltip.SetActive(false);
        tooltipTitle.text = modifier.name;
        tooltipDescription.text = modifier.description;
        tooltipActiveRanksLabel.text = "";
        tooltipActiveRanks.text = "";
        tooltipMaxRanksLabel.text = "";
        tooltipMaxRanks.text = "";
        tooltipRanksSeparator.text = "";

        if (modifier is IRankedModifier ranked)
        {
            maxRanks = ranked.MaxRanks();
            tooltipActiveRanksLabel.text = "Active ranks";
            tooltipActiveRanks.text = activeRanks.ToString();
            tooltipMaxRanksLabel.text = "Max ranks";
            tooltipMaxRanks.text = maxRanks.ToString();
            tooltipRanksSeparator.text = "/";
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        UpdateState();
        UpdateVisual();
    }

    public void UpdateState()
    {
        SkillNodeState oldState = state;

        bool canBeUnlocked = false;

        if (prerequisities == null || prerequisities.Length == 0)
        {
            canBeUnlocked = true;
        }
        else
        {
            foreach (var pr in prerequisities)
            {
                if (pr != null && pr.State == SkillNodeState.Active)
                {
                    canBeUnlocked = true;
                    break;
                }
            }
        }

        if (!canBeUnlocked)
        {
            state = SkillNodeState.Locked;
        }
        else
        {
            if (activeRanks >= 1)
                state = SkillNodeState.Active;
            else
                state = SkillNodeState.Unlocked;
        }

        if (state != oldState)
        {
            UpdateVisual();
            NotifyPostrequisities();
        }
    }

    private void NotifyPostrequisities()
    {
        if (postrequisities == null || postrequisities.Length == 0) return;

        foreach (var pr in postrequisities)
        {
            pr.UpdateState();
        }
    }

    private void GenerateRanks()
    {
        ClearRanks();

        for (int i = 0; i < maxRanks; i++)
        {
            GameObject rankIndicator = Instantiate(rankIndicatorPrefab, rankState.transform);
            rankIndicators.Add(rankIndicator);
        }
    }

    private void ClearRanks()
    {
        foreach (Transform child in rankState.transform)
            Destroy(child.gameObject);
        rankText.text = "";
        rankIndicators.Clear();
    }

    private void FillRanks()
    {
        for (int i = 0; i < rankIndicators.Count; i++)
        {
            var fillImage = rankIndicators[i].transform.GetChild(0).GetComponent<Image>();
            fillImage.fillAmount = (activeRanks > i) ? 1f : 0f;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (state == SkillNodeState.Locked) return;

        if (eventData.button == PointerEventData.InputButton.Left)
            HandleLeftClick();
        else if (eventData.button == PointerEventData.InputButton.Right)
            HandleRightClick();
    }

    private void HandleLeftClick()
    {
        SetActiveRanks(activeRanks + 1);
    }

    private void HandleRightClick()
    {
        SetActiveRanks(activeRanks - 1);
    }

    public void SetActiveRanks(int ranks)
    {
        int newActiveRanks = Mathf.Clamp(ranks, 0, maxRanks);
        int delta = newActiveRanks - activeRanks;
        if (delta == 0) return;

        if (delta > 0 && !skillTree.CanAssignSkillPoint) return;  // cannot assign skill point
        if (delta < 0 && (!skillTree.CanRemoveSkillPoint || postrequisities.Any(pr => pr.State == SkillNodeState.Active))) return;  // cannot remove skill point

        activeRanks = newActiveRanks;
        UpdateState();
        UpdateVisual();

        OnActiveRanksChanged?.Invoke(this, delta);
    }

    public void ResetActiveRanks()
    {
        activeRanks = 0;
        UpdateState();
        UpdateVisual();
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
        if (modifier is IRankedModifier)
        {
            tooltipActiveRanksLabel.text = "Active ranks";
            tooltipActiveRanks.text = activeRanks.ToString();
            tooltipMaxRanksLabel.text = "Max ranks";
            tooltipMaxRanks.text = maxRanks.ToString();
            tooltipRanksSeparator.text = "/";

            if (state != SkillNodeState.Locked)
            {
                GenerateRanks();
                FillRanks();
                rankText.text = $"{activeRanks} / {maxRanks}";
            }
        }
        else
        {
            tooltipActiveRanksLabel.text = state == SkillNodeState.Locked ? "Locked" : state == SkillNodeState.Unlocked ? "Unlocked" : "Active";
        }

        switch (state)
        {
            case SkillNodeState.Locked:
                backgroundImage.sprite = lockedSprite;
                break;
            case SkillNodeState.Unlocked:
                backgroundImage.sprite = unlockedSprite;
                break;
            case SkillNodeState.Active:
                backgroundImage.sprite = activeSprite;
                break;
        }

        button.interactable = state != SkillNodeState.Locked;
        scaleOnHover.enabled = state != SkillNodeState.Locked;

        UpdateRankVisual();
        UpdateTooltipVisual();
    }

    private void UpdateRankVisual()
    {
        if (modifier is not IRankedModifier ranked)
        {
            rankText.text = "";
            ClearRanks();
            return;
        }

        tooltipActiveRanksLabel.text = "Active ranks";
        tooltipActiveRanks.text = activeRanks.ToString();
        tooltipMaxRanksLabel.text = "Max ranks";
        tooltipMaxRanks.text = maxRanks.ToString();
        tooltipRanksSeparator.text = "/";

        if (state == SkillNodeState.Locked)
        {
            ClearRanks();
            return;
        }

        GenerateRanks();
        FillRanks();
        rankText.text = $"{activeRanks} / {maxRanks}";
    }

    private void UpdateTooltipVisual()
    {
        if (modifier is IRankedModifier) return;

        tooltipActiveRanksLabel.text = state switch
        {
            SkillNodeState.Locked => "Locked",
            SkillNodeState.Unlocked => "Unlocked",
            SkillNodeState.Active => "Active",
            _ => ""
        };
    }

    [ContextMenu("Generate Connections")]
    public void GenerateConnections()
    {
        if (prerequisities == null || prerequisities.Length == 0) return;

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

#if UNITY_EDITOR
[CustomEditor(typeof(SkillTreeNodeButton))]
public class SkillTreeNodeButtonEditor : Editor
{
    private SkillTreeNodeButton node;
    private SerializedProperty skillSlugProp;
    private SerializedProperty typeProp;
    private SerializedProperty factionProp;
    private SerializedProperty modifiersDatabaseProp;

    private void OnEnable()
    {
        node = (SkillTreeNodeButton)target;
        skillSlugProp = serializedObject.FindProperty("skillSlug");
        typeProp = serializedObject.FindProperty("type");
        factionProp = serializedObject.FindProperty("faction");
        modifiersDatabaseProp = serializedObject.FindProperty("modifiersDatabase");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(modifiersDatabaseProp);
        EditorGUILayout.PropertyField(typeProp);

        var type = (SkillNodeType)typeProp.enumValueIndex;

        if (type == SkillNodeType.FactionSpecific)
            EditorGUILayout.PropertyField(factionProp);
        else
        {
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(factionProp);
        }

        var db = modifiersDatabaseProp.objectReferenceValue as ModifiersDatabase;
        Modifier currentModifier = null;

        if (db != null)
        {
            DrawSlugDropdown(db);
            currentModifier = FindSelectedModifier(db, type, (Faction)factionProp.enumValueIndex, skillSlugProp.stringValue);
        }
        else
            EditorGUILayout.PropertyField(skillSlugProp);

        if (currentModifier != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(currentModifier.name, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(currentModifier.description, MessageType.None);
        }

        DrawPropertiesExcluding(serializedObject,
            "m_Script", "skillSlug", "type", "faction", "modifiersDatabase");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSlugDropdown(ModifiersDatabase db)
    {
        SkillNodeType type = (SkillNodeType)typeProp.enumValueIndex;
        Faction faction = (Faction)factionProp.enumValueIndex;

        List<string> slugs = new();
        switch (type)
        {
            case SkillNodeType.Minor:
                slugs = db.GenericMinorModifierSlugs;
                break;
            case SkillNodeType.Major:
                slugs = db.GenericMajorModifierSlugs;
                break;
            case SkillNodeType.FactionSpecific:
                switch (faction)
                {
                    case Faction.TheBrassArmy:
                        slugs = db.TheBrassArmyModifierSlugs;
                        break;
                    case Faction.TheValveboundSeraphs:
                        slugs = db.TheValveboundSeraphsModifierSlugs;
                        break;
                    case Faction.OverpressureCollective:
                        slugs = db.OverpressureCollectiveModifierSlugs;
                        break;
                }
                break;
        }

        if (slugs == null || slugs.Count == 0)
        {
            EditorGUILayout.HelpBox("No modifier slugs found for this selection.", MessageType.Info);
            EditorGUILayout.PropertyField(skillSlugProp);
            return;
        }

        int currentIndex = Mathf.Max(0, slugs.IndexOf(skillSlugProp.stringValue));
        int newIndex = EditorGUILayout.Popup("Skill Slug", currentIndex, slugs.ToArray());

        string newSlug = slugs[Mathf.Clamp(newIndex, 0, slugs.Count - 1)];
        skillSlugProp.stringValue = newSlug;
    }

    private Modifier FindSelectedModifier(ModifiersDatabase db, SkillNodeType nodeType, Faction faction, string slug)
    {
        if (db == null || string.IsNullOrEmpty(slug))
            return null;

        return nodeType switch
        {
            SkillNodeType.Minor => db.GetGenericMinorModifierBySlug(slug),
            SkillNodeType.Major => db.GetGenericMajorModifierBySlug(slug),
            SkillNodeType.FactionSpecific => faction switch
            {
                Faction.TheBrassArmy => db.GetTheBrassArmyModifierBySlug(slug),
                Faction.TheValveboundSeraphs => db.GetValveboundSeraphsModifierBySlug(slug),
                Faction.OverpressureCollective => db.GetOverpressureCollectiveModifierBySlug(slug),
                _ => null
            },
            _ => null,
        };
    }
}
#endif
