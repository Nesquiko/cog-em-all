using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public struct EconomyMods
{
    public readonly float passiveGearsAmount;
    public readonly float passiveGearsTick;
    public readonly float towerUpgradeCostRatio;

    public EconomyMods(float passiveGearsAmount, float passiveGearsTick, float towerUpgradeCostRatio)
    {
        this.passiveGearsAmount = passiveGearsAmount;
        this.passiveGearsTick = passiveGearsTick;
        this.towerUpgradeCostRatio = towerUpgradeCostRatio;
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
    public readonly Func<ITower, float, float> CalculateTowerCritChance;
    public readonly Func<ITower, float, float> CalculateTowerFireRate;
    public readonly Func<FlamethrowerTower, float, float> CalculateFlamethrowerFireDuration;
    public readonly Func<ITower, float, float> CalculateDOTDuration;

    public TowerMods(
        Func<ITower, float, float> towerProjectileDamageCalculation,
        Func<ITower, float, float> towerCritChanceCalculation,
        Func<ITower, float, float> towerFireRateCalculation,
        Func<FlamethrowerTower, float, float> flamethrowerFireDuration,
        Func<ITower, float, float> dotDurationPipeline
    )
    {
        CalculateTowerProjectileDamage = towerProjectileDamageCalculation;
        CalculateTowerCritChance = towerCritChanceCalculation;
        CalculateTowerFireRate = towerFireRateCalculation;
        CalculateFlamethrowerFireDuration = flamethrowerFireDuration;
        CalculateDOTDuration = dotDurationPipeline;
    }
}

public static class ModifiersCalculator
{
    public static TowerMods CalculateTowerMods(List<Modifier> modifiers, Func<int> ActiveTowersCount)
    {
        var towerDamagePipeline = new List<Func<ITower, float, float>>();
        var towerCritChancePipeline = new List<Func<ITower, float, float>>();
        var towerFireRatePipeline = new List<Func<ITower, float, float>>();
        var towerDOTDurationPipeline = new List<Func<ITower, float, float>>();

        var flamethrowerFireDurationPipeline = new List<Func<FlamethrowerTower, float, float>>();


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
                        if (towerMod.changeType == ChangeType.PerPlacedTowerAddPercentage)
                        {
                            return baseCritChance + ActiveTowersCount() * towerMod.change;
                        }

                        return ApplyChangeType(towerMod.changeType, towerMod.change, baseCritChance, towerMod.currentRanks);
                    });
                    break;
                case TowerAttribute.FireRate:
                    towerFireRatePipeline.Add((tower, baseFireRate) =>
                    {
                        if (!TowerModifier.AppliesTo(towerMod, tower.TowerType())) return baseFireRate;
                        return ApplyChangeType(towerMod.changeType, towerMod.change, baseFireRate, towerMod.currentRanks);
                    });
                    break;
                case TowerAttribute.FireTime:
                    flamethrowerFireDurationPipeline.Add((flamethrower, baseFireDuration) =>
                    {
                        if (!TowerModifier.AppliesTo(towerMod, TowerTypes.Flamethrower)) return baseFireDuration;
                        return ApplyChangeType(towerMod.changeType, towerMod.change, baseFireDuration, towerMod.currentRanks);
                    });
                    break;
                case TowerAttribute.DotDuration:
                    towerDOTDurationPipeline.Add((tower, dotDuration) =>
                    {
                        if (!TowerModifier.AppliesTo(towerMod, tower.TowerType())) return dotDuration;
                        return ApplyChangeType(towerMod.changeType, towerMod.change, dotDuration, towerMod.currentRanks);
                    });
                    break;

                case TowerAttribute.ChainLength:
                case TowerAttribute.MaxAppliedStacks:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(towerMod.modifiedAttribute), towerMod.modifiedAttribute, "Unsupported modified attritbute.");
            }
        }

        Func<ITower, float, float> baseDamagePipeline = Compose(towerDamagePipeline);
        Func<ITower, float, float> critChancePipeline = Compose(towerCritChancePipeline);
        Func<ITower, float, float> fireRatePipeline = Compose(towerFireRatePipeline);
        Func<FlamethrowerTower, float, float> flamethrowerFireDuration = Compose(flamethrowerFireDurationPipeline);
        Func<ITower, float, float> dotDurationPipeline = Compose(towerDOTDurationPipeline);

        return new TowerMods(baseDamagePipeline, critChancePipeline, fireRatePipeline, flamethrowerFireDuration, dotDurationPipeline);
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

    public static void ModifyGatlin(GatlingTower gatling, List<Modifier> modifiers)
    {
        var additionalRendingStacks = 0;
        foreach (var m in modifiers)
        {
            if (m is not TowerModifier towerMod) continue;
            else if (towerMod.modifiedAttribute != TowerAttribute.MaxAppliedStacks) continue;

            additionalRendingStacks += towerMod.currentRanks * (int)towerMod.change;
        }

        gatling.SetMaxRendingStacks(gatling.MaxArmorRendingStacks + additionalRendingStacks);
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
        float towerUpgradeCostRatio = 1f;

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
                case EconomyAttributes.TowersUpgradeDiscount:
                    towerUpgradeCostRatio = ApplyChangeType(ecoMod.changeType, ecoMod.change, towerUpgradeCostRatio, 1);
                    break;

            }
        }

        return new EconomyMods(
            passiveGearsAmount: passiveAmount,
            passiveGearsTick: passiveTick,
            towerUpgradeCostRatio: towerUpgradeCostRatio
        );
    }

    public static Dictionary<SkillTypes, int> UsagePerAbility(List<Modifier> modifiers)
    {
        Dictionary<SkillTypes, int> usages = new();
        foreach (var m in modifiers)
        {
            if (m is AbilityUnlock abilityUnlock)
            {
                usages[abilityUnlock.toUnlock] = AbilityUnlock.OnUnlockUsages;
            }
            else if (m is BaseUnlock baseUnlock)
            {

                switch (baseUnlock.unlocks)
                {
                    case BaseUnlocks.AirShipAirStrike:
                        usages[SkillTypes.AirshipAirstrike] = 1;
                        break;
                    case BaseUnlocks.AirShipFreeze:
                        usages[SkillTypes.AirshipFreezeZone] = 1;
                        break;
                    case BaseUnlocks.AirShipDisableEnemyAbilitiesZone:
                        usages[SkillTypes.AirshipDisableZone] = 1;
                        break;
                }
            }
        }

        foreach (var m in modifiers)
        {
            if (m is not AbilityAddUsages addUsages) continue;

            if (!usages.ContainsKey(addUsages.addTo))
            {
                Debug.LogWarning($"trying to add usages not unlocked ability ${addUsages.addTo}");
                continue;
            }
            usages[addUsages.addTo] += addUsages.CurrentRanks() * addUsages.numOfUsages;
        }

        return usages;
    }


    public static HashSet<FactionSpecificSkill> GetFactionSpecificSkills(Dictionary<SkillTypes, int> usages)
    {
        HashSet<FactionSpecificSkill> result = new();
        if (usages.ContainsKey(SkillTypes.AirshipAirstrike))
            result.Add(FactionSpecificSkill.AirshipAirstrike);
        if (usages.ContainsKey(SkillTypes.AirshipFreezeZone))
            result.Add(FactionSpecificSkill.AirshipFreezeZone);
        if (usages.ContainsKey(SkillTypes.AirshipDisableZone))
            result.Add(FactionSpecificSkill.AirshipDisableZone);
        if (usages.ContainsKey(SkillTypes.MarkEnemy))
            result.Add(FactionSpecificSkill.MarkEnemy);
        if (usages.ContainsKey(SkillTypes.SuddenDeath))
            result.Add(FactionSpecificSkill.SuddenDeath);
        return result;
    }


    public static Dictionary<TowerTypes, int> UnlockedTowerLevels(List<Modifier> modifiers)
    {
        Dictionary<TowerTypes, int> unlockedTowerLevels = new();
        // Gatling is enabled by default
        unlockedTowerLevels[TowerTypes.Gatling] = 1;


        foreach (var m in modifiers)
        {
            if (m is not UnlockTowerTypeModifier unlock) continue;
            unlockedTowerLevels[unlock.toUnlock] = 1;
        }

        foreach (var m in modifiers)
        {
            if (m is not UnlockTowerUpgradeModifier levelUnlock) continue;
            if (!unlockedTowerLevels.ContainsKey(levelUnlock.applyTo))
            {
                Debug.LogWarning($"trying to allow level {levelUnlock.allowLevel} on locked tower ${levelUnlock.applyTo}");
                continue;
            }
            unlockedTowerLevels[levelUnlock.applyTo] = levelUnlock.allowLevel;
        }

        return unlockedTowerLevels;
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

    public static Func<TCtx, TValue, TValue> Compose<TCtx, TValue>(
       IReadOnlyList<Func<TCtx, TValue, TValue>> steps
   )
    {
        if (steps == null || steps.Count == 0)
            return static (_, x) => x;

        return (ctx, start) =>
        {
            var acc = start;
            for (var i = 0; i < steps.Count; i++)
                acc = steps[i](ctx, acc);
            return acc;
        };
    }

    public static bool IsGainRangeOnHillActive(List<Modifier> modifiers)
    {
        foreach (var m in modifiers)
        {
            if (m is UnlockTowerAbilityModifier unlock && unlock.unlock == TowerUnlocks.OnHillRangeIncrease) return true;
        }
        return false;
    }
}
