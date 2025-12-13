using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

class Spawner : MonoBehaviour
{

    [Header("Enemy prefabs")]
    [SerializeField] private Bomber bomberPrefab;
    [SerializeField] private Bandit banditPrefab;
    [SerializeField] private Dreadnought dreadnoughtPrefab;

    [Header("Enemy spawn settings")]
    [SerializeField] private GameObject spawnInThisGameObject;
    [SerializeField] private Vector2 spawnTimeStaggerRange = new(0f, .005f);
    [SerializeField] private Vector2 spawnLateralOffsetRange = new(-5f, 5f);

    void Awake()
    {
        Assert.IsTrue(spawnTimeStaggerRange.x < spawnTimeStaggerRange.y);
        Assert.IsTrue(spawnLateralOffsetRange.x < spawnLateralOffsetRange.y);
    }


    public IEnumerator RunSpawnWave(Wave wave, int waveIndex, SplineContainer splineContainer, Action<IEnemy> onEnemySpawn, Action<IEnemy> onEnemyDeath)
    {
        Assert.IsNotNull(splineContainer);
        Assert.IsTrue(splineContainer.Splines.Count > 0);

        for (int g = 0; g < wave.spawnGroups.Count; g++)
        {
            var group = wave.spawnGroups[g];
            yield return RunSpawnGroup(waveIndex, g, group, splineContainer, onEnemySpawn, onEnemyDeath);
        }
    }

    private IEnumerator RunSpawnGroup(int waveIndex, int groupIndex, SpawnGroup group, SplineContainer splineContainer, Action<IEnemy> onEnemySpawn, Action<IEnemy> onEnemyDeath)
    {

        for (int r = 0; r < group.repeat; r++)
        {
            for (int p = 0; p < group.pattern.Count; p++)
            {
                var entry = group.pattern[p];

                for (int i = 0; i < entry.count; i++)
                {
                    var enemy = SpawnEnemy(entry.enemy);
                    onEnemySpawn.Invoke(enemy);
                    float startT = Mathf.Clamp01(UnityEngine.Random.Range(spawnTimeStaggerRange.x, spawnTimeStaggerRange.y));
                    float lateralOffset = UnityEngine.Random.Range(spawnLateralOffsetRange.x, spawnLateralOffsetRange.y);
                    enemy.Initialize(splineContainer, startT, lateralOffset, onEnemyDeath);

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

        yield return new WaitForSeconds(group.pauseAfterLastSpawnSeconds);
    }

    private IEnemy SpawnEnemy(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Bandit => Instantiate(banditPrefab, spawnInThisGameObject.transform),
            EnemyType.Bomber => Instantiate(bomberPrefab, spawnInThisGameObject.transform),
            EnemyType.Dreadnought => Instantiate(dreadnoughtPrefab, spawnInThisGameObject.transform),
            _ => throw new ArgumentOutOfRangeException(
                       nameof(enemyType),
                       enemyType,
                       "Unhandled enemy type"
                   )
        };
    }

}