using System;
using System.Collections.Generic;
using UnityEngine;

// TowerUpgradeManager should currently hold a list of TowerUpgradeData
// Later, when converted to tower-specific, we probably don't even need a dictionary for this
// This could be converted to a catalog scriptable object, which would be easier to pass
// (But right now it is a working version)

// Each tower starts at currentLevel = 1, upgrades are leveled from 2
// (meaning the upgrade will turn the tower to this level)

public class TowerUpgradeManager : MonoBehaviour
{
    [SerializeField] private List<TowerUpgradeData> allUpgradeData;

    private Dictionary<TowerTypes, List<TowerUpgradeData>> upgradeTree;

    public event Action<int> OnUpgradeTower;

    private void Awake()
    {
        upgradeTree = new();

        foreach (var data in allUpgradeData)
        {
            if (!upgradeTree.ContainsKey(data.towerType))
                upgradeTree[data.towerType] = new();

            upgradeTree[data.towerType].Add(data);
        }

        foreach (var list in upgradeTree.Values)
            list.Sort((a, b) => a.level.CompareTo(b.level));
    }

    public bool RequestUpgrade(ITowerUpgradeable tower)
    {
        if (!upgradeTree.TryGetValue(tower.TowerType(), out var upgrades))
            return false;

        int nextLevel = tower.CurrentLevel() + 1;

        var nextUpgrade = upgrades.Find(u => u.level == nextLevel);
        if (nextUpgrade == null)
            return false;

        tower.ApplyUpgrade(nextUpgrade);
        OnUpgradeTower.Invoke(nextUpgrade.cost);
        return true;
    }

    public bool CanUpgrade(TowerTypes type, int currentLevel)
    {
        if (!upgradeTree.TryGetValue(type, out var upgrades) || upgrades.Count == 0)
            return false;

        foreach (var data in upgrades)
        {
            if (data.level == currentLevel + 1)
                return true;
        }

        return false;
    }
}