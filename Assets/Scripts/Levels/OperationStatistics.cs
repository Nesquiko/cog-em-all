using System;
using UnityEngine;

[Serializable]
public class OperationStatistics
{
    [Header("Operation Info")]
    public bool cleared;
    public string operationName;
    public float duration;
    public int clearedWaves;
    public int totalWaves;

    [Header("Offensive Performance")]
    public int killedEnemies;
    public int totalEnemies;
    public int damageDealt;
    public int damageTaken;

    [Header("Resource Summary")]
    public int gearsEarned;
    public int gearsSpent;
    public int towersBuilt;
    public int towersUpgraded;

    [Header("Towers")]
    public int[] towerKills;

    public static OperationStatistics Empty()
    {
        return new()
        {
            cleared = false,
            operationName = "",
            duration = 0f,
            totalWaves = 0,
            clearedWaves = 0,

            totalEnemies = 0,
            killedEnemies = 0,
            damageDealt = 0,
            damageTaken = 0,

            gearsEarned = 0,
            gearsSpent = 0,
            towersBuilt = 0,
            towersUpgraded = 0,

            towerKills = new int[4]
        };
    }

    public static OperationStatistics CreateDummyCleared()
    {
        var random = new System.Random();

        int totalWaves = random.Next(5, 15);
        int totalEnemies = random.Next(500, 1500); 

        return new() {
            cleared = true,
            operationName = "Operation Badwater Basin",
            duration = random.Next(300, 720),
            totalWaves = totalWaves,
            clearedWaves = totalWaves,

            totalEnemies = totalEnemies,
            killedEnemies = totalEnemies,
            damageDealt = random.Next(150_000, 250_000),
            damageTaken = random.Next(150, 750),

            gearsEarned = random.Next(1000, 10000),
            gearsSpent = random.Next(1500, 3500),
            towersBuilt = random.Next(5, 20),
            towersUpgraded = random.Next(0, 7),

            towerKills = new int[4]
            {
                random.Next(50, 200),
                random.Next(50, 200),
                random.Next(50, 200),
                random.Next(50, 200)
            }
        };
    }

    public static OperationStatistics CreateDummyFailed()
    {
        var random = new System.Random();

        int totalWaves = random.Next(5, 15);
        int totalEnemies = random.Next(500, 1500);

        return new()
        {
            cleared = false,
            operationName = "Operation Badwater Basin",
            duration = random.Next(300, 720),
            totalWaves = totalWaves,
            clearedWaves = random.Next(0, totalWaves),

            totalEnemies = totalEnemies,
            killedEnemies = random.Next(0, totalEnemies),
            damageDealt = random.Next(150_000, 250_000),
            damageTaken = random.Next(150, 750),

            gearsEarned = random.Next(1000, 10000),
            gearsSpent = random.Next(1500, 3500),
            towersBuilt = random.Next(5, 20),
            towersUpgraded = random.Next(0, 7),

            towerKills = new int[4]
            {
                random.Next(50, 200),
                random.Next(50, 200),
                random.Next(50, 200),
                random.Next(50, 200)
            }
        };
    }
}