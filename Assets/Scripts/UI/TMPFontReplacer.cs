#if UNITY_EDITOR
using UnityEngine;
using TMPro;
using UnityEditor;

public class TMPFontReplacer : MonoBehaviour
{
    [MenuItem("Tools/TextMeshPro/Replace Fonts in Scene")]
    public static void ReplaceFonts()
    {
        TMP_FontAsset newFont = Selection.activeObject as TMP_FontAsset;
        if (newFont == null)
        {
            Debug.LogError("Select a TMP_FontAsset in the Project window first.");
            return;
        }

        int count = 0;
        foreach (TMP_Text tmp in FindObjectsByType<TMP_Text>(FindObjectsSortMode.InstanceID))
        {
            Undo.RecordObject(tmp, "Change TMP Font");
            tmp.font = newFont;
            count++;
        }
        Debug.Log($"Replaced fonts on {count} TMP text components.");
    }
}
#endif