using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class SaveData
{
    /// <summary>
    /// e.g. "Save-1", MUST BE UNIQUE!
    /// </summary>
    public string name;
    /// <summary>
    /// ISO 8601 string, e.g. "2025-12-04T18:00:00Z"
    /// </summary>
    public string lastPlayed;

    public override string ToString() => JsonUtility.ToJson(this, prettyPrint: true);
}

public class SaveSystem : MonoBehaviour
{
    private static readonly string SavesFolder =
      Path.Combine(Application.persistentDataPath, "saves");

    private const string SaveNamePrefix = "Save-";
    private const string FileExtension = ".json";

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

    public static string SaveFilesIndex(string saveName) => saveName[SaveNamePrefix.Length..];

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

        var data = new SaveData
        {
            name = saveName,
            lastPlayed = DateTime.UtcNow.ToString("o"), // ISO 8601
        };

        SaveToFile(data);
        return data;
    }

    public static void UpdateSave(SaveData data)
    {
        data.lastPlayed = DateTime.UtcNow.ToString("o");
        SaveToFile(data);
    }

    public static SaveData LoadSave(string saveName)
    {
        CreateSavesFolder(SavesFolder);
        string path = GetSaveFilePath(saveName);
        Assert.IsTrue(File.Exists(path));
        string json = File.ReadAllText(path);
        try
        {
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading save file {saveName}: {e}");
            return null;
        }
    }

    private static void SaveToFile(SaveData data)
    {
        CreateSavesFolder(SavesFolder);
        string path = GetSaveFilePath(data.name);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"Saved {data.name} to {path}");
    }

    private static string GetSaveFilePath(string saveName) => Path.Combine(SavesFolder, saveName + FileExtension);

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
