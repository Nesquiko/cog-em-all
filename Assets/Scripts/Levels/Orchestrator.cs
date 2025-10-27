using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

[RequireComponent(typeof(Spawner))]
class Orchestator : MonoBehaviour
{

    [SerializeField]
    private Spawner spawner;

    [Header("UI")]
    [SerializeField]
    private WaveCounterInfo waveCounterInfo;
    [SerializeField]
    private NextWaveCountdownInfo nextWaveCountdown;

    public IEnumerator RunLevel(SerializableLevel level, SplineContainer splineContainer)
    {
        Assert.IsNotNull(level);

        waveCounterInfo.SetCounter(0, level.waves.Count);

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
            yield return spawner.RunSpawnWave(wave, w, splineContainer);
        }
    }
}