using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

[RequireComponent(typeof(Spawner))]
class Orchestrator : MonoBehaviour
{
    [SerializeField] private Spawner spawner;
    private SerializableLevel level;
    private int wavesSpawned = 0;
    private readonly Dictionary<int, int> perWaveEnemies = new();
    private int enemiesLive = 0;
    private bool operationEnded = false;

    private readonly Dictionary<int, ITower> towers = new();

    [SerializeField] private TowerDataCatalog towerDataCatalog;
    [SerializeField] private SkillDataCatalog skillDataCatalog;

    [SerializeField] private Nexus nexus;
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;
    [SerializeField] private SkillPlacementSystem skillPlacementSystem;
    [SerializeField] private TowerSellManager towerSellManager;
    [SerializeField] private TowerSelectionManager towerSelectionManager;
    [SerializeField] private GearDropManager gearDropManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineBrain brain;

    [Header("UI")]
    [SerializeField] private WaveCounterInfo waveCounterInfo;
    [SerializeField] private NextWaveCountdownInfo nextWaveCountdown;
    [SerializeField] private HUDPanelUI HUDPanelUI;
    [SerializeField] private MenuPanelUI menuPanelUI;
    [SerializeField] private OperationResultUI operationResultUI;

    [Header("Player resources")]
    [SerializeField] private int passiveIncome = 10;
    [SerializeField] private float passiveTick = 5f;
    [SerializeField, Range(1f, 2f)] private float gearRewardMultiplier = 1f;

    [Header("Operation shenanigans")]
    private OperationStatistics operationStatistics;
    private float operationStartTime;

    [SerializeField] private ExperienceSystem experienceSystem;
    private SaveContextDontDestroy saveContext;

    private int gears = 0;

    public int Gears => gears;

    private TowerMods towerMods = new();
    private EconomyMods economyMods = new();

    private List<Modifier> modifiers = new();

    private void Awake()
    {
        operationStatistics = OperationStatistics.Empty();
        towerPlacementSystem.OnPlace += OnPlaceTower;
        towerSellManager.OnSellTower += OnSellTower;
        towerDataCatalog.OnUpgradeTower += OnUpgradeTower;
        skillPlacementSystem.OnUseSkill += OnUseSkill;
        nexus.OnHealthChanged += OnNexusHealthChange;
        nexus.OnDestroyed += OnNexusDestroyed;

        mainCamera = Camera.main;
        brain = mainCamera.GetComponent<CinemachineBrain>();

        SoundManagersDontDestroy.GerOrCreate().Music.PlayGameMusic();
    }

    private void Update()
    {
        Assert.IsNotNull(level);
        Assert.IsTrue(wavesSpawned <= level.waves.Count);
        Assert.IsTrue(enemiesLive >= 0, $"enemies live is not greater or equal to 0, {enemiesLive}");

        if (wavesSpawned == level.waves.Count && enemiesLive == 0 && !operationEnded)
        {
            OperationEnd(cleared: true);
        }
    }

    private void OnPlaceTower(ITower tower)
    {
        operationStatistics.towersBuilt++;

        tower.OnDamageDealt += OnTowerDamageDealt;
        tower.OnEnemyKilled += OnTowerEnemyKilled;
        tower.OnUpgrade += OnTowerUpgrade;

        tower.SetDamageCalculation((baseDmg) => towerMods.CalculateTowerProjectileDamage(tower, baseDmg));

        towers[tower.InstanceID()] = tower;
        tower.SetCritChangeCalculation((baseCritChange) => towerMods.CalculateTowerCritChance(tower, baseCritChange));
        foreach (var other in towers.Values)
        {
            other.RecalctCritChance();
        }

        tower.SetRange(towerMods.CalculateTowerRange(tower, tower.Range()));

        tower.SetFireRateCalculation((fireRate) => towerMods.CalculateTowerFireRate(tower, fireRate));
        TowerDataBase towerData = towerDataCatalog.FromTypeAndLevel(tower.TowerType(), 1);
        tower.ApplyUpgrade(towerData);
        SpendGears(towerData.Cost);

        if (ModifiersCalculator.IsGainRangeOnHillActive(modifiers))
        {
            tower.ActivateGainRangeOnHill();
        }

        switch (tower)
        {
            case TeslaTower tesla:
                ModifiersCalculator.ModifyTesla(tesla, modifiers);
                break;

            case FlamethrowerTower flamethrower:
                ModifiersCalculator.ModifyDOTTower(flamethrower, modifiers);
                flamethrower.SetFlameDurationCalculation((fireDuration) => towerMods.CalculateFlamethrowerFireDuration(flamethrower, fireDuration));
                flamethrower.SetDotDuration(towerMods.CalculateDOTDuration(flamethrower, flamethrower.BurnDuration));
                ModifiersCalculator.ModifyFlamethrower(flamethrower, modifiers);
                break;

            case GatlingTower gatling:
                ModifiersCalculator.ModifyGatling(gatling, modifiers);
                break;

            case MortarTower mortar:
                ModifiersCalculator.ModifyDOTTower(mortar, modifiers);
                ModifiersCalculator.ModifyMortar(mortar, modifiers);
                break;
        }
    }

    private void OnTowerDamageDealt(TowerTypes towerType, float damage)
    {
        operationStatistics.damageDealt += Mathf.FloorToInt(damage);
    }

    private void OnTowerEnemyKilled(TowerTypes towerType)
    {
        operationStatistics.towerKills[(int)towerType]++;
    }

    private void OnTowerUpgrade(TowerTypes towerType)
    {
        operationStatistics.towersUpgraded++;
    }

    private void OnSellTower(ITower tower)
    {
        TowerDataBase towerData = towerDataCatalog.FromTypeAndLevel(tower.TowerType(), tower.CurrentLevel());
        AddGears(towerData.SellPrice);
        towers.Remove(tower.InstanceID());
    }

    private void OnUpgradeTower(int upgradeCost)
    {
        upgradeCost = (int)(economyMods.towerUpgradeCostRatio * upgradeCost);
        SpendGears(upgradeCost);
    }

    private void OnUseSkill(ISkill skill)
    {
        if (skill.ActivationMode() == SkillActivationMode.Instant && skill is SuddenDeath suddenDeath)
        {
            HUDPanelUI.ShowSuddenDeathOverlay();
            gearRewardMultiplier = suddenDeath.GearRewardMultiplier;
            nexus.MakeVolatile();
        }

        HUDPanelUI.StartSkillCooldown(skill);

        if (economyMods.placeableAbilitiesCostGears)
        {
            SkillData skillData = skillDataCatalog.FromType(skill.SkillType());
            SpendGears(skillData.cost);
        }
    }

    private void OnNexusDestroyed(Nexus nexus)
    {
        OperationEnd(cleared: false);
    }

    private void OnNexusHealthChange(Nexus nexus, float change)
    {
        menuPanelUI.UpdateNexusHealth(nexus.HealthPointsNormalized());
        operationStatistics.damageTaken += (int)change;
    }

    public IEnumerator RunLevel(SerializableLevel level, SplineContainer splineContainer, OperationDataDontDestroy operationData, SaveContextDontDestroy saveContext)
    {
        Assert.IsNotNull(level);
        this.level = level;
        this.saveContext = saveContext;

        var fact = operationData.Faction;
        modifiers = operationData.Modifiers;
        economyMods = ModifiersCalculator.CalculateEconomyMods(passiveTick, passiveIncome, modifiers);
        var enemyMods = ModifiersCalculator.CalculateEnemyMods(modifiers, () => towers.Count);
        towerMods = ModifiersCalculator.CalculateTowerMods(modifiers, () => towers.Count);

        ModifiersCalculator.ModifyNexus(nexus, modifiers);

        gears = level.playerResources.initialGears;
        HUDPanelUI.UpdateGears(gears);
        UpdateTowerButtons();
        UpdateSkillButtons();

        waveCounterInfo.SetCounter(0, level.waves.Count);
        waveCounterInfo.SetGameSpeed(1f);

        var gearsRoutine = StartCoroutine(PassiveGearsIncomeRoutine(economyMods));

        operationStartTime = Time.time;
        operationStatistics.operationName = level.operationName;
        operationStatistics.totalWaves = level.waves.Count;
        operationStatistics.towerKills = new int[Enum.GetValues(typeof(TowerTypes)).Length];

        for (int waveIndex = 0; waveIndex < level.waves.Count; waveIndex++)
        {
            var wave = level.waves[waveIndex];
            if (!wave.enabled)
            {
                Debug.Log($"skipping disabled wave {waveIndex}");
                yield break;
            }
            perWaveEnemies[waveIndex] = 0;

            Assert.IsTrue(wave.prepareTimeSeconds != 0f, "Give a player some time to prepare...");

            yield return nextWaveCountdown.StartCountdown(wave.prepareTimeSeconds);

            waveCounterInfo.SetCounter(waveIndex + 1, level.waves.Count);
            HUDPanelUI.ShowWaveOverlay(waveIndex + 1);
            SoundManagersDontDestroy.GerOrCreate()?.SoundFX.PlaySoundFXClip(SoundFXType.WaveSpawn, transform);
            yield return spawner.RunSpawnWave(wave, waveIndex, splineContainer, (enemy) => OnEnemySpawn(enemy, enemyMods), (enemy) => OnEnemyKilled(enemy, enemyMods));
            wavesSpawned += 1;
        }

        StopCoroutine(gearsRoutine);
    }

    private IEnumerator PassiveGearsIncomeRoutine(EconomyMods economyMods)
    {
        float timer = 0f;

        while (true)
        {
            timer += Time.deltaTime;

            float progress = timer / economyMods.passiveGearsTick;
            progress = Mathf.Clamp01(progress);

            HUDPanelUI.SetPassiveGearsIncomeProgress(progress);

            if (timer >= economyMods.passiveGearsTick)
            {
                AddGears(Mathf.FloorToInt(economyMods.passiveGearsAmount));
                timer = 0f;
            }

            yield return null;
        }
    }

    private void OnEnemySpawn(IEnemy spawned, EnemyMods enemyMods)
    {
        spawned.Speed = enemyMods.CalculateEnemyMovementSpeed(spawned, spawned.Speed);
        if (level.devSettings.unkillableEnemies)
        {
            spawned.DEV_MakeUnkillable();
        }
        enemiesLive += 1;

        operationStatistics.totalEnemies++;

        if (enemyMods.enableBomberFriendlyfire && spawned is Bomber bomber)
            bomber.EnableFriendlyFire();

        perWaveEnemies[spawned.SpawnedInWave]++;
    }

    private void OnEnemyKilled(IEnemy killed, EnemyMods enemyMods)
    {
        int reward = economyMods.CalculateEnemyReward(killed.OnKillGearsReward);
        AddGears(reward);
        enemiesLive -= 1;

        operationStatistics.killedEnemies++;
        perWaveEnemies[killed.SpawnedInWave]--;
    }

    public void AddGears(int amount)
    {
        gears += Mathf.FloorToInt(amount * gearRewardMultiplier);
        HUDPanelUI.UpdateGears(gears);
        UpdateTowerButtons();
        UpdateSkillButtons();

        if (amount > 0) operationStatistics.gearsEarned += amount;
    }

    public void SpendGears(int amount)
    {
        gears -= amount;
        HUDPanelUI.UpdateGears(gears);
        UpdateTowerButtons();
        UpdateSkillButtons();

        if (amount > 0) operationStatistics.gearsSpent += amount;
    }

    private void UpdateTowerButtons()
    {
        (HashSet<TowerTypes> toEnable, HashSet<TowerTypes> toDisable) = towerDataCatalog.AdjustTowers(gears);

        foreach (TowerTypes type in toEnable)
        {
            HUDPanelUI.AdjustTowerButton(type, true);
        }

        foreach (TowerTypes type in toDisable)
        {
            HUDPanelUI.AdjustTowerButton(type, false);
        }
    }

    private void UpdateSkillButtons()
    {
        (HashSet<SkillTypes> toEnable, HashSet<SkillTypes> toDisable) = skillDataCatalog.AdjustSkills(gears);

        foreach (SkillTypes type in toEnable)
        {
            HUDPanelUI.AdjustSkillButton(type, true);
        }

        foreach (SkillTypes type in toDisable)
        {
            HUDPanelUI.AdjustSkillButton(type, false);
        }
    }

    private void OperationEnd(bool cleared)
    {
        operationEnded = true;

        brain.enabled = false;

        StartCoroutine(LerpTimeScale(5f));

        towerSelectionManager.DisableSelection();

        if (cleared)
        {
            float xpReward = experienceSystem.GetXPReward(level.operationIndex);
            saveContext.AddXP(xpReward);
            saveContext.SetHighestOperationClearedIndex(level.operationIndex);
            saveContext.Save();

            operationStatistics.xpReward = xpReward;
        }

        operationStatistics.cleared = cleared;
        operationStatistics.duration = Time.time - operationStartTime;
        operationStatistics.clearedWaves = perWaveEnemies.Count(kvp => kvp.Value == 0);

        operationResultUI.Initialize(operationStatistics);

        HUDPanelUI.gameObject.SetActive(false);
        menuPanelUI.gameObject.SetActive(false);
        waveCounterInfo.gameObject.SetActive(false);
        nextWaveCountdown.gameObject.SetActive(false);

        operationResultUI.gameObject.SetActive(true);
    }

    private IEnumerator LerpTimeScale(float duration)
    {
        float start = Time.timeScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(start, 0f, elapsed / duration);
            yield return null;
        }

        Time.timeScale = 0f;
    }

    public void ToggleGameSpeed()
    {
        float newSpeed = Time.timeScale == 1f ? 2f : 1f;
        waveCounterInfo.SetGameSpeed(newSpeed);
        Time.timeScale = newSpeed;
    }

    private void OnDestroy()
    {
        if (towerPlacementSystem != null)
            towerPlacementSystem.OnPlace -= OnPlaceTower;
        if (towerSellManager != null)
            towerSellManager.OnSellTower -= OnSellTower;
    }
}
