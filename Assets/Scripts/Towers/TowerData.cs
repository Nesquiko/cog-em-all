using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
    public float beamMaxChains;
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
    [SerializeField] List<T> perLevelStats = new();

    public TowerTypes TowerType => type;
    public string DisplayName => displayName;
    public string Description => description;
    public IReadOnlyList<T> PerLevelStats => perLevelStats;

    public TowerData(TowerTypes type, string displayName, string description, List<T> perLevelStats)
    {
        this.type = type;
        this.displayName = displayName;
        this.description = description;
        this.perLevelStats = perLevelStats;
    }

    public T GetDataForLevel(int level)
    {
        return perLevelStats.Find(d => d.Level == level);
    }

    public bool CanUpgrade(int currentLevel)
    {
        return perLevelStats.Exists(d => d.Level == currentLevel + 1);
    }
}