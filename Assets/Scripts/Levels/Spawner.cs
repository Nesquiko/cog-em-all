using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

class Spawner : MonoBehaviour
{

    [Header("Enemy catalog Scriptable Object")]
    [SerializeField]
    private EnemyCatalog enemyCatalog;

    [Header("Enemy spawn settings")]
    [SerializeField]
    public Vector2 spawnTimeStaggerRange = new(0f, .005f);
    [SerializeField]
    public Vector2 spawnLateralOffsetRange = new(-5f, 5f);

    void Awake()
    {
        Assert.IsTrue(spawnTimeStaggerRange.x < spawnTimeStaggerRange.y);
        Assert.IsTrue(spawnLateralOffsetRange.x < spawnLateralOffsetRange.y);
    }


    public IEnumerator RunSpawnWave(Wave wave, int waveIndex, SplineContainer splineContainer)
    {
        Assert.IsNotNull(splineContainer);
        Assert.IsTrue(splineContainer.Splines.Count > 0);

        for (int g = 0; g < wave.spawnGroups.Count; g++)
        {
            var group = wave.spawnGroups[g];
            yield return RunSpawnGroup(waveIndex, g, group, splineContainer);
        }
    }

    private IEnumerator RunSpawnGroup(int waveIndex, int groupIndex, SpawnGroup group, SplineContainer splineContainer)
    {

        for (int r = 0; r < group.repeat; r++)
        {
            for (int p = 0; p < group.pattern.Count; p++)
            {
                var entry = group.pattern[p];
                Enemy prefab = enemyCatalog.Get(entry.enemy);

                for (int i = 0; i < entry.count; i++)
                {
                    Enemy enemy = Instantiate(prefab);
                    float startT = Mathf.Clamp01(Random.Range(spawnTimeStaggerRange.x, spawnTimeStaggerRange.y));
                    float lateralOffset = Random.Range(spawnLateralOffsetRange.x, spawnLateralOffsetRange.y);
                    enemy.SetSpline(splineContainer, startT, lateralOffset);

                    // delay spawn of next enemy in this entry, if the spawnRateSeconds is set to something
                    // greater than 0
                    // if spawn is <= 0, enemies are spawned at once
                    if (entry.spawnRateSeconds > 0f)
                    {
                        yield return new WaitForSeconds(entry.spawnRateSeconds);
                    }
                }
            }


            // delay spawn of next cycle of pattern
            if (group.spawnRateSeconds > 0f)
            {
                yield return new WaitForSeconds(group.spawnRateSeconds);
            }
        }

        Debug.Log($"wave {waveIndex} group {groupIndex} pause before next wave {group.pauseAfterLastSpawnSeconds:F2}s");
        yield return new WaitForSeconds(group.pauseAfterLastSpawnSeconds);
    }

}