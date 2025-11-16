using UnityEngine;


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