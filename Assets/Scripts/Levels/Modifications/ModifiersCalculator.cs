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

        // var teslaAdditionalChains = new List<Func<TeslaTower, int, int>>();

        foreach (var m in modifiers)
        {
            if (m is not TowerModifier towerMod) continue;

            switch (towerMod.modifiedAttribute)
            {
                case TowerAttribute.Damage:
                    towerDamagePipeline.Add((tower, baseDmg) =>
                    {
                        if (!TowerModifier.AppliesTo(towerMod, tower.TowerType())) return baseDmg;
                        return ApplyChangeType(towerMod.changeType, towerMod.change, baseDmg, towerMod.currentRanks);
                    });
                    break;
                case TowerAttribute.CritChange:
                    towerCritChancePipeline.Add((tower, baseCritChance) =>
                    {
                        if (!TowerModifier.AppliesTo(towerMod, tower.TowerType())) return baseCritChance;
                        return ApplyChangeType(towerMod.changeType, towerMod.change, baseCritChance, towerMod.currentRanks);
                    });
                    break;
                case TowerAttribute.ChainLength:
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

    public static void ModifyTesla(TeslaTower tesla, List<Modifier> modifiers)
    {
        var additionalChains = 0;
        foreach (var m in modifiers)
        {
            if (m is not TowerModifier towerMod) continue;
            else if (towerMod.modifiedAttribute != TowerAttribute.ChainLength) continue;

            additionalChains += towerMod.currentRanks * (int)towerMod.change;
        }

        tesla.SetAdditionalChainReach(additionalChains);
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
                enemyRewardPipeline.Add((reward) =>
                {
                    return Mathf.FloorToInt(ApplyChangeType(ecoMod.changeType, ecoMod.change, reward, 1));
                });
                continue;
            }

            if (m is not EnemyModifier enemyMod) continue;

            switch (enemyMod.modifiedAttribute)
            {
                case EnemyAttributes.MovementSpeed:
                    enemySpeedPipeline.Add((enemy, speed) =>
                    {
                        if (!EnemyModifier.AppliesTo(enemyMod, enemy.Type)) return speed;
                        return ApplyChangeType(enemyMod.changeType, enemyMod.change, speed, 1);
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
                    passiveAmount = ApplyChangeType(ecoMod.changeType, ecoMod.change, passiveAmount, 1);
                    break;
                case EconomyAttributes.PassiveGearsTick:
                    passiveTick = ApplyChangeType(ecoMod.changeType, ecoMod.change, passiveTick, 1);
                    break;
            }
        }

        return new EconomyMods(
            passiveGearsAmount: passiveAmount,
            passiveGearsTick: passiveTick
        );
    }

    private static float ApplyChangeType(ChangeType changeType, float change, float value, int ranks)
    {
        return changeType switch
        {
            ChangeType.Add => value + ranks * change,
            ChangeType.Mult => value * Mathf.Pow(change, ranks),
            ChangeType.Replace => change,
            _ => throw new ArgumentException($"Unsupported changeType '{nameof(changeType)}' in ApplyChangeType."),
        };
    }

}
