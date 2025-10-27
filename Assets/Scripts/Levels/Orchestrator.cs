using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

[RequireComponent(typeof(Spawner))]
class Orchestrator : MonoBehaviour
{
    [SerializeField]
    private Spawner spawner;

    [Header("UI")]
    [SerializeField] private WaveCounterInfo waveCounterInfo;
    [SerializeField] private NextWaveCountdownInfo nextWaveCountdown;
    [SerializeField] private HUDPanelUI hUDPanelUI;

    [Header("Player resources")]
    [SerializeField] private int passiveGearsIncome = 10;
    private int gears = 0;

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
            gears += passiveGearsIncome;
            hUDPanelUI.UpdateGears(gears);
            yield return wait;
        }
    }

    private void AddOnKillGears(Enemy killed)
    {
        gears += killed.OnKillGearsReward;
        hUDPanelUI.UpdateGears(gears);
    }
}