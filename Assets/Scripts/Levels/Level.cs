// Insipiration taken from https://www.youtube.com/watch?v=YJk66V-jnsU
using UnityEngine.Splines;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Linq;

[DisallowMultipleComponent]
[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(SplineMeshTools.Core.SplineMesh))]
[RequireComponent(typeof(Orchestrator))]
public class Level : MonoBehaviour
{

    [SerializeField]
    private Orchestrator orchestrator;

    [Header("Level JSON (relative to Assets/Levels)")]
    [SerializeField]
    private string levelFileName = "testing-level.json";

    private SerializableLevel data = new();
    private SplineContainer splineContainer;
    private SplineMeshTools.Core.SplineMesh splineMesh;

    void OnValidate()
    {
        if (splineContainer == null)
        {
            splineContainer = GetComponent<SplineContainer>();
        }

        if (splineMesh == null)
        {
            splineMesh = GetComponent<SplineMeshTools.Core.SplineMesh>();
        }
    }

    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
    }

    private void Start()
    {
        var operationData = OperationDataDontDestroy.GetOrReadDev();

        Debug.Log(
            $"operation with faction {operationData.Faction} with these slugs: "
            + string.Join(", ", operationData.Modifiers.Select(m =>
            {
                var ranks = m is IRankedModifier r ? r.CurrentRanks() : 1;
                return $"{m.slug} (ranks: {ranks})";
            }))
            + " | abilityModifiers: "
            + string.Join(", ", operationData.AbilityModifiersSet.Select(a => a.ToString())
        ));

        LoadLevelFromFile(levelFileName);
        StartCoroutine(orchestrator.RunLevel(data, splineContainer, operationData));
    }

    private void LoadLevelFromFile(string fileName)
    {
        string fullPath = GetLevelsFullPath(fileName);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"JSON file not found: {fullPath}");
            return;
        }

        string json = File.ReadAllText(fullPath);
        var loaded = SerializableLevel.FromJson(json);
        if (loaded == null)
        {
            Debug.LogError($"failed to deserialize JSON: {fullPath}");
            return;
        }
        else
        {
            data = loaded;
        }

        ApplySplinesToScene();
    }

    public string ToJson()
    {
        SyncSplinesFromScene();
        return SerializableLevel.ToJson(data);
    }

    public void LoadFromJson(string json)
    {
        var loaded = SerializableLevel.FromJson(json);
        if (loaded == null)
        {
            Debug.LogError("failed to deserialize level JSON.");
            return;
        }

        data = loaded;
        ApplySplinesToScene();
    }

    public void ApplySplinesToScene()
    {
        while (splineContainer.Splines.Count > 0)
        {
            splineContainer.RemoveSplineAt(0);
        }

        if (data.pathSplines != null)
        {
            foreach (var spline in data.pathSplines)
            {
                splineContainer.AddSpline(spline);
            }
        }

        splineMesh.GenerateMeshAlongSpline();
        MeshCollider col = GetComponent<MeshCollider>();
        if (col) col.sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }

    public void SyncSplinesFromScene()
    {
        data.pathSplines = new List<Spline>(splineContainer.Splines);
    }

    private string GetLevelsFullPath(string fileName)
    {
        // Relative to Assets/Levels
        string projectAssets = Application.dataPath;
        return Path.Combine(projectAssets, "Levels", fileName);
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(Level))]
public class LevelEditorInspector : Editor
{
    private SerializedProperty orchestratorProp;
    private SerializedProperty levelFileNameProp;
    private Level level;
    private Vector2 scroll;

    private void OnEnable()
    {
        level = (Level)target;
        levelFileNameProp = serializedObject.FindProperty("levelFileName");
        orchestratorProp = serializedObject.FindProperty("orchestrator");
    }

    public override void OnInspectorGUI()
    {
        // Header
        EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This inspector lets you edit waves, import/export JSON, and sync splines. ", MessageType.Info);

        EditorGUILayout.PropertyField(orchestratorProp, new GUIContent("Level orchestrator"));

        GUILayout.Space(5);
        DrawLevelFileField();

        GUILayout.Space(5);
        DrawImportExportButtons();

        GUILayout.Space(10);
        DrawSplineSyncButtons();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Player Resources", EditorStyles.boldLabel);
        DrawPlayerResourcesEditor();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Dev Settings", EditorStyles.boldLabel);
        DrawDevSettingsEditor();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Waves Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(3);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawWavesEditor();
        EditorGUILayout.EndScrollView();

        serializedObject.ApplyModifiedProperties();

        // Ensure changes are persisted
        if (GUI.changed)
        {
            EditorUtility.SetDirty(level);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            );
        }
    }

    private void DrawLevelFileField()
    {
        EditorGUILayout.BeginHorizontal();
        if (levelFileNameProp != null)
        {
            EditorGUILayout.PropertyField(levelFileNameProp, new GUIContent("File Name"));
        }
        else
        {
            EditorGUILayout.LabelField("File Name property not found on Level.");
        }

        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string initialDir = Path.Combine(Application.dataPath, "Levels");
            string selectedPath = EditorUtility.OpenFilePanel("Select JSON file", initialDir, "json");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                string assetsPath = Application.dataPath.Replace('\\', '/');
                string levelsDir = (assetsPath + "/Levels/").Replace('\\', '/');
                selectedPath = selectedPath.Replace('\\', '/');

                if (selectedPath.StartsWith(levelsDir))
                {
                    string relativeFile = selectedPath.Substring(levelsDir.Length);
                    if (levelFileNameProp != null)
                    {
                        levelFileNameProp.stringValue = relativeFile;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Location", "Please choose a file inside Assets/Levels.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawImportExportButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Import JSON", GUILayout.Height(24)))
        {
            ImportLevelFromFile();
        }

        if (GUILayout.Button("Export JSON", GUILayout.Height(24)))
        {
            ExportLevelToFile();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawSplineSyncButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Sync Splines From Scene", GUILayout.Height(22)))
        {
            level.SyncSplinesFromScene();
            EditorUtility.SetDirty(level);
        }

        if (GUILayout.Button("Apply Splines To Scene", GUILayout.Height(22)))
        {
            level.ApplySplinesToScene();
            EditorUtility.SetDirty(level);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawWavesEditor()
    {
        // We will access Level's SerializableLevel through JSON roundtrip to avoid private field access.
        // Get a mutable copy of data via JSON, edit it, then push back if changed.

        string beforeJson = level.ToJson();
        SerializableLevel temp = SerializableLevel.FromJson(beforeJson) ?? new SerializableLevel();

        // Waves list
        if (temp.waves == null)
            temp.waves = new List<Wave>();

        if (GUILayout.Button("Add Wave", GUILayout.Height(22)))
        {
            temp.waves.Add(new Wave
            {
                enabled = true,
                prepareTimeSeconds = 0f,
                spawnGroups = new List<SpawnGroup>()
            });
        }

        int removeWaveAt = -1;

        for (int w = 0; w < temp.waves.Count; w++)
        {
            var wave = temp.waves[w] ?? new Wave();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Wave {w + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
                removeWaveAt = w;
            EditorGUILayout.EndHorizontal();

            wave.enabled = EditorGUILayout.Toggle("Enabled", wave.enabled);
            wave.prepareTimeSeconds = EditorGUILayout.FloatField("Prepare Time (s)", wave.prepareTimeSeconds);

            // Groups
            if (wave.spawnGroups == null)
                wave.spawnGroups = new List<SpawnGroup>();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Spawn Groups", EditorStyles.miniBoldLabel);

            if (GUILayout.Button("Add Group"))
            {
                wave.spawnGroups.Add(new SpawnGroup
                {
                    repeat = 1,
                    spawnRateSeconds = 0.2f,
                    pauseAfterLastSpawnSeconds = 0f,
                    pattern = new List<PatternEntry>()
                });
            }

            int removeGroupAt = -1;
            for (int g = 0; g < wave.spawnGroups.Count; g++)
            {
                var group = wave.spawnGroups[g] ?? new SpawnGroup();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Group {g + 1}", EditorStyles.boldLabel);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    removeGroupAt = g;
                EditorGUILayout.EndHorizontal();

                group.repeat = EditorGUILayout.IntField("Repeat", Mathf.Max(0, group.repeat));
                group.spawnRateSeconds = EditorGUILayout.FloatField("Pattern Delay (s)", Mathf.Max(0f, group.spawnRateSeconds));
                group.pauseAfterLastSpawnSeconds = EditorGUILayout.FloatField("Pause After Group (s)", Mathf.Max(0f, group.pauseAfterLastSpawnSeconds));

                // Pattern
                group.pattern ??= new List<PatternEntry>();

                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("Pattern", EditorStyles.miniBoldLabel);

                if (GUILayout.Button("Add Entry"))
                {
                    group.pattern.Add(new PatternEntry { enemy = EnemyType.Bandit, count = 1, spawnRateSeconds = 0f });
                }

                int removeEntryAt = -1;
                for (int p = 0; p < group.pattern.Count; p++)
                {
                    var entry = group.pattern[p] ?? new PatternEntry();

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Entry {p + 1}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                        removeEntryAt = p;
                    EditorGUILayout.EndHorizontal();

                    entry.enemy = (EnemyType)EditorGUILayout.EnumPopup("Enemy", entry.enemy);
                    entry.count = EditorGUILayout.IntField("Count", Mathf.Max(0, entry.count));
                    entry.spawnRateSeconds = EditorGUILayout.FloatField("Per-Enemy Delay (s)", Mathf.Max(0f, entry.spawnRateSeconds));

                    group.pattern[p] = entry;
                    EditorGUILayout.EndVertical();
                }

                if (removeEntryAt >= 0 && removeEntryAt < group.pattern.Count)
                    group.pattern.RemoveAt(removeEntryAt);

                wave.spawnGroups[g] = group;
                EditorGUILayout.EndVertical();
            }

            if (removeGroupAt >= 0 && removeGroupAt < wave.spawnGroups.Count)
                wave.spawnGroups.RemoveAt(removeGroupAt);

            temp.waves[w] = wave;
            EditorGUILayout.EndVertical();
        }

        if (removeWaveAt >= 0 && removeWaveAt < temp.waves.Count)
            temp.waves.RemoveAt(removeWaveAt);

        string afterJson = SerializableLevel.ToJson(temp);
        if (afterJson != beforeJson)
        {
            level.LoadFromJson(afterJson);
            EditorUtility.SetDirty(level);
        }
    }

    private string GetLevelsFullPath(string fileName)
    {
        string projectAssets = Application.dataPath;
        return Path.Combine(projectAssets, "Levels", fileName);
    }

    private string GetLevelFileName()
    {
        if (levelFileNameProp != null)
            return levelFileNameProp.stringValue;

        return "testsing-level.json";
    }

    private void SetLevelFileName(string fileName)
    {
        levelFileNameProp.stringValue = fileName;
        serializedObject.ApplyModifiedProperties();
    }

    private void ImportLevelFromFile()
    {
        string fileName = GetLevelFileName();
        string fullPath = GetLevelsFullPath(fileName);

        if (!File.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("Import Error", $"File not found:\n{fullPath}", "OK");
            return;
        }

        try
        {
            string json = File.ReadAllText(fullPath);
            level.LoadFromJson(json);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            );

            Debug.Log($"[LevelEditor] Imported level from: {fullPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LevelEditor] Failed to import level: {e.Message}");
            EditorUtility.DisplayDialog("Import Error", e.Message, "OK");
        }
    }

    private void ExportLevelToFile()
    {
        string fileName = GetLevelFileName();
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = "level-" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".json";
            SetLevelFileName(fileName);
        }

        string fullPath = GetLevelsFullPath(fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        try
        {
            level.SyncSplinesFromScene();

            string json = level.ToJson();
            File.WriteAllText(fullPath, json);
            AssetDatabase.Refresh();

            Debug.Log($"exported level to: {fullPath}");
            EditorUtility.DisplayDialog("export Successful", fullPath, "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"failed to export level: {e.Message}");
            EditorUtility.DisplayDialog("export Error", e.Message, "OK");
        }
    }


    private void DrawPlayerResourcesEditor()
    {
        string beforeJson = level.ToJson();
        SerializableLevel temp = SerializableLevel.FromJson(beforeJson) ?? new SerializableLevel();

        if (temp.playerResources == null)
            temp.playerResources = new PlayerResources();

        EditorGUILayout.BeginVertical("box");

        int newInitialGears = EditorGUILayout.IntField(
            new GUIContent("Initial Gears", "Initial number of gears at the start of the level"),
            temp.playerResources.initialGears
        );

        if (!Mathf.Approximately(newInitialGears, temp.playerResources.initialGears))
        {
            temp.playerResources.initialGears = newInitialGears;

            string afterJson = SerializableLevel.ToJson(temp);
            if (afterJson != beforeJson)
            {
                level.LoadFromJson(afterJson);
                EditorUtility.SetDirty(level);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                );
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawDevSettingsEditor()
    {
        string beforeJson = level.ToJson();
        SerializableLevel temp = SerializableLevel.FromJson(beforeJson) ?? new SerializableLevel();

        temp.devSettings ??= new DevSettings();

        EditorGUILayout.BeginVertical("box");

        bool newUnkillable = EditorGUILayout.Toggle(
            new GUIContent(
                "Unkillable Enemies",
                "If enabled, enemies should not be killable (dev/testing)."
            ),
            temp.devSettings.unkillableEnemies
        );

        if (newUnkillable != temp.devSettings.unkillableEnemies)
        {
            temp.devSettings.unkillableEnemies = newUnkillable;

            string afterJson = SerializableLevel.ToJson(temp);
            if (afterJson != beforeJson)
            {
                level.LoadFromJson(afterJson);
                EditorUtility.SetDirty(level);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                );
            }
        }

        EditorGUILayout.EndVertical();
    }
}
#endif