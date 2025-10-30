using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;
using UnityEngine.UI;

[RequireComponent(typeof(Spawner))]
class Orchestrator : MonoBehaviour
{
    [SerializeField] private Spawner spawner;

    [SerializeField] private TowerDataCatalog towerDataCatalog;

    [Header("UI")]
    [SerializeField] private WaveCounterInfo waveCounterInfo;
    [SerializeField] private NextWaveCountdownInfo nextWaveCountdown;
    [SerializeField] private HUDPanelUI HUDPanelUI;

    [Header("Player resources")]
    [SerializeField] private int passiveGearsIncome = 10;
    private int gears = 0;

    public int Gears => gears;

    private void Awake()
    {
        TowerPlacementSystem.Instance.OnPlace += OnPlaceTower;
    }

    private void OnPlaceTower(TowerTypes type)
    {
        TowerData towerData = towerDataCatalog.FromType(type);
        SpendGears(towerData.cost);
    }

    public IEnumerator RunLevel(SerializableLevel level, SplineContainer splineContainer)
    {
        Assert.IsNotNull(level);
        gears = level.playerResources.initialGears;

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
            yield return spawner.RunSpawnWave(wave, w, splineContainer, AddOnKillGears);
        }

        StopCoroutine(gearsRoutine);
    }

    private IEnumerator PassiveGearsIncomeRoutine()
    {
        var wait = new WaitForSeconds(1f);
        while (true)
        {
            AddGears(passiveGearsIncome);
            yield return wait;
        }
    }

    private void AddOnKillGears(Enemy killed)
    {
        AddGears(killed.OnKillGearsReward);
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
}