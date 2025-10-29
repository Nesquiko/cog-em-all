using System;
using UnityEngine;

[Serializable]
public class EnemyStatusEffect
{
    public EffectType type;
    public float duration;
    public float tickDamage;
    public float tickInterval = 1f;
    public float speedMultiplier = 1f;
}

public enum EffectType
{
    Burning,
    Bleeding,
    Slowed
}
