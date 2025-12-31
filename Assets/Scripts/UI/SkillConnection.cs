using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillConnection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image background;
    [SerializeField] private Image fill;
    [SerializeField] private Image foreground;

    [SerializeField] private float thickness = 12f;
    [SerializeField] private float fillDuration = 0.35f;

    private Coroutine fillRoutine;

    public void Build(RectTransform from, RectTransform to, RectTransform layer)
    {
        Vector2 start = layer.InverseTransformPoint(from.position);
        Vector2 end = layer.InverseTransformPoint(to.position);

        Vector2 direction = end - start;
        float length = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        ApplyTransform(start, direction, length, angle);
    }

    public void SetFilledInstant(bool filled)
    {
        StopFill();
        fill.fillAmount = filled ? 1f : 0f;
    }

    public void AnimateFill(bool filled)
    {
        StopFill();
        fillRoutine = StartCoroutine(
            FillRoutine(filled ? 1f : 0f)
        );
    }

    private void StopFill()
    {
        if (fillRoutine == null) return;
        StopCoroutine(fillRoutine);
    }

    private void ApplyTransform(
        Vector2 start,
        Vector2 direction,
        float length,
        float angle
    )
    {
        Vector2 size = new(length, thickness);
        Vector2 position = start + direction * 0.5f;

        ApplyTo(background.rectTransform, size, position, angle);
        ApplyTo(fill.rectTransform, size, position, angle);
        ApplyTo(foreground.rectTransform, size, position, angle);
    }

    private void ApplyTo(
        RectTransform rt,
        Vector2 size,
        Vector2 position,
        float angle
    )
    {
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        rt.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private IEnumerator FillRoutine(float target)
    {
        float start = fill.fillAmount;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fillDuration;
            fill.fillAmount = Mathf.SmoothStep(start, target, t);
            yield return null;
        }

        fill.fillAmount = target;
        fillRoutine = null;
    }
}
