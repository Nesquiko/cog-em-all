using UnityEngine;
using UnityEngine.UI;

public class ScrollProgress : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Slider scrollSlider;

    private void OnEnable()
    {
        scrollRect.onValueChanged.AddListener(OnScrollChanged);
        UpdateTargetFill();
    }

    private void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
    }

    private void OnScrollChanged(Vector2 scrollPosition)
    {
        UpdateTargetFill();
    }

    private void UpdateTargetFill()
    {
        float contentHeight = scrollRect.content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;

        if (contentHeight <= viewportHeight)
        {
            scrollSlider.value = 1f;
            return;
        }

        scrollSlider.value = 1f - scrollRect.verticalNormalizedPosition;
    }
}
