using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public struct EconomyMods
{

    public readonly Func<int, int> CalculateEnemyReward;

    public readonly float passiveGearsAmount;
    public readonly float passiveGearsTick;
    public readonly float towerUpgradeCostRatio;
    public readonly float nexusOnHitSpendGears;

    public EconomyMods(
        Func<int, int> enemyRewardCalculation,
        float passiveGearsAmount,
        float passiveGearsTick,
        float towerUpgradeCostRatio,
        float nexusOnHitSpendGears
        )
    {
        this.CalculateEnemyReward = enemyRewardCalculation;
        this.passiveGearsAmount = passiveGearsAmount;
        this.passiveGearsTick = passiveGearsTick;
        this.towerUpgradeCostRatio = towerUpgradeCostRatio;
        this.nexusOnHitSpendGears = nexusOnHitSpendGears;
    }
}

public struct EnemyMods
{
    public readonly Func<IEnemy, float, float> CalculateEnemyMovementSpeed;

    public EnemyMods(Func<IEnemy, float, float> enemySpeedCalculation)
    {
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
    public readonly Func<ITower, float, float> CalculateTowerRange;

    public TowerMods(
        Func<ITower, float, float> towerProjectileDamageCalculation,
        Func<ITower, float, float> towerCritChanceCalculation,
        Func<ITower, float, float> towerFireRateCalculation,
        Func<FlamethrowerTower, float, float> flamethrowerFireDuration,
        Func<ITower, float, float> dotDurationPipeline,
        Func<ITower, float, float> rangeCalculation
    )
    {
        CalculateTowerProjectileDamage = towerProjectileDamageCalculation;
        CalculateTowerCritChance = towerCritChanceCalculation;
        CalculateTowerFireRate = towerFireRateCalculation;
        CalculateFlamethrowerFireDuration = flamethrowerFireDuration;
        CalculateDOTDuration = dotDurationPipeline;
        CalculateTowerRange = rangeCalculation;
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
        var towerRangePipeline = new List<Func<ITower, float, float>>();

        var flamethrowerFireDurationPipeline = new List<Func<FlamethrowerTower, float, float>>();


        foreach (var m in modifiers)
        {
            if (m is not TowerModifier towerMod) continue;

            switch (towerMod.modifiedAttribute)
            {
                case TowerAttribute.Range:
                    towerRangePipeline.Add((tower, range) =>
                    {
                        if (!TowerModifier.AppliesTo(towerMod, tower.TowerType())) return range;
                        return ApplyChangeType(towerMod.changeType, towerMod.change, range, towerMod.currentRanks);
                    });
                    break;
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
        Func<ITower, float, float> rangePipeline = Compose(towerRangePipeline);

        return new TowerMods(baseDamagePipeline, critChancePipeline, fireRatePipeline, flamethrowerFireDuration, dotDurationPipeline, rangePipeline);
    }

    public static void ModifyTesla(TeslaTower tesla, List<Modifier> modifiers)
    {
        int additionalChains = 0;
        bool manualModeEnabled = false;
        bool stunFirstEnemyEnabled = false;
        bool disableBuffsOnHitEnabled = false;
        foreach (var m in modifiers)
        {
            switch (m)
            {
                case TowerModifier towerMod:
                    if (towerMod.modifiedAttribute != TowerAttribute.ChainLength) break;

                    additionalChains += towerMod.currentRanks * (int)towerMod.change;
                    break;
                case UnlockTowerAbilityModifier abilityUnlock:
                    manualModeEnabled = manualModeEnabled || abilityUnlock.unlock == TowerUnlocks.ManualMode;
                    stunFirstEnemyEnabled = stunFirstEnemyEnabled || abilityUnlock.unlock == TowerUnlocks.OnHitStun;
                    disableBuffsOnHitEnabled = disableBuffsOnHitEnabled || abilityUnlock.unlock == TowerUnlocks.OnHitRemoveEnemyAbilities;
                    break;
            }
        }

        tesla.SetAdditionalChainReach(additionalChains);
        if (manualModeEnabled) tesla.EnableControlMode();
        if (stunFirstEnemyEnabled) tesla.EnableStunFirst();
        if (disableBuffsOnHitEnabled) tesla.EnableDisableBuffs();
    }

    public static void ModifyGatling(GatlingTower gatling, List<Modifier> modifiers)
    {
        bool isArmorRendingActive = false;
        int additionalRendingStacks = 0;
        bool manualModeEnabled = false;
        foreach (var m in modifiers)
        {
            switch (m)
            {
                case TowerModifier towerMod:
                    if (towerMod.modifiedAttribute != TowerAttribute.MaxAppliedStacks) continue;

                    additionalRendingStacks += towerMod.currentRanks * (int)towerMod.change;
                    break;

                case UnlockTowerAbilityModifier abilityUnlock:
                    isArmorRendingActive = isArmorRendingActive || abilityUnlock.unlock == TowerUnlocks.ArmorShreding;
                    manualModeEnabled = manualModeEnabled || abilityUnlock.unlock == TowerUnlocks.ManualMode;
                    break;
            }
        }

        gatling.SetRendingEnabled(isArmorRendingActive);
        gatling.SetMaxRendingStacks(gatling.MaxArmorRendingStacks + additionalRendingStacks);
        if (manualModeEnabled) gatling.EnableControlMode();
    }

    public static void ModifyMortar(MortarTower mortar, List<Modifier> modifiers)
    {
        bool isSlowOnHitEnabled = false;
        foreach (var m in modifiers)
        {
            switch (m)
            {
                case UnlockTowerAbilityModifier abilityUnlock:
                    isSlowOnHitEnabled = isSlowOnHitEnabled || abilityUnlock.unlock == TowerUnlocks.OnHitSlow;
                    break;
            }
        }

        if (isSlowOnHitEnabled) mortar.EnableSlowOnhit();
    }

    public static void ModifyDOTTower(IAppliesDOT dotTower, List<Modifier> modifiers)
    {
        bool dotEnabled = false;
        foreach (var m in modifiers)
        {
            dotEnabled = m is UnlockTowerAbilityModifier abilityUnlock && abilityUnlock.unlock == TowerUnlocks.OnHitDot;
            if (dotEnabled) break;
        }

        dotTower.SetDotEnabled(dotEnabled);
    }

    public static EnemyMods CalculateEnemyMods(List<Modifier> modifiers, Func<int> ActiveTowersCount)
    {
        var enemySpeedPipeline = new List<Func<IEnemy, float, float>>();

        foreach (var m in modifiers)
        {
            if (m is not EnemyModifier enemyMod) continue;

            switch (enemyMod.modifiedAttribute)
            {
                case EnemyAttributes.MovementSpeed:
                    enemySpeedPipeline.Add((enemy, speed) =>
                    {
                        if (!EnemyModifier.AppliesTo(enemyMod, enemy.Type)) return speed;

                        if (enemyMod.changeType == ChangeType.PerPlacedTowerAddPercentage)
                        {
                            return speed + (enemyMod.change * ActiveTowersCount() * speed);
                        }

                        return ApplyChangeType(enemyMod.changeType, enemyMod.change, speed, 1);
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(enemyMod.modifiedAttribute), enemyMod.modifiedAttribute, "Unsupported modified attritbute.");
            }
        }


        Func<IEnemy, float, float> speedPipeline = Compose(enemySpeedPipeline);

        return new EnemyMods(enemySpeedCalculation: speedPipeline);
    }

    public static EconomyMods CalculateEconomyMods(float basePassiveTickAmount, float basePassiveGearsPerTickAmount, List<Modifier> modifiers)
    {
        Assert.IsNotNull(modifiers);
        float passiveAmount = basePassiveGearsPerTickAmount;
        float passiveTick = basePassiveTickAmount;
        float towerUpgradeCostRatio = 1f;
        float nexusOnHitSpendGears = 0;
        var enemyRewardPipeline = new List<Func<int, int>>();

        foreach (var m in modifiers)
        {
            if (m is EconomyModifier ecoMod)
            {
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
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ecoMod.category), ecoMod.category, "Unsupported economy category modifier.");
                }
            }
            else if (m is EconomyDoubleEdgedMofifier doubleEdgedMofifier)
            {
                var benefit = doubleEdgedMofifier.benefit;
                switch (benefit.category)
                {
                    case EconomyAttributes.PerEnemyKillGears:
                        if (benefit.category != EconomyAttributes.PerEnemyKillGears) continue;
                        enemyRewardPipeline.Add((reward) => Mathf.FloorToInt(ApplyChangeType(benefit.changeType, benefit.change, reward)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(benefit.category), benefit.category, "Unsupported economy benefit category modifier.");
                }

                var disadvantage = doubleEdgedMofifier.disadvantage;
                switch (disadvantage.category)
                {
                    case EconomyAttributes.BaseOnHitDeduction:
                        nexusOnHitSpendGears = ApplyChangeType(disadvantage.changeType, disadvantage.change, nexusOnHitSpendGears);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(benefit.category), benefit.category, "Unsupported economy disadvantage category modifier.");
                }
            }
        }


        Func<int, int> rewardPipeline = Compose(enemyRewardPipeline);

        return new EconomyMods(
            enemyRewardCalculation: rewardPipeline,
            passiveGearsAmount: passiveAmount,
            passiveGearsTick: passiveTick,
            towerUpgradeCostRatio: towerUpgradeCostRatio,
            nexusOnHitSpendGears
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

    private static float ApplyChangeType(ChangeType changeType, float change, float value, int ranks = 1)
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

    public static Func<TValue, TValue> Compose<TValue>(
       IReadOnlyList<Func<TValue, TValue>> steps
   )
    {
        if (steps == null || steps.Count == 0)
            return static (x) => x;

        return (start) =>
        {
            var acc = start;
            for (var i = 0; i < steps.Count; i++)
                acc = steps[i](acc);
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

    public static void ModifyNexus(Nexus nexus, List<Modifier> modifiers)
    {
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i] is BaseUnlock unlock &&
                unlock.unlocks == BaseUnlocks.HealthRegen)
            {
                nexus.SetIsHealing(true);
                return;
            }
        }

        nexus.SetIsHealing(false);
    }
}
