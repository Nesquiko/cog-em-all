using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(CanvasGroup), typeof(CursorPointer))]
public class TowerButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private CursorPointer cursorPointer;
    [SerializeField] private int hotkeyIndex = -1;

    private CanvasGroup canvasGroup;

    private bool permanentlyDisabled = false;
    public bool IsEnabled => button.interactable && !permanentlyDisabled;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void HandleClick()
    {
        if (!button.interactable || permanentlyDisabled) return;
        towerPlacementSystem.BeginPlacement(towerPrefab, hotkeyIndex);
    }

    public void Enable(bool enable, bool permanently = false)
    {
        if (permanentlyDisabled) return;
        if (!enable && permanently)
            permanentlyDisabled = true;

        button.interactable = enable;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        bool active = button.interactable && !permanentlyDisabled;
        cursorPointer.enabled = active;
    }
}
