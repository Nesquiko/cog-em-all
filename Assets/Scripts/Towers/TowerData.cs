using UnityEngine;

public class TowerData
{
    public string displayName;
    public string description;
    public float damage;
    public float range;
    public float fireRate;
    public float critChance;
    public float critMultiplier;
    public int cost;
    public int sellPrice;

    public TowerData(string displayName, string description, float damage, float range, float fireRate, float critChance, float critMultiplier, int cost, int sellPrice)
    {
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
