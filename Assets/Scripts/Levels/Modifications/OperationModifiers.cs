using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OperationModifiers", menuName = "Scriptable Objects/OperationModifiers")]
public class OperationModifiers : ScriptableObject
{
    [SerializeField] private Faction faction;
    [SerializeField] private ModifiersDatabase modifiersDatabase;
    private List<Modifier> modifiers;

#if UNITY_EDITOR
    private void OnValidate()
    {
        RefreshModifiers();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    public void SetModifiers(Faction faction)
    {
        this.faction = faction;
        RefreshModifiers();
    }

    public (Faction, List<Modifier>) ReadModifiers()
    {
        return (faction, modifiers);
    }

    private void RefreshModifiers()
    {
        if (modifiersDatabase == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name}: ModifiersDatabase is not assigned.");
#endif
            modifiers = null;
            return;
        }

        switch (faction)
        {
            case Faction.TheBrassArmy:
                modifiers = modifiersDatabase.TheBrassArmyBaseModifiers;
                break;
            case Faction.TheValveboundSeraphs:
                modifiers = modifiersDatabase.TheValveboundSeraphsBaseModifiers;
                break;
            case Faction.OverpressureCollective:
                modifiers = modifiersDatabase.OverpressureCollectiveBaseModifiers;
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(faction),
                    faction,
                    "Unsupported faction in modifiers lookup."
                );
        }
    }
}
