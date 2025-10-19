using UnityEngine;

public class TowerData
{
    public string displayName;
    public string description;
    public float damage;
    public float range;
    public float fireRate;
    public int cost;

    public TowerData(string displayName, string description, float damage, float range, float fireRate, int cost)
    {
        this.displayName = displayName;
        this.description = description;
        this.damage = damage;
        this.range = range;
        this.fireRate = fireRate;
        this.cost = cost;
    }
}
