using UnityEngine.Splines;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableLevel
{
    public List<Wave> waves = new List<Wave>();
    public List<Spline> pathSplines = new List<Spline>();

    public static string ToJson(SerializableLevel lvl)
    {
        return JsonUtility.ToJson(lvl);
    }

    public static SerializableLevel FromJson(string json)
    {
        return JsonUtility.FromJson<SerializableLevel>(json);
    }
}

[System.Serializable]
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

[System.Serializable]
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

[System.Serializable]
public class PatternEntry
{
    /// <summary> 
    /// type of enemy
    /// </summary> 
    public string enemy;

    /// <summary> 
    /// how many enemies of this type to spawn
    /// </summary> 
    public int count = 1;

    /// <summary> 
    /// spawn rate between each enemy in this entry, if set to 0, `count` of enemies will spawn at once
    /// </summary> 
    public float spawnRateSeconds = 0f;
}