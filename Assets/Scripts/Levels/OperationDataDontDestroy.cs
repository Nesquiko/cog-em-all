using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System.Linq;


public class OperationDataDontDestroy : MonoBehaviour
{
    [SerializeField] private Faction faction;
    public Faction Faction => faction;

    [SerializeReference] private List<Modifier> modifiers = new();
    public List<Modifier> Modifiers => modifiers;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(Faction faction, List<Modifier> modifiers)
    {
        this.faction = faction;
        this.modifiers = modifiers;
    }

    private const string DEV_OPERATION_DATA_PREFAB = "Assets/Prefabs/Levels/DevOperationData.prefab";

    public static OperationDataDontDestroy GetOrReadDev()
    {
        var existing = FindFirstObjectByType<OperationDataDontDestroy>();
        if (existing != null) return existing;

        Debug.Log("Reading DEV operation data");

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DEV_OPERATION_DATA_PREFAB);
        Assert.IsNotNull(
            prefab,
            $"DevOperationData prefab not found at '{DEV_OPERATION_DATA_PREFAB}'. Make sure the path is correct and the prefab exists."
        );

        var instance = Instantiate(prefab);
        Assert.IsNotNull(instance, "Failed to instantiate DevOperationData prefab");

        var data = instance.GetComponent<OperationDataDontDestroy>();
        Assert.IsNotNull(
            data,
            "DevOperationData prefab does not contain an OperationDataDontDestroy component"
        );

        return data;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(OperationDataDontDestroy))]
public class OperationDataDontDestroyEditor : Editor
{
    private ModifiersDatabase modifiersDatabase;

    private string[] genericMinorOptions = new string[0];
    private int genericMinorIndex;

    private string[] genericMajorOptions = new string[0];
    private int genericMajorIndex;

    private string[] brassArmyOptions = new string[0];
    private int brassArmyIndex;

    private string[] valveboundSeraphsOptions = new string[0];
    private int valveboundSeraphsIndex;

    private string[] overpressureCollectiveOptions = new string[0];
    private int overpressureCollectiveIndex;

    private void OnEnable()
    {
        var guids = AssetDatabase.FindAssets("t:ModifiersDatabase");
        Assert.AreEqual(1, guids.Length);
        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
        modifiersDatabase = AssetDatabase.LoadAssetAtPath<ModifiersDatabase>(path);

        if (modifiersDatabase == null)
        {
            genericMinorOptions = new string[0];
            genericMajorOptions = new string[0];
            brassArmyOptions = new string[0];
            valveboundSeraphsOptions = new string[0];
            overpressureCollectiveOptions = new string[0];
            return;
        }

        genericMinorOptions = modifiersDatabase.GenericMinorModifierSlugs.ToArray();
        genericMajorOptions = modifiersDatabase.GenericMajorModifierSlugs.ToArray();
        brassArmyOptions = modifiersDatabase.TheBrassArmyModifierSlugs.ToArray();
        valveboundSeraphsOptions = modifiersDatabase.TheValveboundSeraphsModifierSlugs.ToArray();
        overpressureCollectiveOptions =
            modifiersDatabase.OverpressureCollectiveModifierSlugs.ToArray();

        // start all popups on placeholder
        genericMinorIndex = 0;
        genericMajorIndex = 0;
        brassArmyIndex = 0;
        valveboundSeraphsIndex = 0;
        overpressureCollectiveIndex = 0;
    }

    public override void OnInspectorGUI()
    {
        var data = (OperationDataDontDestroy)target;

        EditorGUILayout.LabelField("Operation Data", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        var newFaction = (Faction)EditorGUILayout.EnumPopup("Faction", data.Faction);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(data, "Change Faction");
            var modsCopy = data.Modifiers;
            data.Initialize(newFaction, modsCopy);
            EditorUtility.SetDirty(data);
        }

        EditorGUILayout.Space();

        modifiersDatabase = (ModifiersDatabase)EditorGUILayout.ObjectField(
            "Modifiers Database",
            modifiersDatabase,
            typeof(ModifiersDatabase),
            false
        );

        if (modifiersDatabase == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a ModifiersDatabase to use the modifier pickers.",
                MessageType.Warning
            );
            return;
        }

        if (genericMinorOptions == null || genericMinorOptions.Length == 0)
        {
            OnEnable();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(
            "Add Modifiers (select an entry to add it)",
            EditorStyles.boldLabel
        );

        DrawPopup(
            "Generic Minor",
            genericMinorOptions,
            ref genericMinorIndex,
            modifiersDatabase.GenericMinorModifiers,
            data
        );

        DrawPopup(
            "Generic Major",
            genericMajorOptions,
            ref genericMajorIndex,
            modifiersDatabase.GenericMajorModifiers,
            data
        );

        DrawPopup(
            "Brass Army",
            brassArmyOptions,
            ref brassArmyIndex,
            modifiersDatabase.TheBrassArmyModifiers,
            data
        );

        DrawPopup(
            "Valvebound Seraphs",
            valveboundSeraphsOptions,
            ref valveboundSeraphsIndex,
            modifiersDatabase.TheValveboundSeraphsModifiers,
            data
        );

        DrawPopup(
            "Overpressure Collective",
            overpressureCollectiveOptions,
            ref overpressureCollectiveIndex,
            modifiersDatabase.OverpressureCollectiveModifiers,
            data
        );

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Current Modifiers", EditorStyles.boldLabel);

        for (var i = 0; i < data.Modifiers.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            var modifier = data.Modifiers[i];

            var slug = string.IsNullOrWhiteSpace(modifier.slug) ? "<no-slug>" : modifier.slug;
            var modName = string.IsNullOrWhiteSpace(modifier.name) ? "<no-name>" : modifier.name;

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(100));
            EditorGUILayout.LabelField($"{slug} — {modName}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                string.IsNullOrWhiteSpace(modifier.description)
                    ? "<no-description>"
                    : modifier.description,
                EditorStyles.wordWrappedMiniLabel
            );
            EditorGUILayout.EndVertical();

            if (modifier is IRankedModifier ranked)
            {
                int current = ranked.CurrentRanks();
                int max = ranked.MaxRanks();

                // small "Ranks" label + compact int field
                EditorGUILayout.LabelField("Ranks", GUILayout.Width(40));
                int newRanks = EditorGUILayout.IntField(current, GUILayout.Width(40));
                EditorGUILayout.LabelField($"/ {max}", GUILayout.Width(35));

                newRanks = Mathf.Clamp(newRanks, 0, max);
                if (newRanks != current)
                {
                    Undo.RecordObject(data, "Change Modifier Ranks");
                    ranked.SetCurrentRanks(newRanks);
                    EditorUtility.SetDirty(data);
                }
            }

            // push the remove button to the far right
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                Undo.RecordObject(data, "Remove Modifier");
                data.Modifiers.RemoveAt(i);
                EditorUtility.SetDirty(data);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(data);
        }
    }

    private void DrawPopup(
        string label,
        string[] options,
        ref int index,
        List<Modifier> sourceList,
        OperationDataDontDestroy data
    )
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(label, GUILayout.Width(140));

            if (options == null || options.Length == 0)
            {
                EditorGUILayout.LabelField("No modifiers", EditorStyles.miniLabel);
                return;
            }

            var optionsWithPlaceholder = new string[options.Length + 1];
            optionsWithPlaceholder[0] = "<Select from slugs>";
            for (var i = 0; i < options.Length; i++)
            {
                optionsWithPlaceholder[i + 1] = options[i];
            }

            var previousIndex = index;
            index = EditorGUILayout.Popup(index, optionsWithPlaceholder);

            // user chose a real modifier (index > 0)
            if (index > 0 && index != previousIndex)
            {
                var realIndex = index - 1;
                var slug = options[realIndex];
                var modifier = sourceList.FirstOrDefault(m => m != null && m.slug == slug);

                if (modifier == null)
                {
                    Debug.LogError($"Modifier with slug '{slug}' not found in list for {label}");
                }
                else if (!data.Modifiers.Contains(modifier))
                {
                    Undo.RecordObject(data, "Add Modifier");
                    if (modifier is IRankedModifier ranked)
                    {
                        ranked.SetCurrentRanks(1);
                    }
                    data.Modifiers.Add(modifier);
                    EditorUtility.SetDirty(data);
                }
                else
                {
                    Debug.Log($"Modifier '{slug}' is already in the operation modifiers list.");
                }

                // reset back to placeholder so the same item can be added again later if needed
                index = 0;
            }
        }
    }
}

#endif