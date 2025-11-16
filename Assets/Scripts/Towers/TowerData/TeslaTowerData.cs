using UnityEngine;


[CreateAssetMenu(fileName = "TeslaTowerData", menuName = "Towers/Tesla Tower Data")]
public class TeslaTowerData : TowerDataBase
{
    [Header("Beam Stats")]
    public float beamDamage;
    public float beamSpeed;
    public float beamChainRadius;
    public int beamMaxChains;
    public float beamStayTimeOnHit;

    [Header("Tesla Tower Stats")]
    public float fireRate;
    public float range;
    [Range(0f, 1f)] public float critChance;
    public float critMultiplier;
}
