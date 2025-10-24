using UnityEngine;

[CreateAssetMenu(
    fileName = "Cursor Settings", 
    menuName = "Scriptable Objects/Cursor Settings"
)]
public class CursorSettings : ScriptableObject
{
    public Texture2D defaultCursor;
    public Texture2D hoverCursor;
    public Vector2 hotspot = Vector2.zero;
}
