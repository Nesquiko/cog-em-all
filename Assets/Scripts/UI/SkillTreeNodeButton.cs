using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
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
    [SerializeField] private GameObject rankState;
    [SerializeField] private GameObject rankIndicatorPrefab;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private int activeRanks;

    private int maxRanks = 1;

    private readonly List<GameObject> rankIndicators = new();

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

        if (modifier is IRankedModifier ranked && state != SkillNodeState.Locked)
        {
            maxRanks = ranked.MaxRanks();
            GenerateRanks();
        }

        UpdateVisual();
    }

    private void Start()
    {
        StartCoroutine(DeferredConnection());
    }

    private void GenerateRanks()
    {
        foreach (Transform child in rankState.transform)
            Destroy(child.gameObject);
        rankIndicators.Clear();

        for (int i = 0; i < maxRanks; i++)
        {
            GameObject rankIndicator = Instantiate(rankIndicatorPrefab, rankState.transform);
            rankIndicators.Add(rankIndicator);
        }
    }

    private void FillRanks()
    {
        for (int i = 0; i < rankIndicators.Count; i++)
        {
            GameObject rankIndicator = rankIndicators[i];
            rankIndicator.GetComponentInChildren<Image>().fillAmount = activeRanks > i ? 1 : 0;
        }
    }

    public void HandleClick()
    {
        if (state == SkillNodeState.Locked) return;
        SetActiveRanks(activeRanks + 1);

        state = activeRanks > 0 ? SkillNodeState.Active : SkillNodeState.Unlocked;
    }

    public void SetActiveRanks(int ranks)
    {
        activeRanks = Mathf.Clamp(ranks, 0, maxRanks);
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
        lockedOverlay.SetActive(state == SkillNodeState.Locked);
        unlockedOverlay.SetActive(state == SkillNodeState.Unlocked);
        button.interactable = state != SkillNodeState.Locked;

        if (type == SkillNodeType.FactionSpecific)
            borderImage.color = FactionAccent(faction);

        if (modifier is IRankedModifier && state != SkillNodeState.Locked)
        {
            FillRanks();
            rankText.text = $"{activeRanks} / {maxRanks}";
        }
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
