using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;


[CreateAssetMenu(fileName = "TowerDataCatalog", menuName = "Scriptable Objects/Tower Data Catalog")]
public class TowerDataCatalog : ScriptableObject
{
    [SerializeField, Tooltip("All towers and their per-level data.")]
    private TowerData<TowerDataBase>[] towers;

    private readonly Dictionary<TowerTypes, TowerData<TowerDataBase>> catalog = new();

    public Action<int> OnUpgradeTower;

    public Dictionary<TowerTypes, TowerData<TowerDataBase>> Catalog => catalog;
    public int TowersCount => towers.Count();
    public int TowerLevelsCount => 3;

    private void OnEnable() => RebuildCatalog();

    private void OnValidate() => RebuildCatalog();

    private void RebuildCatalog()
    {
        catalog.Clear();
        foreach (var towerData in towers)
        {
            if (towerData == null)
                continue;

            if (catalog.ContainsKey(towerData.TowerType))
            {
                Debug.LogWarning($"Duplicate TowerType {towerData.TowerType} detected in catalog, ignoring duplicate.");
                continue;
            }

            catalog.Add(towerData.TowerType, towerData);
        }
    }

    public TowerData<TowerDataBase> FromIndex(int index)
    {
        Assert.IsTrue(index >= 0 && index < towers.Count(), $"Invalid tower index: {index}");
        var towerData = towers[index];
        Assert.IsNotNull(towerData, $"Tower data missing at index: {index}");
        return towerData;
    }

    public TData FromIndexAndLevel<TData>(int index, int level) where TData : TowerDataBase
    {
        return FromIndexAndLevel(index, level) as TData;
    }

    public TowerDataBase FromIndexAndLevel(int index, int level)
    {
        Assert.IsTrue(index >= 0 && index < towers.Count(), $"Invalid tower index: {index}");
        var towerData = towers[index];
        Assert.IsNotNull(towerData, $"Tower data missing at index: {index}");
        var levelData = towerData.GetDataForLevel(level);
        return levelData;
    }

    public TowerData<TowerDataBase> FromType(TowerTypes type)
    {
        Assert.IsTrue(catalog.ContainsKey(type), $"Type not defined in catalog: {type}");
        var towerData = catalog[type];
        Assert.IsNotNull(towerData, $"No tower data found for type: {type}");
        return towerData;
    }

    public TowerDataBase FromTypeAndLevel(TowerTypes type, int level)
    {
        Assert.IsTrue(catalog.ContainsKey(type), $"Type not defined in catalog: ${type}");
        var towerData = catalog[type];
        Assert.IsNotNull(towerData, $"No tower level data found for type: {type}");
        var levelData = towerData.GetDataForLevel(level);
        return levelData;
    }

    public TData FromTypeAndLevel<TData>(TowerTypes type, int level) where TData : TowerDataBase
    {
        return FromTypeAndLevel(type, level) as TData;
    }

    public bool CanUpgrade(TowerTypes type, int currentLevel, int maxAllowedLevel)
    {
        if (currentLevel + 1 > maxAllowedLevel) return false;
        if (!catalog.TryGetValue(type, out var data)) return false;
        return data.CanUpgrade(currentLevel);
    }

    public int GetMaxLevel(TowerTypes type)
    {
        Assert.IsTrue(catalog.ContainsKey(type), $"Type not defined in catalog: ${type}");
        var towerData = catalog[type];
        return towerData.PerLevelStats.Count;
    }

    public bool RequestUpgrade(ITower tower)
    {
        Assert.IsNotNull(tower, "Invalid tower reference.");

        TowerTypes type = tower.TowerType();
        int currentLevel = tower.CurrentLevel();
        int maxAllowedLevel = tower.MaxAllowedLevel();

        if (!CanUpgrade(type, currentLevel, maxAllowedLevel)) return false;

        int nextLevel = currentLevel + 1;
        TowerDataBase nextLevelData = FromTypeAndLevel(type, nextLevel);
        Assert.IsNotNull(nextLevelData, $"Upgrade data missing for {type} level {nextLevel}.");

        tower.ApplyUpgrade(nextLevelData);

        OnUpgradeTower?.Invoke(nextLevelData.Cost);

        return true;
    }

    public (HashSet<TowerTypes>, HashSet<TowerTypes>) AdjustTowers(int gears)
    {
        HashSet<TowerTypes> toEnable = new();
        HashSet<TowerTypes> toDisable = new();

        Assert.IsNotNull(towers);

        foreach (var tower in towers)
        {
            Assert.IsNotNull(tower);
            var baseData = tower.GetDataForLevel(1);
            Assert.IsNotNull(baseData);

            if (gears >= baseData.Cost)
                toEnable.Add(tower.TowerType);
            else
                toDisable.Add(tower.TowerType);
        }

        return (toEnable, toDisable);
    }
}