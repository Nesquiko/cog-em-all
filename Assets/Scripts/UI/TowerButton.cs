using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup), typeof(CursorPointer), typeof(ScaleOnHover))]
public class TowerButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private ScaleOnHover scaleOnHover;
    [SerializeField] private CursorPointer cursorPointer;
    [SerializeField] private int hotkeyIndex = -1;

    private CanvasGroup canvasGroup;

    private bool isEnabled = false;
    private bool permanentlyDisabled = false;
    public bool IsEnabled => isEnabled && !permanentlyDisabled;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEnabled || permanentlyDisabled) return;
        towerPlacementSystem.BeginPlacement(towerPrefab, hotkeyIndex);
    }

    public void Enable(bool enable, bool permanently = false)
    {
        if (permanentlyDisabled) return;
        if (!enable && permanently)
            permanentlyDisabled = true;

        isEnabled = enable;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        bool active = isEnabled && !permanentlyDisabled;
        canvasGroup.alpha = active ? 1f : 0.5f;
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
        scaleOnHover.enabled = active;
        cursorPointer.enabled = active;
    }
}
