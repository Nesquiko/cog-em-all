using System;
using System.Collections.Generic;
using UnityEngine;

// music / sfx work when starting in HomeScene

// to add more sounds, create a new enum value and assign
// one or more clips to it inside SoundFXDatabase SO
public enum SoundFXType
{
    GatlingShoot,
    MortarShoot,
    TeslaShoot,
    FlamethrowerShoot,

    BanditHit,
    BomberHit,
    DreadnoughtHit,

    AirshipDrop,
    AirshipHit,
}

[Serializable]
public class SoundFXEntry
{
    public SoundFXType type;
    public AudioClip[] clips;
}

[CreateAssetMenu(fileName = "SoundFXDatabase", menuName = "Scriptable Objects/Sound FX Database")]
public class SoundFXDatabase : ScriptableObject
{
    [Header("Audio Clips")]
    [SerializeField] private SoundFXEntry[] soundFXEntries;

    private readonly Dictionary<SoundFXType, AudioClip[]> database = new();

    private void OnEnable()
    {
        RebuildDatabase();
    }

    private void RebuildDatabase()
    {
        database.Clear();

        foreach (var entry in soundFXEntries)
        {
            if (entry == null || entry.clips == null || entry.clips.Length == 0)
                continue;

            if (!database.ContainsKey(entry.type))
                database.Add(entry.type, entry.clips);
            else
                Debug.LogWarning($"Duplicate SoundFXType {entry.type} found in SoundFXDatabase.");
        }
    }

    public AudioClip[] GetClips(SoundFXType type)
    {
        return database.TryGetValue(type, out var clips) ? clips : null;
    }

    public AudioClip GetRandomClip(SoundFXType type)
    {
        if (!database.TryGetValue(type, out var clips) || clips.Length == 0)
            return null;

        int index = UnityEngine.Random.Range(0, clips.Length);
        return clips[index];
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Database")]
    private void EditorRebuild() => RebuildDatabase();
#endif
}
