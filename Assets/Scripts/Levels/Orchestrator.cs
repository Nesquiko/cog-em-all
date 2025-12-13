using System.Collections;
using System.Collections.Generic;
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
    private int enemiesLive = 0;

    private Dictionary<int, ITower> towers = new();

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

    private OperationStatistics operationStatistics;

    private int gears = 0;

    public int Gears => gears;

    private TowerMods towerMods = new();
    private EconomyMods economyMods = new();

    private List<Modifier> modifiers = new();

    private void Awake()
    {
        towerPlacementSystem.OnPlace += OnPlaceTower;
        towerSellManager.OnSellTower += OnSellTower;
        towerDataCatalog.OnUpgradeTower += OnUpgradeTower;
        skillPlacementSystem.OnUseSkill += OnUseSkill;
        nexus.OnHealthChanged += OnNexusHealthChange;
        nexus.OnDestroyed += OnNexusDestroyed;

        mainCamera = Camera.main;
        brain = mainCamera.GetComponent<CinemachineBrain>();
    }

    private void Update()
    {
        Assert.IsNotNull(level);
        Assert.IsTrue(wavesSpawned <= level.waves.Count);
        Assert.IsTrue(enemiesLive >= 0, $"enemies live is not greater or equal to 0, {enemiesLive}");

        if (wavesSpawned == level.waves.Count && enemiesLive == 0)
        {
            OperationEnd(cleared: true);
        }
    }

    private void OnPlaceTower(ITower tower)
    {

        tower.SetDamageCalculation((baseDmg) => towerMods.CalculateTowerProjectileDamage(tower, baseDmg));

        towers[tower.InstanceID()] = tower;
        tower.SetCritChangeCalculation((baseCritChange) => towerMods.CalculateTowerCritChance(tower, baseCritChange));
        foreach (var other in towers.Values)
        {
            other.RecalctCritChance();
        }

        tower.SetRange(towerMods.CalculateTowerRange(tower, tower.Range()));

        tower.SetFireRateCalculation((fireRate) => towerMods.CalculateTowerFireRate(tower, fireRate));
        TowerDataBase towerData = towerDataCatalog.FromTypeAndLevel(tower.TowerType(), tower.CurrentLevel());
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
                break;

            case GatlingTower gatling:
                ModifiersCalculator.ModifyGatling(gatling, modifiers);
                break;

            case MortarTower mortar:
                ModifiersCalculator.ModifyDOTTower(mortar, modifiers);
                break;
        }
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

        SkillData skillData = skillDataCatalog.FromType(skill.SkillType());
        SpendGears(skillData.cost);
    }

    private void OnNexusDestroyed(Nexus nexus)
    {
        OperationEnd(cleared: false);
    }

    private void OnNexusHealthChange(Nexus nexus)
    {
        menuPanelUI.UpdateNexusHealth(nexus.HealthPointsNormalized());
    }

    public IEnumerator RunLevel(SerializableLevel level, SplineContainer splineContainer, OperationDataDontDestroy operationData)
    {
        Assert.IsNotNull(level);
        this.level = level;

        var fact = operationData.Faction;
        modifiers = operationData.Modifiers;
        economyMods = ModifiersCalculator.CalculateEconomyMods(passiveTick, passiveIncome, modifiers);
        var enemyMods = ModifiersCalculator.CalculateEnemyMods(modifiers);
        towerMods = ModifiersCalculator.CalculateTowerMods(modifiers, () => towers.Count);

        gears = level.playerResources.initialGears;
        HUDPanelUI.UpdateGears(gears);
        UpdateTowerButtons();
        UpdateSkillButtons();

        waveCounterInfo.SetCounter(0, level.waves.Count);
        waveCounterInfo.SetGameSpeed(1f);

        var gearsRoutine = StartCoroutine(PassiveGearsIncomeRoutine(economyMods));

        for (int w = 0; w < level.waves.Count; w++)
        {
            var wave = level.waves[w];
            if (!wave.enabled)
            {
                Debug.Log($"skipping disabled wave {w}");
                yield break;
            }

            Assert.IsTrue(wave.prepareTimeSeconds != 0f, "Give a player some time to prepare...");

            yield return nextWaveCountdown.StartCountdown(wave.prepareTimeSeconds);

            waveCounterInfo.SetCounter(w + 1, level.waves.Count);
            yield return spawner.RunSpawnWave(wave, w, splineContainer, (enemy) => OnEnemySpawn(enemy, enemyMods), (enemy) => OnEnemyKilled(enemy, enemyMods));
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
    }

    private void OnEnemyKilled(IEnemy killed, EnemyMods enemyMods)
    {
        int reward = enemyMods.CalculateEnemyReward(killed.OnKillGearsReward);
        AddGears(reward);
        enemiesLive -= 1;
    }

    public void AddGears(int amount)
    {
        gears += Mathf.FloorToInt(amount * gearRewardMultiplier);
        HUDPanelUI.UpdateGears(gears);
        UpdateTowerButtons();
        UpdateSkillButtons();
    }

    public void SpendGears(int amount)
    {
        gears -= amount;
        HUDPanelUI.UpdateGears(gears);
        UpdateTowerButtons();
        UpdateSkillButtons();
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
        brain.enabled = false;

        StartCoroutine(LerpTimeScale(5f));

        towerSelectionManager.DisableSelection();
        operationStatistics = cleared ? OperationStatistics.CreateDummyCleared() : OperationStatistics.CreateDummyFailed();
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
