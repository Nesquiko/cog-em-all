using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class TowerButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private float dragThreshold = 10f;

    private CanvasGroup canvasGroup;
    private bool isPressing;
    private Vector2 pressPosition;
    private bool draggedEnough;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TowerPlacementSystem.Instance.BeginPlacement(towerPrefab);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressPosition = Mouse.current.position.ReadValue();
        isPressing = true;
        draggedEnough = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (draggedEnough && TowerPlacementSystem.Instance.IsPlacing)
        {
            TowerPlacementSystem.Instance.TryPlaceAtMouse();
        }

        isPressing = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1;
    }

    private void Update()
    {
        if (!isPressing) return;

        Vector2 currentPosition = Mouse.current.position.ReadValue();
        float distance = Vector2.Distance(currentPosition, pressPosition);

        if (distance >= dragThreshold && !draggedEnough)
        {
            draggedEnough = true;

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.7f;

            TowerPlacementSystem.Instance.BeginPlacement(towerPrefab);
        }
    }
}
