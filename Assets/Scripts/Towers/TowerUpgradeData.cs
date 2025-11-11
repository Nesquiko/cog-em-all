using UnityEngine;

// TODO: convert this to tower-specific upgrade data

[CreateAssetMenu(fileName = "TowerUpgradeData", menuName = "Scriptable Objects/Tower Upgrade Data")]
public class TowerUpgradeData : ScriptableObject
{
    public TowerTypes towerType;
    public int level;
    public int cost;
    public float damage;
    public float fireRate;
    [Range(0f, 1f)] public float critChance;
    public float critMultiplier;
    public float range;
}
