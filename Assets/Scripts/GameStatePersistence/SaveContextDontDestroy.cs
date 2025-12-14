using UnityEngine;
using UnityEditor;
using System;

public class SaveContextDontDestroy : MonoBehaviour
{
    public SaveData CurrentSave { get; private set; }

    public void SetCurrentSave(SaveData saveData)
    {
        CurrentSave = saveData;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Save() => SaveSystem.UpdateSave(CurrentSave);

    public void AddXP(float xp)
    {
        var (_, playedFaction) = LastFactionSaveState();
        playedFaction.totalXP += xp;
        Save();
    }

    public (Faction, FactionSaveState) LastFactionSaveState()
    {
        return CurrentSave.LastPlayedFaction switch
        {
            Faction.TheBrassArmy => (Faction.TheBrassArmy, CurrentSave.brassArmySave),
            Faction.TheValveboundSeraphs => (Faction.TheValveboundSeraphs, CurrentSave.seraphsSave),
            Faction.OverpressureCollective => (Faction.OverpressureCollective, CurrentSave.overpressuSave),
            _ => throw new ArgumentOutOfRangeException(nameof(CurrentSave.lastPlayedFaction), CurrentSave.lastPlayedFaction, "Unhandled faction"),
        };
    }

    public static SaveContextDontDestroy GetOrCreateDev()
    {
        var existing = FindFirstObjectByType<SaveContextDontDestroy>();
        if (existing != null) return existing;

        Debug.Log("Using dev save");
        var go = new GameObject("SaveContext (Dev)");
        var ctx = go.AddComponent<SaveContextDontDestroy>();
        ctx.SetCurrentSave(SaveSystem.LoadDevSave());
        return ctx;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(SaveContextDontDestroy))]
public class SaveContextDontDestroyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var ctx = (SaveContextDontDestroy)target;
        var save = ctx.CurrentSave;

        if (save == null)
        {
            EditorGUILayout.LabelField("CurrentSave", "None (null)");
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Save Context Info", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Name", save.name ?? "null");

        if (DateTime.TryParse(save.lastPlayed, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
        {
            var local = parsed.ToLocalTime();
            EditorGUILayout.LabelField(
                "Last Played (local)",
                local.ToString("yyyy-MM-dd HH:mm:ss")
            );
        }
        else
        {
            EditorGUILayout.LabelField("Last Played (local)", $"Invalid or empty date '{save.lastPlayed}'");
        }
    }
}
#endif