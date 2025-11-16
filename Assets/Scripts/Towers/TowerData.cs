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

[CreateAssetMenu(fileName = "GatlingTowerData", menuName = "Towers/Gatling Tower Data")]
public class GatlingTowerData : TowerDataBase
{
    [Header("Bullet Stats")]
    public float bulletDamage;
    public float bulletSpeed;
    public float bulletLifetime;

    [Header("Gatling Tower Stats")]
    public float fireRate;
    public float range;
    [Range(0f, 1f)] public float critChance;
    public float critMultiplier;
}

[CreateAssetMenu(fileName = "TeslaTowerData", menuName = "Towers/Tesla Tower Data")]
public class TeslaTowerData : TowerDataBase
{
    [Header("Beam Stats")]
    public float beamDamage;
    public float beamSpeed;
    public float beamChainRadius;
    public int beamMaxChains;
    public float beamStayTimeOnHit;

    [Header("Tesla Tower Stats")]
    public float fireRate;
    public float range;
    [Range(0f, 1f)] public float critChance;
    public float critMultiplier;
}

[CreateAssetMenu(fileName = "MortarTowerData", menuName = "Towers/Mortar Tower Data")]
public class MortarTowerData : TowerDataBase
{
    [Header("Shell Stats")]
    public float shellDamage;
    public float shellSplashRadius;
    public float shellLifetime;

    [Header("Mortar Tower Stats")]
    public float fireRate;
    public float minRange;
    public float maxRange;
    [Range(0f, 1f)] public float critChance;
    public float critMultiplier;
    public float rotationSpeed;
    public float launchSpeed;
    public float arcHeight;
}

[CreateAssetMenu(fileName = "FlamethrowerTowerData", menuName = "Towers/Flamethrower Tower Data")]
public class FlamethrowerTowerData : TowerDataBase
{
    [Header("Flame Stats")]
    public float flameDamagePerPulse;
    public float flamePulseInterval;
    public float flameDuration;

    [Header("Flamethrower Tower Stats")]
    public float range;
    public float flameAngle;
    public float cooldownDuration;
    [Range(0f, 1f)] public float critChance;
    public float critMultiplier;
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