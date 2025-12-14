using UnityEngine;

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

    public override void RebuildDisplayStats()
    {
        displayStats = new DisplayStat[]
        {
            new() { label = "Shell damage", value = shellDamage.ToString("0.##") },
            new() { label = "Splash radius", value = Meters(shellSplashRadius) },
            new() { label = "Fire rate", value = PerSecond(fireRate) },
            new() { label = "Min range", value = Meters(minRange) },
            new() { label = "Max range", value = Meters(maxRange) },
            new() { label = "Crit chance", value = Percent(critChance) },
            new() { label = "Crit multiplier", value = Mult(critMultiplier) },
        };
    }

#if UNITY_EDITOR
    private void OnValidate() => RebuildDisplayStats();
#endif
}
