using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    [SerializeField] private TowerDataCatalog towerDataCatalog;

    [SerializeField] private Nexus nexus;
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;
    [SerializeField] private TowerSellManager towerSellManager;

    [Header("UI")]
    [SerializeField] private WaveCounterInfo waveCounterInfo;
    [SerializeField] private NextWaveCountdownInfo nextWaveCountdown;
    [SerializeField] private HUDPanelUI HUDPanelUI;

    [Header("Player resources")]
    [SerializeField] private int passiveIncome = 10;
    [SerializeField] private float passiveTick = 5f;

    private int gears = 0;

    public int Gears => gears;

    private void Awake()
    {
        towerPlacementSystem.OnPlace += OnPlaceTower;
        towerSellManager.OnSellTower += OnSellTower;
        nexus.OnDestroyed += OnNexusDestroyed;
    }

    private void Update()
    {
        Assert.IsNotNull(level);
        Assert.IsTrue(wavesSpawned <= level.waves.Count);
        Assert.IsTrue(enemiesLive >= 0, $"enemies live is not greater or equal to 0, {enemiesLive}");

        if (wavesSpawned == level.waves.Count && enemiesLive == 0)
        {
            // TODO luky show operation cleared screen
            Debug.Log("operation cleared");
            EditorApplication.isPlaying = false;
            Application.Quit();
        }
    }

    private void OnPlaceTower(TowerTypes type)
    {
        TowerData towerData = towerDataCatalog.FromType(type);
        SpendGears(towerData.cost);
    }

    private void OnSellTower(TowerTypes type)
    {
        TowerData towerData = towerDataCatalog.FromType(type);
        AddGears(towerData.sellPrice);
    }

    private void OnNexusDestroyed(Nexus nexus)
    {
        // TODO luky show operation failed screen
        Debug.Log("operation failed");
        EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public IEnumerator RunLevel(SerializableLevel level, SplineContainer splineContainer)
    {
        Assert.IsNotNull(level);
        this.level = level;

        gears = level.playerResources.initialGears;
        HUDPanelUI.UpdateGears(gears);
        UpdateTowerButtons();

        waveCounterInfo.SetCounter(0, level.waves.Count);

        var gearsRoutine = StartCoroutine(PassiveGearsIncomeRoutine());

        for (int w = 0; w < level.waves.Count; w++)
        {
            var wave = level.waves[w];
            if (!wave.enabled)
            {
                Debug.Log($"skipping disabled wave {w}");
                yield break;
            }

            Assert.IsTrue(wave.prepareTimeSeconds != 0f, "Give a player some time to preapre...");

            yield return nextWaveCountdown.StartCountdown(wave.prepareTimeSeconds);

            waveCounterInfo.SetCounter(w + 1, level.waves.Count);
            yield return spawner.RunSpawnWave(wave, w, splineContainer, OnEnemySpawn, OnEnemyKilled);
            wavesSpawned += 1;
        }

        StopCoroutine(gearsRoutine);
    }

    private IEnumerator PassiveGearsIncomeRoutine()
    {
        float timer = 0f;

        while (true)
        {
            timer += Time.deltaTime;

            float progress = timer / passiveTick;
            progress = Mathf.Clamp01(progress);

            HUDPanelUI.SetPassiveGearsIncomeProgress(progress);

            if (timer >= passiveTick)
            {
                AddGears(passiveIncome);
                timer = 0f;
            }

            yield return null;
        }
    }

    private void OnEnemySpawn(Enemy spawned)
    {
        enemiesLive += 1;
    }

    private void OnEnemyKilled(Enemy killed)
    {
        AddGears(killed.OnKillGearsReward);
        enemiesLive -= 1;
    }

    public void AddGears(int amount)
    {
        gears += amount;
        HUDPanelUI.UpdateGears(gears);
        UpdateTowerButtons();
    }

    public void SpendGears(int amount)
    {
        gears -= amount;
        HUDPanelUI.UpdateGears(gears);
        UpdateTowerButtons();
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

    private void OnDestroy()
    {
        if (towerPlacementSystem != null)
            towerPlacementSystem.OnPlace -= OnPlaceTower;
        if (towerSellManager != null)
            towerSellManager.OnSellTower -= OnSellTower;
    }
}
