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

    public override void RebuildDisplayStats()
    {
        displayStats = new DisplayStat[]
        {
            new() { label = "Bullet damage", value = bulletDamage.ToString("0.##") },
            new() { label = "Fire rate", value = PerSecond(fireRate) },
            new() { label = "Range", value = Meters(range) },
            new() { label = "Crit chance", value = Percent(critChance) },
            new() { label = "Crit multiplier", value = Mult(critMultiplier) },
        };
    }

#if UNITY_EDITOR
    private void OnValidate() => RebuildDisplayStats();
#endif
}