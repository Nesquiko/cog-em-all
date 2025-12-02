using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeConnector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform[] ranks;

    [ContextMenu("Generate Connections")]
    public void GenerateConnections()
    {
        foreach (var rank in ranks)
        {
            for (int i = rank.childCount - 1; i >= 0; i--)
            {
                var node = rank.GetChild(i);
                node.GetComponent<SkillTreeNodeButton>().GenerateConnections();
            }
        }
    }

    [ContextMenu("Clear Connections")]
    private void ClearConnections()
    {
        foreach (var rank in ranks)
        {
            for (int i = rank.childCount - 1; i >= 0; i--)
            {
                var node = rank.GetChild(i);
                node.GetComponent<SkillTreeNodeButton>().ClearConnections();
            }
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