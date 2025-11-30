using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "TowerOverlayCatalog", menuName = "Scriptable Objects/Tower Overlay Catalog")]
public class TowerOverlayCatalog : ScriptableObject
{
    [SerializeField] private GameObject[] brassArmyTowerOverlays;
    [SerializeField] private GameObject[] valveboundSeraphsTowerOverlays;
    [SerializeField] private GameObject[] overpressureCollectiveTowerOverlays;

    private readonly Dictionary<(Faction, TowerTypes), GameObject> catalog = new();

    private void OnEnable()
    {
        RebuildCatalog();
    }

    private void OnValidate()
    {
        RebuildCatalog();
    }

    private void RebuildCatalog()
    {
        catalog.Clear();

        Register(Faction.TheBrassArmy, brassArmyTowerOverlays);
        Register(Faction.TheValveboundSeraphs, valveboundSeraphsTowerOverlays);
        Register(Faction.OverpressureCollective, overpressureCollectiveTowerOverlays);
    }

    private void Register(Faction faction, GameObject[] source)
    {
        if (source == null) return;

        int towerTypeCount = Enum.GetValues(typeof(TowerTypes)).Length;
        for (int i = 0; i < source.Length; i++)
        {
            if (i >= towerTypeCount) break;

            var prefab = source[i];
            var type = (TowerTypes)i;
            if (prefab != null)
                catalog[(faction, type)] = prefab;
        }
    }

    public GameObject FromFactionAndTowerType(Faction faction, TowerTypes towerType)
    {
        if (catalog.TryGetValue((faction, towerType), out var prefab)) 
            return prefab;
        Debug.LogWarning($"No overlay found for {faction} {towerType}");
        return null;
    }
}
