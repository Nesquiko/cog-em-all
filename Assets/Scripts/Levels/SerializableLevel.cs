using UnityEngine.Splines;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SerializableLevel
{
    public string operationName;
    public float operationDifficulty;
    public int operationIndex;
    public PlayerResources playerResources;
    public List<Wave> waves = new();
    public List<Spline> pathSplines = new();

    public DevSettings devSettings;

    public static string ToJson(SerializableLevel lvl)
    {
        return JsonUtility.ToJson(lvl);
    }

    public static SerializableLevel FromJson(string json)
    {
        return JsonUtility.FromJson<SerializableLevel>(json);
    }
}

[Serializable]
public class Wave
{
    /// <summary> 
    /// flag to turn wave on/off for testing
    /// </summary> 
    public bool enabled = true;

    /// <summary> 
    /// how long to wait until this wave should be spawned
    /// </summary> 
    public float prepareTimeSeconds = 0f;

    /// <summary> 
    /// groups that will be spawned in this way
    /// </summary> 
    public List<SpawnGroup> spawnGroups;
}

[Serializable]
public class SpawnGroup
{
    /// <summary> 
    /// sequence/pattern of entries/enemies in this group
    /// </summary> 
    public List<PatternEntry> pattern;

    /// <summary> 
    /// how many times to repeat the whole pattern
    /// </summary> 
    public int repeat = 1;

    /// <summary> 
    /// spawn rate of one pattern repeat
    /// </summary> 
    public float spawnRateSeconds = 0f;

    /// <summary> 
    /// pause after the whole group is spawned
    /// </summary> 
    public float pauseAfterLastSpawnSeconds = 0f;
}

[Serializable]
public class PatternEntry
{
    /// <summary> 
    /// type of enemy
    /// </summary> 
    public EnemyType enemy;

    /// <summary> 
    /// how many enemies of this type to spawn
    /// </summary> 
    public int count = 1;

    /// <summary> 
    /// spawn rate between each enemy in this entry, if set to 0, `count` of enemies will spawn at once
    /// </summary> 
    public float spawnRateSeconds = 0f;
}

[Serializable]
public class PlayerResources
{

    /// <summary> 
    /// intial number of gears at the start of a level
    /// </summary> 
    public int initialGears = 0;
}


[Serializable]
public class DevSettings
{
    public bool unkillableEnemies;
}