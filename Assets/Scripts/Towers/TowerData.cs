using System;
using UnityEngine;

[Serializable]
public class TowerData
{
    public TowerTypes type;
    public string displayName;
    [TextArea] public string description;
    public float damage;
    public float range;
    public float fireRate;
    public float critChance;
    public float critMultiplier;
    public int cost;
    public int sellPrice;

    public TowerData(TowerTypes type, string displayName, string description, float damage, float range, float fireRate, float critChance, float critMultiplier, int cost, int sellPrice)
    {
        this.type = type;
        this.displayName = displayName;
        this.description = description;
        this.damage = damage;
        this.range = range;
        this.fireRate = fireRate;
        this.critChance = critChance;
        this.critMultiplier = critMultiplier;
        this.cost = cost;
        this.sellPrice = sellPrice;
    }
}