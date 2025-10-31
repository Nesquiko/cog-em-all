using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


[CreateAssetMenu(fileName = "TowerDataCatalog", menuName = "Scriptable Objects/TowerDataCatalog")]
public class TowerDataCatalog : ScriptableObject
{
    [SerializeField, Tooltip("List of towers available in the game.")]
    private List<TowerData> towers = new();

    [SerializeField, Tooltip("List of levels available for each tower.")]
    private List<int> towerLevels = new();

    private readonly Dictionary<TowerTypes, TowerData> catalog = new();

    public int TowerCount => towers.Count;

    public int TowerLevelsCount => towerLevels.Count;

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

        foreach (var t in towers)
        {
            if (catalog.ContainsKey(t.type))
            {
                Debug.LogWarning($"duplicate TowerType detected: {t.type}");
                continue;
            }

            catalog[t.type] = t;
        }
    }

    public TowerData FromIndex(int i)
    {
        Assert.IsTrue(System.Enum.IsDefined(typeof(TowerTypes), i), $"invalid TowerType value: {i}");
        return FromType((TowerTypes)i);
    }

    public TowerData FromType(TowerTypes type)
    {
        Assert.IsNotNull(catalog);
        Assert.IsTrue(catalog.ContainsKey(type));
        var towerData = catalog[type];
        Assert.IsNotNull(towerData);
        return towerData;
    }

    public (HashSet<TowerTypes>, HashSet<TowerTypes>) AdjustTowers(int gears)
    {
        HashSet<TowerTypes> toEnable = new();
        HashSet<TowerTypes> toDisable = new();

        foreach (TowerData tower in towers)
        {
            if (gears >= tower.cost)
            {
                toEnable.Add(tower.type);
            }
            else
            {
                toDisable.Add(tower.type);
            }
        }

        return (toEnable, toDisable);
    }
}