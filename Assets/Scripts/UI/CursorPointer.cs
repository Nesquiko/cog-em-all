using UnityEngine;
using UnityEngine.EventSystems;

public class CursorPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CursorSettings cursorSettings;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(cursorSettings.hoverCursor, cursorSettings.hotspot, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(cursorSettings.defaultCursor, Vector2.zero, CursorMode.Auto);
    }
}
