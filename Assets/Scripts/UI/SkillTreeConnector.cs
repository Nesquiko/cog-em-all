using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Connection
{
    public RectTransform from;
    public RectTransform to;
}

public class SkillTreeConnector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform treeContainer;
    [SerializeField] private RectTransform connectionLayer;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private string linePrefix;

    [SerializeField] private List<Connection> connections = new();

    private void Start()
    {
        StartCoroutine(DeferredGeneration());
    }

    private IEnumerator DeferredGeneration()
    {
        yield return null;
        GenerateConnections();
    }

    [ContextMenu("Generate Lines")]
    public void GenerateConnections()
    {
        ClearConnections();

        foreach (var c in connections)
        {
            if (c.from == null || c.to == null) continue;
            DrawConnection(c.from, c.to);
        }
    }

    private void DrawConnection(RectTransform from, RectTransform to)
    {
        var lineObj = Instantiate(connectionPrefab, connectionLayer);
        lineObj.name = $"{linePrefix}_{from.name}_{to.name}";

        var line = lineObj.GetComponent<Image>();
        var rt = line.rectTransform;

        Vector2 start = connectionLayer.InverseTransformPoint(from.position);
        Vector2 end = connectionLayer.InverseTransformPoint(to.position);

        Vector2 direction = end - start;
        float length = direction.magnitude;

        rt.sizeDelta = new(length, 4f);
        rt.anchoredPosition = start + direction * 0.5f;
        rt.rotation = Quaternion.Euler(0, 0,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void ClearConnections()
    {
        for (int i = connectionLayer.childCount - 1; i >= 0; i--)
        {
            var c = connectionLayer.GetChild(i);
            if (c.name.StartsWith(linePrefix))
                DestroyImmediate(c.gameObject);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SkillTreeConnector))]
    private class SkillTreeConnectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);
            if (GUILayout.Button("Generate Connections"))
            {
                var connector = (SkillTreeConnector)target;
                connector.GenerateConnections();

                UnityEditor.EditorUtility.SetDirty(connector);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Clear Connections"))
            {
                var connector = (SkillTreeConnector)target;
                connector.ClearConnections();

                UnityEditor.EditorUtility.SetDirty(connector);
            }
        }
    }
#endif
}