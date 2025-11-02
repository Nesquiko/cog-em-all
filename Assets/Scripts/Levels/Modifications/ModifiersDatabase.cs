using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum Faction
{
    TheBrassArmy,
    TheValveboundSeraphs,
    OverpressureCollective,
}

public enum ModifierType
{
    Buff,
    Debuff,
    Unlock
}

public enum ChangeType
{
    Add,       // adds flat value (e.g., +1 gear)
    Mult,      // multiplies (e.g., 1.10 = +10%)
    Replace    // overrides the base value entirely
}

[Serializable]
public abstract class Modifier
{
    public ModifierType type;
}

[Serializable]
public class TowerModifier : Modifier
{
    public TowerTypes applyTo;
    public TowerAttribute modifiedAttribute;
    public ChangeType changeType;
    public float change;
}

[Serializable]
public class EnemyModifier : Modifier
{
    public EnemyType applyTo;
    public EnemyAttributes modifiedAttribute;
    public ChangeType changeType;
    public float change;
}

public enum EconomyAttributes
{
    PassiveGearsAmount,
    PassiveGearsTick,
    PerEnemyKillGears,
}

[Serializable]
public class EconomyModifier : Modifier
{
    public EconomyAttributes category;
    public ChangeType changeType;
    public float change;
}

[Serializable]
public class UnlockTowerTypeModifier : Modifier
{
    public TowerTypes toUnlock;
}

[Serializable]
public class UnlockTowerAbilityModifier : Modifier { }

[Serializable]
public class UnlockTowerUpgradeModifier : Modifier
{
    public TowerTypes applyTo;
    public int allowLevel;
}

[CreateAssetMenu(fileName = "ModifiersDatabase", menuName = "Scriptable Objects/ModifiersDatabase")]
public class ModifiersDatabase : ScriptableObject
{
    [SerializeReference] private List<Modifier> theBrassArmyBaseModifiers = new();
    public List<Modifier> TheBrassArmyBaseModifiers => theBrassArmyBaseModifiers;
    [SerializeReference] private List<Modifier> theValveboundSeraphsBaseModifiers = new();
    public List<Modifier> TheValveboundSeraphsBaseModifiers => theValveboundSeraphsBaseModifiers;
    [SerializeReference] private List<Modifier> overpressureCollectiveBaseModifiers = new();
    public List<Modifier> OverpressureCollectiveBaseModifiers => overpressureCollectiveBaseModifiers;
}

#if UNITY_EDITOR

[CustomEditor(typeof(ModifiersDatabase))]
public class ModifiersDatabaseEditor : Editor
{
    private SerializedProperty brassProp;
    private SerializedProperty seraphsProp;
    private SerializedProperty collectiveProp;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw each faction list with its own controls
        DrawFactionSection(
            header: "The Brass Army",
            listProp: brassProp
        );

        EditorGUILayout.Space(10);

        DrawFactionSection(
            header: "The Valvebound Seraphs",
            listProp: seraphsProp
        );

        EditorGUILayout.Space(10);

        DrawFactionSection(
            header: "Overpressure Collective",
            listProp: collectiveProp
        );

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void OnEnable()
    {
        brassProp = serializedObject.FindProperty("theBrassArmyBaseModifiers");
        seraphsProp = serializedObject.FindProperty("theValveboundSeraphsBaseModifiers");
        collectiveProp = serializedObject.FindProperty("overpressureCollectiveBaseModifiers");
    }

    private void DrawFactionSection(string header, SerializedProperty listProp)
    {
        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);

        // Draw the list (SerializeReference will show concrete subclass fields)
        EditorGUILayout.PropertyField(listProp, includeChildren: true);

        // Add buttons to insert specific subclass instances
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Tower Modifier"))
                Add(listProp, new TowerModifier());
            if (GUILayout.Button("Add Enemy Modifiery"))
                Add(listProp, new EnemyModifier());
            if (GUILayout.Button("Add Economy Modifiery"))
                Add(listProp, new EconomyModifier());
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Unlock Tower Type"))
                Add(listProp, new UnlockTowerTypeModifier());
            if (GUILayout.Button("Add Unlock Tower Ability"))
                Add(listProp, new UnlockTowerAbilityModifier());
            if (GUILayout.Button("Add Unlock Tower Upgrade"))
                Add(listProp, new UnlockTowerUpgradeModifier());
        }
    }

    private static void Add(SerializedProperty listProp, Modifier instance)
    {
        int idx = listProp.arraySize;
        listProp.InsertArrayElementAtIndex(idx);
        var elem = listProp.GetArrayElementAtIndex(idx);
        elem.managedReferenceValue = instance;
    }
}
#endif