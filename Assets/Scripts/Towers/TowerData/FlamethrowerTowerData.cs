using UnityEngine;

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

    public override void RebuildDisplayStats()
    {
        displayStats = new DisplayStat[]
        {
            new() { label = "Damage per pulse", value = flameDamagePerPulse.ToString("0.##") },
            new() { label = "Pulse interval", value = Seconds(flamePulseInterval) },
            new() { label = "Flame duration", value = Seconds(flameDuration) },
            new() { label = "Range", value = Meters(range) },
            new() { label = "Cooldown duration", value = Seconds(cooldownDuration) },
            new() { label = "Crit chance", value = Percent(critChance) },
            new() { label = "Crit multiplier", value = Mult(critMultiplier) },
        };
    }

#if UNITY_EDITOR
    private void OnValidate() => RebuildDisplayStats();
#endif
}