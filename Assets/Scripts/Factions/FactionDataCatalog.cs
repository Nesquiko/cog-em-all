using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "FactionDataCatalog", menuName = "Scriptable Objects/Faction Data Catalog")]
public class FactionDataCatalog : ScriptableObject
{
    [SerializeField, Tooltip("List of all factions and their data")]
    private List<FactionData> factions = new();

    private readonly Dictionary<Faction, FactionData> catalog;

    public int FactionCount => factions.Count;

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

        foreach (var f in factions)
        {
            if (catalog.ContainsKey(f.faction))
            {
                Debug.LogWarning($"duplicate Faction detected: {f.faction}");
                continue;
            }

            catalog[f.faction] = f;
        }
    }

    public FactionData FromIndex(int i)
    {
        Assert.IsTrue(System.Enum.IsDefined(typeof(Faction), i), $"invalid Faction value: {i}");
        return FromType((Faction)i);
    }

    public FactionData FromType(Faction faction)
    {
        Assert.IsNotNull(catalog);
        Assert.IsTrue(catalog.ContainsKey(faction));
        var factionData = catalog[faction];
        Assert.IsNotNull(factionData);
        return factionData;
    }
}
