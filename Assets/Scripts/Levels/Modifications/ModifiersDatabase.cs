using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;

public enum Faction
{
    TheBrassArmy,
    TheValveboundSeraphs,
    OverpressureCollective,
}

[Obsolete("For now this enum is useless, maybe the modifiers are expressive enough without this. First implement the logic for modifiers, then remove this if it is useless.")]
public enum ModifierType
{
    Buff,
    Debuff,
    Unlock
}

public enum ChangeType
{
    Add = 0,       // adds flat value (e.g., +1 gear)
    Mult = 1,      // multiplies (e.g., 1.10 = +10%)
    Replace = 2,    // overrides the base value entirely
    PerPlacedTowerAddPercentage = 3, // for each tower on the map, add +X% (if there are 2 towers, adds +2*X% => should multiplies by 1 + 2*X )
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
    public int maxRanks = 1;

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

public enum EnemyAbilities
{
    BomberOnDeathFriendlyFire = 0,
}

[Serializable]
class EnemyAbilityUnlock : Modifier
{
    public EnemyAbilities toUnlock;
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
    ManualMode = 1,
    OnHillRangeIncrease = 2,
    OnHitDot = 3,
    ArmorShreding = 4,
    OnHitStun = 5,
    OnHitRemoveEnemyAbilities = 6,
    OnHitSlow = 7,
    OnHitKnockback = 8,
}

[Serializable]
public class UnlockTowerAbilityModifier : Modifier
{
    public TowerModifierApplyTo unlockOn;
    public TowerUnlocks unlock;
}

[Serializable]
public class UnlockTowerAbilityOnMultipleModifier : Modifier
{
    public List<TowerModifierApplyTo> unlockOn;
    public TowerUnlocks unlock;
}

[Serializable]
public class TowerAttributeChange
{
    public TowerAttribute modifiedAttribute;
    public ChangeType changeType;
    public float change;
}


[Serializable]
public class UnlockTowerUpgradeModifier : Modifier
{
    public TowerTypes applyTo;
    public int allowLevel;
    public List<TowerAttributeChange> upgrades;
}

public enum BaseUnlocks
{
    HealthRegen = 0,
    AirShipAirStrike = 1,
    AirShipFreeze = 2,
    AirShipDisableEnemyAbilitiesZone = 3,
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
public class AbilityAddUsages : Modifier
{
    public SkillTypes addTo;
    public int maxRanks;
    public int numOfUsages;
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

public enum StimModeModifiers
{
    /// <summary>
    /// For example mortar shoots 2 shells, or tesla fires 2 beams
    /// </summary>
    DoublePayload = 0,
    FlamethrowerSweepLeftToRight = 1,

    /// <summary>
    /// The stim mode keeps upping the stats of the tower until the end
    /// </summary>
    ExponentialStim = 2,
}

[Serializable]
public class StimModeModifier : Modifier
{
    public TowerTypes applyTo;
    public StimModeModifiers modifies;
}

[CreateAssetMenu(fileName = "ModifiersDatabase", menuName = "Scriptable Objects/ModifiersDatabase")]
public class ModifiersDatabase : ScriptableObject
{

    [SerializeReference] private List<Modifier> genericMinorModifiers = new();
    public List<Modifier> GenericMinorModifiers => genericMinorModifiers;

    [SerializeReference] private List<Modifier> genericMajorModifiers = new();
    public List<Modifier> GenericMajorModifiers => genericMajorModifiers;

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
    private SerializedProperty genericMinorModifsProp;
    private SerializedProperty genericMajorModifsProp;

    private SerializedProperty brassArmyModifsProp;
    private SerializedProperty brassArmyStaticModifsProp;

    private SerializedProperty seraphsModifsProp;
    private SerializedProperty seraphsStaticModifsProp;

    private SerializedProperty collectiveModifsProp;
    private SerializedProperty collectiveStaticModifsProp;

    private GUIStyle BigBoldLabelStyle => new(EditorStyles.boldLabel)
    {
        fontSize = 20,
        fontStyle = FontStyle.Bold
    };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        AutoSyncSlugs(genericMinorModifsProp);
        AutoSyncSlugs(genericMajorModifsProp);

        AutoSyncSlugs(brassArmyModifsProp);
        AutoSyncSlugs(brassArmyStaticModifsProp);

        AutoSyncSlugs(seraphsModifsProp);
        AutoSyncSlugs(seraphsStaticModifsProp);

        AutoSyncSlugs(collectiveModifsProp);
        AutoSyncSlugs(collectiveStaticModifsProp);

        DrawGeneric(genericMinorModifsProp);
        DrawGeneric(genericMajorModifsProp);

        EditorGUILayout.Space(10);

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
        genericMinorModifsProp = serializedObject.FindProperty("genericMinorModifiers");
        genericMajorModifsProp = serializedObject.FindProperty("genericMajorModifiers");

        brassArmyModifsProp = serializedObject.FindProperty("theBrassArmyModifiers");
        brassArmyStaticModifsProp = serializedObject.FindProperty("theBrassArmyStaticModifiersSlugs");

        seraphsModifsProp = serializedObject.FindProperty("theValveboundSeraphsModifiers");
        seraphsStaticModifsProp = serializedObject.FindProperty("theValveboundSeraphsStaticModifiersSlugs");

        collectiveModifsProp = serializedObject.FindProperty("overpressureCollectiveModifiers");
        collectiveStaticModifsProp = serializedObject.FindProperty("overpressureCollectiveStaticModifiersSlugs");
    }

    private void DrawGeneric(SerializedProperty modifsListProp)
    {
        EditorGUILayout.LabelField("Generic major modifications", BigBoldLabelStyle);
        EditorGUILayout.PropertyField(modifsListProp, includeChildren: true);

        DrawAddModificationButtons(modifsListProp, null);
    }

    private void DrawFactionSection(string header, Faction faction, SerializedProperty factionModifsProp, SerializedProperty factionStaticSlugsProp)
    {
        EditorGUILayout.LabelField(header, BigBoldLabelStyle);

        // list of faction specific modifications
        EditorGUILayout.PropertyField(factionModifsProp, includeChildren: true);

        DrawAddModificationButtons(factionModifsProp, faction);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Static Modifiers (by slug)", EditorStyles.miniBoldLabel);

        DrawStaticSlugList(factionModifsProp, factionStaticSlugsProp);
    }

    private void DrawAddModificationButtons(SerializedProperty modifsListProp, Faction? faction)
    {
        EditorGUILayout.LabelField("Add Modification", EditorStyles.boldLabel);

        // --- General categories ---

        // Towers
        EditorGUILayout.LabelField("Towers", EditorStyles.miniBoldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Tower Modifier"))
                Add(modifsListProp, new TowerModifier());

            if (GUILayout.Button("Unlock Tower Type"))
                Add(modifsListProp, new UnlockTowerTypeModifier());
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Unlock Tower Ability"))
                Add(modifsListProp, new UnlockTowerAbilityModifier());

            if (GUILayout.Button("Unlock Tower Ability (Multiple)"))
                Add(modifsListProp, new UnlockTowerAbilityOnMultipleModifier());

            if (GUILayout.Button("Unlock Tower Upgrade"))
                Add(modifsListProp, new UnlockTowerUpgradeModifier());
        }

        EditorGUILayout.Space(4);

        // Enemies
        EditorGUILayout.LabelField("Enemies", EditorStyles.miniBoldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Enemy Modifier"))
                Add(modifsListProp, new EnemyModifier());

            if (GUILayout.Button("Enemy Ability Unlock"))
                Add(modifsListProp, new EnemyAbilityUnlock());
        }

        EditorGUILayout.Space(4);

        // Economy
        EditorGUILayout.LabelField("Economy", EditorStyles.miniBoldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Economy Modifier"))
                Add(modifsListProp, new EconomyModifier());
        }

        EditorGUILayout.Space(4);

        // Abilities
        EditorGUILayout.LabelField("Abilities", EditorStyles.miniBoldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Ability Unlock"))
                Add(modifsListProp, new AbilityUnlock());

            if (GUILayout.Button("Ability add usages"))
                Add(modifsListProp, new AbilityAddUsages());

            if (GUILayout.Button("Ability Modifier Unlock"))
                Add(modifsListProp, new AbilityModifierUnlock());
        }

        EditorGUILayout.Space(4);

        // Base
        EditorGUILayout.LabelField("Base", EditorStyles.miniBoldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Base Unlock"))
                Add(modifsListProp, new BaseUnlock());
        }

        EditorGUILayout.Space(8);

        // --- Faction-specific: Overpressure Collective ---

        if (faction == Faction.OverpressureCollective)
        {
            EditorGUILayout.LabelField(
                "Overpressure Collective – Special",
                EditorStyles.miniBoldLabel
            );

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Double-Edged Economy Modifier"))
                    Add(modifsListProp, new EconomyDoubleEdgedMofifier());

                if (GUILayout.Button("Ability: No CD, 100 Gears"))
                    Add(modifsListProp, new AbilityNoCooldownCost100Gears());

                if (GUILayout.Button("Stim Mode Modifier"))
                    Add(modifsListProp, new StimModeModifier());
            }
        }
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

        return s.Replace(' ', '_').Replace("'", "");
    }
}
#endif