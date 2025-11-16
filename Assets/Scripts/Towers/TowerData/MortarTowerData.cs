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
}
