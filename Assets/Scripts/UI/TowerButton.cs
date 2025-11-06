using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class TowerButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private float dragThreshold = 10f;
    [SerializeField] private TowerSelectionManager towerSelectionManager;

    private CanvasGroup canvasGroup;
    private bool isPressing;
    private Vector2 pressPosition;
    private bool draggedEnough;

    private bool isEnabled = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEnabled) return;
        towerPlacementSystem.BeginPlacement(towerPrefab);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isEnabled) return;
        pressPosition = Mouse.current.position.ReadValue();
        isPressing = true;
        draggedEnough = false;

        towerSelectionManager.DisableSelection();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isEnabled) return;
        if (draggedEnough && towerPlacementSystem.IsPlacing)
        {
            towerPlacementSystem.TryPlaceAtMouse();
        }

        isPressing = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1;

        towerSelectionManager.EnableSelection();
    }

    private void Update()
    {
        if (!isEnabled) return;
        if (!isPressing) return;

        Vector2 currentPosition = Mouse.current.position.ReadValue();
        float distance = Vector2.Distance(currentPosition, pressPosition);

        if (distance >= dragThreshold && !draggedEnough)
        {
            draggedEnough = true;

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.7f;

            towerPlacementSystem.BeginPlacement(towerPrefab);
        }
    }

    public void Enable(bool enable)
    {
        isEnabled = enable;
        canvasGroup.alpha = enable ? 1f : 0.5f;
    }
}
