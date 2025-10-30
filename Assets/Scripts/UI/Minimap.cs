using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class Minimap : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private CameraInputSystem cameraInputSystem;
    [SerializeField] private PointerEventData.InputButton camereMoveTriggerMouseButton = PointerEventData.InputButton.Right;
    private RawImage minimapImage;

    private bool isDraggingOverMinimap;

    private void Awake()
    {
        minimapImage = GetComponent<RawImage>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != camereMoveTriggerMouseButton) return;
        MoveCameraFromPointer(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != camereMoveTriggerMouseButton) return;
        isDraggingOverMinimap = true;

        MoveCameraFromPointer(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != camereMoveTriggerMouseButton) return;
        isDraggingOverMinimap = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggingOverMinimap) return;
        MoveCameraFromPointer(eventData);
    }

    private void MoveCameraFromPointer(PointerEventData eventData)
    {
        bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapImage.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 local
        );
        Assert.IsTrue(ok, "failed to convert screen to local point on minimap.");

        // local-to-rect UV (0..1)
        Rect rect = minimapImage.rectTransform.rect;
        float u = Mathf.InverseLerp(rect.xMin, rect.xMax, local.x);
        float v = Mathf.InverseLerp(rect.yMin, rect.yMax, local.y);

        // Apply RawImage uvRect (cropping/tiling)
        Rect uvRect = minimapImage.uvRect;
        float uvX = uvRect.x + u * uvRect.width;
        float uvY = uvRect.y + v * uvRect.height;

        // Convert to texture pixel space
        Texture tex = minimapImage.texture;
        if (tex == null) return;

        int px = Mathf.FloorToInt(uvX * tex.width);
        int py = Mathf.FloorToInt(uvY * tex.height);

        cameraInputSystem.MoveCameraInViewableZone(new Vector2(px, py));
    }
}