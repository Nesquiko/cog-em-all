using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;
using NUnit.Framework;

[CreateAssetMenu(fileName = "OperationLevelCatalog", menuName = "Scriptable Objects/OperationLevelCatalog")]
public class OperationLevelCatalog : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public int operationIndex;
        public string levelFileName; // relative to StreamingAssets/Levels, e.g. "testing-level.json"
    }

    [SerializeField] private List<Entry> entries = new();

    // Hidden runtime cache (not serialized)
    [NonSerialized] private Dictionary<int, string> catalog;

    public IReadOnlyList<Entry> Entries => entries;


    private void OnEnable() => BuildCatalog();

    private void BuildCatalog()
    {
        if (catalog != null) return;

        catalog = new Dictionary<int, string>(entries.Count);
        foreach (var e in entries)
        {
            catalog[e.operationIndex] = e.levelFileName;
        }
    }

    public string GetLevelFileNameByOperationIndex(int operationIndex)
    {
        BuildCatalog();
        Assert.IsTrue(
            catalog.ContainsKey(operationIndex),
            $"Operation index '{operationIndex}' not found in '{name}' catalog."
        );

        string fileName = catalog[operationIndex];
        Assert.IsFalse(
            string.IsNullOrWhiteSpace(fileName),
            $"Catalog '{name}' has empty level filename for operation index '{operationIndex}'."
        );

        return fileName;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(OperationLevelCatalog))]
public class OperationLevelCatalogEditor : Editor
{
    private SerializedProperty entriesProp;

    private string[] levelOptions = Array.Empty<string>();

    private void OnEnable()
    {
        entriesProp = serializedObject.FindProperty("entries");
        RefreshLevelOptions();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Operation Level Catalog", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Maps Operation Index -> Level JSON filename.\n" +
            "Level files are taken from StreamingAssets/Levels.",
            MessageType.Info
        );

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Refresh Level Files"))
            {
                RefreshLevelOptions();
            }

            if (GUILayout.Button("Sort By Index"))
            {
                SortEntriesByIndex();
            }
        }

        EditorGUILayout.Space(6);

        DrawEntriesList();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void RefreshLevelOptions()
    {
        string dir = Path.Combine(Application.streamingAssetsPath, "Levels");

        if (!Directory.Exists(dir))
        {
            levelOptions = Array.Empty<string>();
            return;
        }

        levelOptions = Directory
            .GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .OrderBy(x => x)
            .ToArray();
    }

    private void DrawEntriesList()
    {
        if (GUILayout.Button("Add Entry", GUILayout.Height(22)))
        {
            entriesProp.arraySize++;
            var el = entriesProp.GetArrayElementAtIndex(entriesProp.arraySize - 1);
            el.FindPropertyRelative("operationIndex").intValue = 0;
            el.FindPropertyRelative("levelFileName").stringValue =
                levelOptions.Length > 0 ? levelOptions[0] : string.Empty;
        }

        if (entriesProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No entries yet.", MessageType.None);
            return;
        }

        for (int i = 0; i < entriesProp.arraySize; i++)
        {
            SerializedProperty entryProp = entriesProp.GetArrayElementAtIndex(i);
            var indexProp = entryProp.FindPropertyRelative("operationIndex");
            var fileProp = entryProp.FindPropertyRelative("levelFileName");

            EditorGUILayout.BeginVertical("box");

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Entry {i + 1}", EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    entriesProp.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndVertical();
                    break;
                }
            }

            indexProp.intValue = EditorGUILayout.IntField(
                new GUIContent("Operation Index"),
                Mathf.Max(0, indexProp.intValue)
            );

            DrawLevelFilePopup(fileProp);

            // quick duplicate warning
            if (HasDuplicateIndex(indexProp.intValue, i))
            {
                EditorGUILayout.HelpBox(
                    $"Duplicate operation index: {indexProp.intValue}. " +
                    "The last one will win when building the dictionary.",
                    MessageType.Warning
                );
            }

            EditorGUILayout.EndVertical();
        }

        if (levelOptions.Length == 0)
        {
            EditorGUILayout.HelpBox(
                $"No .json files found in:\n{Path.Combine(Application.streamingAssetsPath, "Levels")}\n\n" +
                "Create Assets/StreamingAssets/Levels and add your level json files there.",
                MessageType.Warning
            );
        }
    }

    private void DrawLevelFilePopup(SerializedProperty fileProp)
    {
        if (levelOptions == null || levelOptions.Length == 0)
        {
            // fallback to manual string field
            EditorGUILayout.PropertyField(fileProp, new GUIContent("Level File"));
            return;
        }

        string current = fileProp.stringValue ?? string.Empty;
        int currentIndex = Array.IndexOf(levelOptions, current);
        if (currentIndex < 0) currentIndex = 0;

        int newIndex = EditorGUILayout.Popup("Level File", currentIndex, levelOptions);
        fileProp.stringValue = levelOptions[newIndex];
    }

    private bool HasDuplicateIndex(int index, int selfArrayIndex)
    {
        for (int i = 0; i < entriesProp.arraySize; i++)
        {
            if (i == selfArrayIndex) continue;

            var other = entriesProp.GetArrayElementAtIndex(i);
            var otherIndex = other.FindPropertyRelative("operationIndex").intValue;

            if (otherIndex == index) return true;
        }

        return false;
    }

    private void SortEntriesByIndex()
    {
        // Sort via managed copy (simple + safe for small lists)
        var list = new List<(int index, string file)>();

        for (int i = 0; i < entriesProp.arraySize; i++)
        {
            var el = entriesProp.GetArrayElementAtIndex(i);
            list.Add(
                (
                    el.FindPropertyRelative("operationIndex").intValue,
                    el.FindPropertyRelative("levelFileName").stringValue
                )
            );
        }

        list = list.OrderBy(x => x.index).ThenBy(x => x.file).ToList();

        for (int i = 0; i < list.Count; i++)
        {
            var el = entriesProp.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("operationIndex").intValue = list[i].index;
            el.FindPropertyRelative("levelFileName").stringValue = list[i].file;
        }
    }
}
#endif