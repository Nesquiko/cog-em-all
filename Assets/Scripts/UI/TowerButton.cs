using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class TowerButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private int hotkeyIndex = -1;

    private CanvasGroup canvasGroup;

    private bool isEnabled = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEnabled) return;
        towerPlacementSystem.BeginPlacement(towerPrefab, hotkeyIndex);
    }

    public void Enable(bool enable)
    {
        isEnabled = enable;
        canvasGroup.alpha = enable ? 1f : 0.5f;
    }
}
