using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class SaveData
{
    /// <summary>
    /// e.g. "Save-1", MUST BE UNIQUE!, the file name will match this name.
    /// </summary>
    public string name;

    /// <summary>
    /// ISO 8601 string, e.g. "2025-12-04T18:00:00Z"
    /// </summary>
    public string lastPlayed;

    public FactionSaveState brassArmySave;
    public FactionSaveState seraphsSave;
    public FactionSaveState overpressuSave;

    public SaveData() { }

    public SaveData(
        string name,
        DateTime lastPlayed,
        FactionSaveState brassArmySave,
        FactionSaveState seraphsSave,
        FactionSaveState overpressuSave
    )
    {
        this.name = name;
        this.lastPlayed = lastPlayed.ToString("o");
        this.brassArmySave = brassArmySave;
        this.seraphsSave = seraphsSave;
        this.overpressuSave = overpressuSave;
    }

    public override string ToString() => JsonUtility.ToJson(this, prettyPrint: true);
}

[Serializable]
public class FactionSaveState
{
    public const int FactionLevelMax = 15;

    public FactionSaveState() { }

    public FactionSaveState(int level, Dictionary<string, int> skillNodes)
    {
        this.level = level;
        this.skillNodes = new();
        foreach (var skillNode in skillNodes)
        {
            this.skillNodes.Add(new SkillNodeEntry { slug = skillNode.Key, level = skillNode.Value });
        }
    }

    public int level;

    [Serializable]
    public class SkillNodeEntry
    {
        public string slug;
        public int level;
    }

    // JsonUtil doesn't support serializing dictionaries...
    private List<SkillNodeEntry> skillNodes = new();

    public Dictionary<string, int> SkillNodes()
    {
        var dict = new Dictionary<string, int>();
        foreach (var entry in skillNodes)
        {
            dict[entry.slug] = entry.level;
        }

        return dict;
    }
}

public class SaveSystem : MonoBehaviour
{
    private static readonly string SavesFolder =
      Path.Combine(Application.persistentDataPath, "saves");

    private const string SaveNamePrefix = "Save-";
    private const string FileExtension = ".json";

    private static readonly string DevSavesFolder = Path.Combine(SavesFolder, "dev");
    private const string DevSaveName = "Save-DEV";

    static SaveSystem()
    {
        CreateSavesFolder(SavesFolder);
    }

    public static int CountSaveFiles()
    {
        CreateSavesFolder(SavesFolder);
        string[] files = Directory.GetFiles(SavesFolder, "*" + FileExtension);
        return files.Length;
    }

    public static string SaveFileNumber(string saveName) => saveName[SaveNamePrefix.Length..];

    public static List<SaveData> LoadAllSaves()
    {
        CreateSavesFolder(SavesFolder);

        var saves = new List<SaveData>();
        string[] files = Directory.GetFiles(SavesFolder, "*" + FileExtension);

        foreach (var file in files)
        {
            if (!IsSaveFile(file))
            {
                Debug.LogWarning($"there is an unknown file '{file}' in the saves folder");
                continue;
            }

            try
            {
                string json = File.ReadAllText(file);
                Assert.IsNotNull(json);
                var data = JsonUtility.FromJson<SaveData>(json);
                Assert.IsNotNull(data);
                saves.Add(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading save file {file}: {e}");
            }
        }

        return saves;
    }

    public static SaveData CreateNewSave()
    {
        CreateSavesFolder(SavesFolder);

        int nextIndex = GetNextSaveIndex();
        string saveName = $"{SaveNamePrefix}{nextIndex}";

        var data = new SaveData(
            name: saveName,
            lastPlayed: DateTime.UtcNow,
            brassArmySave: new FactionSaveState(0, new()),
            seraphsSave: new FactionSaveState(0, new()),
            overpressuSave: new FactionSaveState(0, new())
        );

        SaveToFile(data, SavesFolder);
        return data;
    }

    public static void UpdateSave(SaveData data, string folder)
    {
        data.lastPlayed = DateTime.UtcNow.ToString("o");
        SaveToFile(data, folder);
    }

    public static SaveData LoadSave(string saveName, string folder)
    {
        var fullSavePath = Path.Combine(folder, saveName);
        if (!File.Exists(fullSavePath)) return null;

        string json = File.ReadAllText(fullSavePath);
        try
        {
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading save file {fullSavePath}: {e}");
            return null;
        }
    }

    public static SaveData LoadDevSave()
    {
        CreateSavesFolder(DevSavesFolder);

        var devSave = LoadSave(DevSaveName + FileExtension, DevSavesFolder);
        if (devSave != null) return devSave;

        var devSaveData = new SaveData(
            name: DevSaveName,
            lastPlayed: DateTime.UtcNow,
            brassArmySave: new FactionSaveState(FactionSaveState.FactionLevelMax, new()),
            seraphsSave: new FactionSaveState(FactionSaveState.FactionLevelMax, new()),
            overpressuSave: new FactionSaveState(FactionSaveState.FactionLevelMax, new())
        );

        SaveToFile(devSaveData, DevSavesFolder);
        return devSaveData;
    }

    private static void SaveToFile(SaveData data, string folder)
    {
        CreateSavesFolder(folder);

        var path = GetSaveFilePath(data.name, folder);
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(path, json);
        Debug.Log($"Saved {data.name} to {path}");
    }

    private static string GetSaveFilePath(string saveName, string folder) => Path.Combine(folder, saveName + FileExtension);

    private static int GetNextSaveIndex()
    {
        CreateSavesFolder(SavesFolder);
        var saves = LoadAllSaves();

        int maxIndex = 0;
        foreach (var save in saves)
        {
            // Expect format "Save-X"
            if (!save.name.StartsWith(SaveNamePrefix))
            {
                Debug.LogWarning($"there is a save with unknown name '{save.name}' in the saves folder");
                continue;
            }

            string suffix = save.name[SaveNamePrefix.Length..];
            if (int.TryParse(suffix, out int index))
            {
                if (index > maxIndex) maxIndex = index;
            }
            else
            {
                Debug.LogWarning($"there is a save '{save.name}' with unparsable int in name");
                continue;
            }
        }

        return maxIndex + 1;
    }

    private static bool IsSaveFile(string absoluteFilePath)
    {
        string fileName = Path.GetFileName(absoluteFilePath);
        return fileName.StartsWith(SaveNamePrefix) && fileName.EndsWith(FileExtension);
    }

    private static void CreateSavesFolder(string savesDirName)
    {
        if (!Directory.Exists(savesDirName))
            Directory.CreateDirectory(savesDirName);
    }
}
