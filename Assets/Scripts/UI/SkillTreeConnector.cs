using System.Collections;
using UnityEngine;

public class SkillTreeConnector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform[] ranks;

    private IEnumerator Start()
    {
        yield return null;
        yield return null;
        yield return null;
        GenerateConnections();
    }

    [ContextMenu("Generate Connections")]
    public void GenerateConnections()
    {
        foreach (var rank in ranks)
        {
            for (int i = rank.childCount - 1; i >= 0; i--)
            {
                var node = rank.GetChild(i);
                if (!node.TryGetComponent<SkillTreeNodeButton>(out var button)) continue;
                button.GenerateConnections();
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
                if (!node.TryGetComponent<SkillTreeNodeButton>(out var button)) continue;
                button.ClearConnections();
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