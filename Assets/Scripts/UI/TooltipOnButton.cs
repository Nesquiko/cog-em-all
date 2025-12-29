using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TooltipOnButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CanvasGroup tooltip;
    [SerializeField] private float hoverDelay = 0.5f;
    [SerializeField] private float fadeDuration = 0.3f;

    private Coroutine showRoutine;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        tooltip.alpha = 0f;
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        if (tooltip != null)
        {
            tooltip.alpha = 0f;
            tooltip.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        if (showRoutine != null) StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(ShowTooltipAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (showRoutine != null) StopCoroutine(showRoutine);
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeTooltip(tooltip.alpha, 0f, fadeDuration));
    }

    private IEnumerator ShowTooltipAfterDelay()
    {
        yield return new WaitForSecondsRealtime(hoverDelay);
        fadeRoutine = StartCoroutine(FadeTooltip(tooltip.alpha, 1f, fadeDuration));
    }

    private IEnumerator FadeTooltip(float from, float to, float duration)
    {
        float timer = 0f;
        tooltip.alpha = from;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            tooltip.alpha = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }
        tooltip.alpha = to;
    }
}
