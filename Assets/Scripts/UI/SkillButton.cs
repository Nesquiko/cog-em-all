using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class SkillButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SkillPlacementSystem skillPlacementSystem;
    [SerializeField] private GameObject skillPrefab;
    [SerializeField] private int hotkeyIndex = -1;

    private CanvasGroup canvasGroup;

    private bool isEnabled = true;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEnabled) return;
        skillPlacementSystem.BeginPlacement(skillPrefab, hotkeyIndex);
    }

    public void Enable(bool enable)
    {
        isEnabled = enable;
        canvasGroup.alpha = enable ? 1f : 0.5f;
    }
}
