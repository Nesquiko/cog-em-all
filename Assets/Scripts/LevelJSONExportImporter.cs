using UnityEngine;
using UnityEngine.Splines;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class Level
{
    public List<Spline> pathSplines;

    public Level()
    {
        pathSplines = new List<Spline>();
    }

    public Level(IReadOnlyList<Spline> splines)
    {
        pathSplines = new List<Spline>(splines);
    }
}

public class LevelJSONExportImporter : MonoBehaviour
{
    [SerializeField]
    private SplineContainer splineContainer;

    public string ToJSON()
    {
        if (splineContainer == null) return null;

        Level level = new Level(splineContainer.Splines);
        return JsonUtility.ToJson(level);
    }

    public void FromJSON(string json)
    {
        if (splineContainer == null) return;

        Level level = JsonUtility.FromJson<Level>(json);
        if (level == null || level.pathSplines == null) return;

        foreach (var spline in level.pathSplines)
        {
            splineContainer.AddSpline(spline);
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(LevelJSONExportImporter))]
public class LevelJSONExportImporterInspector : Editor
{
    private string exportFilename = "";
    private string importFilePath = "";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var exporter = (LevelJSONExportImporter)target;

        if (string.IsNullOrEmpty(exportFilename))
        {
            exportFilename = $"level-{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
        }

        GUILayout.Space(15);

        // Export Section
        EditorGUILayout.LabelField("Export Level to JSON", EditorStyles.boldLabel);
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Filename:", GUILayout.Width(60));
        exportFilename = EditorGUILayout.TextField(exportFilename);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        if (GUILayout.Button("Export to JSON", GUILayout.Height(30)))
        {
            ExportLevelToJSON(exporter);
        }

        GUILayout.Space(20);

        // Import Section
        EditorGUILayout.LabelField("Import Level from JSON", EditorStyles.boldLabel);
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("File:", GUILayout.Width(35));
        EditorGUILayout.TextField(importFilePath, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFilePanel("Select JSON file", Application.dataPath + "/Levels", "json");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                importFilePath = selectedPath;
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUI.enabled = !string.IsNullOrEmpty(importFilePath) && File.Exists(importFilePath);
        if (GUILayout.Button("Import from JSON", GUILayout.Height(30)))
        {
            ImportLevelFromJSON(exporter);
        }
        GUI.enabled = true;

        if (!string.IsNullOrEmpty(importFilePath) && !File.Exists(importFilePath))
        {
            EditorGUILayout.HelpBox("Selected file does not exist!", MessageType.Error);
        }
    }

    private void ExportLevelToJSON(LevelJSONExportImporter exporter)
    {
        string json = exporter.ToJSON();
        if (string.IsNullOrEmpty(json))
        {
            EditorUtility.DisplayDialog("Export Error", "Failed to export level data. Make sure SplineContainer is attached and has data.", "OK");
            return;
        }

        string filename = exportFilename;
        if (!filename.EndsWith(".json"))
        {
            filename += ".json";
        }

        var projectRoot = Application.dataPath;
        var levelsDir = Path.Combine(projectRoot, "Levels");
        if (!Directory.Exists(levelsDir))
        {
            Directory.CreateDirectory(levelsDir);
        }

        var fullPath = Path.Combine(levelsDir, filename);

        try
        {
            File.WriteAllText(fullPath, json);
            AssetDatabase.Refresh();

            Debug.Log($"Level exported successfully to: {fullPath}");
            EditorUtility.DisplayDialog("Export Successful", $"Level data exported to:\n{fullPath}", "OK");

            exportFilename = $"level-{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export level: {e.Message}");
            EditorUtility.DisplayDialog("Export Error", $"Failed to save file:\n{e.Message}", "OK");
        }
    }

    private void ImportLevelFromJSON(LevelJSONExportImporter exporter)
    {
        try
        {
            string json = File.ReadAllText(importFilePath);
            exporter.FromJSON(json);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log($"Level imported successfully from: {importFilePath}");
            EditorUtility.DisplayDialog("Import Successful", $"Level data imported from:\n{Path.GetFileName(importFilePath)}", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to import level: {e.Message}");
            EditorUtility.DisplayDialog("Import Error", $"Failed to load file:\n{e.Message}", "OK");
        }
    }
}

#endif