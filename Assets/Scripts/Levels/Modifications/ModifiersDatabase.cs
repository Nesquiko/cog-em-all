using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using JetBrains.Annotations;

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
    Replace,    // overrides the base value entirely
    PerPlacedTowerAddPercentage // for each tower on the map, add +X% (if there are 2 towers, adds +2*X% => should multiplies by 1 + 2*X )
}

[Serializable]
public abstract class Modifier
{

    public string name;
    public string slug;
    [TextArea] public string description;
    public ModifierType type;
}

public enum TowerModifierApplyTo
{
    All = -1,
    Gatling = TowerTypes.Gatling,
    Tesla = TowerTypes.Tesla,
    Mortar = TowerTypes.Mortar,
    Flamethrower = TowerTypes.Flamethrower,
}

[Serializable]
public class TowerModifier : Modifier
{
    public TowerModifierApplyTo applyTo;
    public TowerAttribute modifiedAttribute;
    public ChangeType changeType;
    public float change;

    public static bool AppliesTo(TowerModifier mod, TowerTypes towerType)
    {
        return mod.applyTo == TowerModifierApplyTo.All || (TowerTypes)mod.applyTo == towerType;
    }
}

public enum EnemyModifierApplyTo
{
    All = -1,
    Bandit = EnemyType.Bandit,
    Dreadnought = EnemyType.Dreadnought,
    Bomber = EnemyType.Bomber,
}

[Serializable]
public class EnemyModifier : Modifier
{
    public EnemyModifierApplyTo applyTo;
    public EnemyAttributes modifiedAttribute;
    public ChangeType changeType;
    public float change;

    public static bool AppliesTo(EnemyModifier mod, EnemyType enemyType)
    {
        return mod.applyTo == EnemyModifierApplyTo.All || (EnemyType)mod.applyTo == enemyType;
    }
}

public enum EconomyAttributes
{
    PassiveGearsAmount,
    PassiveGearsTick,
    PerEnemyKillGears,
    TowersUpgradeDiscount,
    BaseOnHitDeduction
}

[Serializable]
public class EconomyModifier : Modifier
{
    public EconomyAttributes category;
    public ChangeType changeType;
    public float change;
}

[Serializable]
public class EconomyChange
{
    public EconomyAttributes category;
    public ChangeType changeType;
    public float change;
}

[Serializable]
public class EconomyDoubleEdgedMofifier : Modifier
{
    public EconomyChange benefit;

    public EconomyChange disadvantage;
}

[Serializable]
public class UnlockTowerTypeModifier : Modifier
{
    public TowerTypes toUnlock;
}

public enum TowerUnlocks
{
    StimMode = 0,
}

[Serializable]
public class UnlockTowerAbilityModifier : Modifier
{
    public TowerModifierApplyTo unlockOn;
    public TowerUnlocks unlock;
}

[Serializable]
public class UnlockTowerUpgradeModifier : Modifier
{
    public TowerTypes applyTo;
    public int allowLevel;
}

public enum BaseUnlocks
{
    HealthRegen = 0,
}

[Serializable]
public class BaseUnlock : Modifier
{
    public BaseUnlocks unlocks;
}

[Serializable]
public class AbilityUnlock : Modifier
{
    public SkillTypes toUnlock;
}

[Serializable]
public class AbilityModifierUnlock : Modifier
{
    public SkillTypes applyTo;
    public SkillModifiers toUnlock;
}

[Serializable]
// I know this isn't generic, but there isn't another modifier for "double edged" behaviour on abilities,
// so I am betting on my future self, that I will not come up with something like this...
public class AbilityNoCooldownCost100Gears : Modifier { }

[CreateAssetMenu(fileName = "ModifiersDatabase", menuName = "Scriptable Objects/ModifiersDatabase")]
public class ModifiersDatabase : ScriptableObject
{

    [SerializeReference] private List<Modifier> theBrassArmyModifiers = new();
    public List<Modifier> TheBrassArmyModifiers => theBrassArmyModifiers;
    [SerializeReference] private List<string> theBrassArmyStaticModifiersSlugs = new();
    public List<Modifier> TheBrassArmyStaticModifiers => ResolveModifiers(theBrassArmyModifiers, theBrassArmyStaticModifiersSlugs);

    [SerializeReference] private List<Modifier> theValveboundSeraphsModifiers = new();
    public List<Modifier> TheValveboundSeraphsModifiers => theValveboundSeraphsModifiers;
    [SerializeReference] private List<string> theValveboundSeraphsStaticModifiersSlugs = new();
    public List<Modifier> TheValveboundSeraphsStaticModifiers => ResolveModifiers(theValveboundSeraphsModifiers, theValveboundSeraphsStaticModifiersSlugs);

    [SerializeReference] private List<Modifier> overpressureCollectiveModifiers = new();
    public List<Modifier> OverpressureCollectiveModifiers => overpressureCollectiveModifiers;
    [SerializeReference] private List<string> overpressureCollectiveStaticModifiersSlugs = new();
    public List<Modifier> OverpressureCollectiveStaticModifiers => ResolveModifiers(overpressureCollectiveModifiers, overpressureCollectiveStaticModifiersSlugs);

    private static List<Modifier> ResolveModifiers(List<Modifier> source, List<string> slugRefs)
    {
        var result = new List<Modifier>(slugRefs.Count);
        if (source == null || slugRefs == null)
            return result;

        foreach (var slug in slugRefs)
        {
            Assert.IsFalse(string.IsNullOrEmpty(slug), $"empty slug in slugs {slugRefs}");

            var found = source.Find(m => m.slug == slug);
            Assert.IsNotNull(found, $"didn't find a modifier with slug {slug}");
            result.Add(found);
        }

        return result;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(ModifiersDatabase))]
public class ModifiersDatabaseEditor : Editor
{
    private SerializedProperty brassArmyModifsProp;
    private SerializedProperty brassArmyStaticModifsProp;

    private SerializedProperty seraphsModifsProp;
    private SerializedProperty seraphsStaticModifsProp;

    private SerializedProperty collectiveModifsProp;
    private SerializedProperty collectiveStaticModifsProp;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        AutoSyncSlugs(brassArmyModifsProp);
        AutoSyncSlugs(brassArmyStaticModifsProp);

        AutoSyncSlugs(seraphsModifsProp);
        AutoSyncSlugs(seraphsStaticModifsProp);

        AutoSyncSlugs(collectiveModifsProp);
        AutoSyncSlugs(collectiveStaticModifsProp);

        // Draw each faction list with its own controls
        DrawFactionSection(
            header: "The Brass Army",
            faction: Faction.TheBrassArmy,
            factionModifsProp: brassArmyModifsProp,
            factionStaticSlugsProp: brassArmyStaticModifsProp
        );

        EditorGUILayout.Space(10);

        DrawFactionSection(
            header: "The Valvebound Seraphs",
            faction: Faction.TheValveboundSeraphs,
            factionModifsProp: seraphsModifsProp,
            factionStaticSlugsProp: seraphsStaticModifsProp
        );

        EditorGUILayout.Space(10);

        DrawFactionSection(
            header: "Overpressure Collective",
            faction: Faction.OverpressureCollective,
            factionModifsProp: collectiveModifsProp,
            factionStaticSlugsProp: collectiveStaticModifsProp
        );

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void OnEnable()
    {
        brassArmyModifsProp = serializedObject.FindProperty("theBrassArmyModifiers");
        brassArmyStaticModifsProp = serializedObject.FindProperty("theBrassArmyStaticModifiersSlugs");

        seraphsModifsProp = serializedObject.FindProperty("theValveboundSeraphsModifiers");
        seraphsStaticModifsProp = serializedObject.FindProperty("theValveboundSeraphsStaticModifiersSlugs");

        collectiveModifsProp = serializedObject.FindProperty("overpressureCollectiveModifiers");
        collectiveStaticModifsProp = serializedObject.FindProperty("overpressureCollectiveStaticModifiersSlugs");
    }

    private void DrawFactionSection(string header, Faction faction, SerializedProperty factionModifsProp, SerializedProperty factionStaticSlugsProp)
    {
        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);

        // list of faction specific modifications
        EditorGUILayout.PropertyField(factionModifsProp, includeChildren: true);

        // buttons to insert modifier subclass instances
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Tower Modifier"))
                Add(factionModifsProp, new TowerModifier());
            if (GUILayout.Button("Add Enemy Modifiery"))
                Add(factionModifsProp, new EnemyModifier());
            if (GUILayout.Button("Add Economy Modifiery"))
                Add(factionModifsProp, new EconomyModifier());
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Unlock Tower Type"))
                Add(factionModifsProp, new UnlockTowerTypeModifier());
            if (GUILayout.Button("Add Unlock Tower Ability"))
                Add(factionModifsProp, new UnlockTowerAbilityModifier());
            if (GUILayout.Button("Add Unlock Tower Upgrade"))
                Add(factionModifsProp, new UnlockTowerUpgradeModifier());
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add ability unlock"))
                Add(factionModifsProp, new AbilityUnlock());
            if (GUILayout.Button("Add ability modifier unlock"))
                Add(factionModifsProp, new AbilityModifierUnlock());
        }

        if (faction == Faction.TheValveboundSeraphs)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Base Unlock"))
                    Add(factionModifsProp, new BaseUnlock());
            }
        }

        if (faction == Faction.OverpressureCollective)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Double edged economy modifier"))
                    Add(factionModifsProp, new EconomyDoubleEdgedMofifier());
                if (GUILayout.Button("Add Double edged ability modifier"))
                    Add(factionModifsProp, new AbilityNoCooldownCost100Gears());
            }
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Static Modifiers (by slug)", EditorStyles.miniBoldLabel);

        DrawStaticSlugList(factionModifsProp, factionStaticSlugsProp);
    }

    private static void Add(SerializedProperty listProp, Modifier instance)
    {
        int idx = listProp.arraySize;
        listProp.InsertArrayElementAtIndex(idx);
        var elem = listProp.GetArrayElementAtIndex(idx);
        elem.managedReferenceValue = instance;
    }

    private void DrawStaticSlugList(SerializedProperty factionModifsProp, SerializedProperty staticSlugsProp)
    {
        var slugs = CollectSlugs(factionModifsProp);

        if (slugs.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No modifiers defined for this faction. " +
                "Add modifiers above before selecting static modifiers.",
                MessageType.Info
            );
        }

        int toRemove = -1;
        for (int i = 0; i < staticSlugsProp.arraySize; i++)
        {
            var slugProp = staticSlugsProp.GetArrayElementAtIndex(i);
            string currentSlug = slugProp.stringValue;

            EditorGUILayout.BeginHorizontal();

            int newIndex = 0;
            if (slugs.Count > 0)
            {
                newIndex = Mathf.Max(0, slugs.IndexOf(currentSlug));

                int selectedIndex = EditorGUILayout.Popup(
                    label: $"Static {i}",
                    selectedIndex: newIndex,
                    displayedOptions: slugs.ToArray()
                );

                if (selectedIndex >= 0 && selectedIndex < slugs.Count)
                {
                    slugProp.stringValue = slugs[selectedIndex];
                }
            }
            else
            {
                EditorGUILayout.PropertyField(
                    slugProp,
                    new GUIContent($"Static {i}")
                );
            }

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                toRemove = i;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (toRemove >= 0)
        {
            staticSlugsProp.DeleteArrayElementAtIndex(toRemove);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(slugs.Count == 0))
            {
                if (GUILayout.Button("Add Static", GUILayout.Width(100)))
                {
                    int idx = staticSlugsProp.arraySize;
                    staticSlugsProp.InsertArrayElementAtIndex(idx);
                    var newElem = staticSlugsProp.GetArrayElementAtIndex(idx);
                    newElem.stringValue = slugs.Count > 0 ? slugs[0] : string.Empty;
                }
            }
        }
    }

    private static List<string> CollectSlugs(SerializedProperty factionModifsProp)
    {
        Assert.IsTrue(factionModifsProp.isArray);

        var list = new List<string>();

        for (int i = 0; i < factionModifsProp.arraySize; i++)
        {
            var element = factionModifsProp.GetArrayElementAtIndex(i);
            Assert.IsNotNull(element);
            Assert.AreEqual(element.propertyType, SerializedPropertyType.ManagedReference);

            var slugProp = element.FindPropertyRelative("slug");
            Assert.IsNotNull(slugProp);

            string slug = slugProp.stringValue;

            if (string.IsNullOrEmpty(slug)) continue;

            // prevents duplications
            Assert.IsTrue(!list.Contains(slug));

            list.Add(slug);
        }

        return list;
    }

    private void AutoSyncSlugs(SerializedProperty listProp)
    {

        if (listProp == null || !listProp.isArray)
            return;

        for (int i = 0; i < listProp.arraySize; i++)
        {
            var element = listProp.GetArrayElementAtIndex(i);
            if (element == null || element.propertyType != SerializedPropertyType.ManagedReference)
                continue;

            var nameProp = element.FindPropertyRelative("name");
            var slugProp = element.FindPropertyRelative("slug");

            if (nameProp == null || slugProp == null)
                continue;

            string nameValue = nameProp.stringValue;
            string newSlug = MakeSlug(nameValue);
            slugProp.stringValue = newSlug;
        }
    }

    private static string MakeSlug(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        string s = raw.Trim().ToLowerInvariant();

        return s.Replace(' ', '_');
    }
}
#endif