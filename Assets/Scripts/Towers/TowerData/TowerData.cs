using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class TowerDataBase : ScriptableObject
{
    [SerializeField, Min(1)] protected int level = 1;
    [SerializeField] protected int cost;
    [SerializeField] protected int sellPrice;

    public virtual int Level => level;
    public virtual int Cost => cost;
    public virtual int SellPrice => sellPrice;
}


[Serializable]
public class TowerData<T> where T : TowerDataBase
{
    [SerializeField] private TowerTypes type;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;
    [SerializeField] T[] perLevelStats;

    public TowerTypes TowerType => type;
    public string DisplayName => displayName;
    public string Description => description;
    public IReadOnlyList<T> PerLevelStats => perLevelStats;

    public TowerData(TowerTypes type, string displayName, string description, T[] perLevelStats)
    {
        this.type = type;
        this.displayName = displayName;
        this.description = description;
        this.perLevelStats = perLevelStats;
    }

    public T GetDataForLevel(int level)
    {
        if (perLevelStats == null) return null;
        foreach (var d in perLevelStats)
            if (d != null && d.Level == level)
                return d;
        return null;
    }

    public bool CanUpgrade(int currentLevel)
    {
        if (perLevelStats == null) return false;
        foreach (var d in perLevelStats)
            if (d != null && d.Level == currentLevel + 1)
                return true;
        return false;
    }
}