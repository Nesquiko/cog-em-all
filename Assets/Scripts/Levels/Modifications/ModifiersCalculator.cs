using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public struct EconomyMods
{
    public readonly float passiveGearsAmount;
    public readonly float passiveGearsTick;

    public EconomyMods(float passiveGearsAmount, float passiveGearsTick)
    {
        this.passiveGearsAmount = passiveGearsAmount;
        this.passiveGearsTick = passiveGearsTick;
    }
}

public struct EnemyMods
{
    public readonly Func<int, int> CalculateEnemyReward;
    public readonly Func<IEnemy, float, float> CalculateEnemyMovementSpeed;

    public EnemyMods(Func<int, int> enemyRewardCalculation, Func<IEnemy, float, float> enemySpeedCalculation)
    {
        CalculateEnemyReward = enemyRewardCalculation;
        CalculateEnemyMovementSpeed = enemySpeedCalculation;
    }
}

public struct TowerMods
{
    public readonly Func<ITower, float, float> CalculateTowerProjectileDamage;

    public TowerMods(Func<ITower, float, float> towerProjectileDamageCalculation)
    {
        CalculateTowerProjectileDamage = towerProjectileDamageCalculation;
    }
}

public static class ModifiersCalculator
{

    public static TowerMods CalculateTowerMods(List<Modifier> modifiers)
    {
        var towerDamagePipeline = new List<Func<ITower, float, float>>();
        var towerCritChancePipeline = new List<Func<ITower, float, float>>();

        foreach (var m in modifiers)
        {
            if (m is not TowerModifier towerMod) continue;

            switch (towerMod.modifiedAttribute)
            {
                case TowerAttribute.Damage:
                    towerDamagePipeline.Add((tower, baseDmg) =>
                    {
                        if (!TowerModifier.AppliesTo(towerMod, tower.TowerType())) return baseDmg;
                        return ApplyChangeType(towerMod.changeType, towerMod.change, baseDmg);
                    });
                    break;
                case TowerAttribute.CritChange:
                    towerCritChancePipeline.Add((tower, baseCritChance) =>
                    {
                        if (!TowerModifier.AppliesTo(towerMod, tower.TowerType())) return baseCritChance;
                        return ApplyChangeType(towerMod.changeType, towerMod.change, baseCritChance);
                    });
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(towerMod.modifiedAttribute), towerMod.modifiedAttribute, "Unsupported modified attritbute.");
            }
        }

        Func<ITower, float, float> baseDamagePipeline = (enemy, baseDmg) =>
        {
            float acc = baseDmg;
            for (int i = 0; i < towerDamagePipeline.Count; i++)
                acc = towerDamagePipeline[i](enemy, acc);
            return acc;
        };

        if (towerDamagePipeline.Count == 0)
            baseDamagePipeline = (tower, speed) => speed;

        return new TowerMods(baseDamagePipeline);

    }

    public static EnemyMods CalculateEnemyMods(List<Modifier> modifiers)
    {

        var enemyRewardPipeline = new List<Func<int, int>>();
        var enemySpeedPipeline = new List<Func<IEnemy, float, float>>();

        foreach (var m in modifiers)
        {
            if (m is EconomyModifier ecoMod)
            {
                if (ecoMod.category != EconomyAttributes.PerEnemyKillGears) continue;
                enemyRewardPipeline.Add((reward) => { return Mathf.FloorToInt(ApplyChangeType(ecoMod.changeType, ecoMod.change, reward)); });
                continue;
            }

            if (m is not EnemyModifier enemyMod) continue;

            switch (enemyMod.modifiedAttribute)
            {
                case EnemyAttributes.MovementSpeed:
                    enemySpeedPipeline.Add((enemy, speed) =>
                    {
                        if (!EnemyModifier.AppliesTo(enemyMod, enemy.Type)) return speed;
                        return ApplyChangeType(enemyMod.changeType, enemyMod.change, speed);
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(enemyMod.modifiedAttribute), enemyMod.modifiedAttribute, "Unsupported modified attritbute.");
            }
        }

        Func<int, int> rewardPipeline = reward =>
        {
            int acc = reward;
            for (int i = 0; i < enemyRewardPipeline.Count; i++)
                acc = enemyRewardPipeline[i](acc);
            return acc;
        };

        if (enemyRewardPipeline.Count == 0) rewardPipeline = reward => reward;

        Func<IEnemy, float, float> speedPipeline = (enemy, baseSpeed) =>
        {
            float acc = baseSpeed;
            for (int i = 0; i < enemySpeedPipeline.Count; i++)
                acc = enemySpeedPipeline[i](enemy, acc);
            return acc;
        };

        if (enemySpeedPipeline.Count == 0)
            speedPipeline = (enemy, speed) => speed;

        return new EnemyMods(enemyRewardCalculation: rewardPipeline, enemySpeedCalculation: speedPipeline);
    }

    public static EconomyMods CalculateEconomyMods(float basePassiveTickAmount, float basePassiveGearsPerTickAmount, List<Modifier> modifiers)
    {
        Assert.IsNotNull(modifiers);
        float passiveAmount = basePassiveGearsPerTickAmount;
        float passiveTick = basePassiveTickAmount;

        foreach (var m in modifiers)
        {
            if (m is not EconomyModifier ecoMod) continue;

            switch (ecoMod.category)
            {
                case EconomyAttributes.PassiveGearsAmount:
                    passiveAmount = ApplyChangeType(ecoMod.changeType, ecoMod.change, passiveAmount);
                    break;
                case EconomyAttributes.PassiveGearsTick:
                    passiveTick = ApplyChangeType(ecoMod.changeType, ecoMod.change, passiveTick);
                    break;
            }
        }

        return new EconomyMods(
            passiveGearsAmount: passiveAmount,
            passiveGearsTick: passiveTick
        );
    }

    public static float ApplyChangeType(ChangeType changeType, float change, float value)
    {
        switch (changeType)
        {
            case ChangeType.Add:
                return value + change;
            case ChangeType.Mult:
                return value * change;
            case ChangeType.Replace:
                return change;
            default:
                throw new ArgumentException($"Unsupported changeType '{nameof(changeType)}' in ApplyChangeType.");
        }
    }

}
