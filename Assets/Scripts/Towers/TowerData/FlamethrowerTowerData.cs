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
}