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

    public override void RebuildDisplayStats()
    {
        displayStats = new DisplayStat[]
        {
            new() { label = "Beam damage", value = beamDamage.ToString("0.##") },
            new() { label = "Chain radius", value = Meters(beamChainRadius) },
            new() { label = "Max chains", value = beamMaxChains.ToString() },
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
