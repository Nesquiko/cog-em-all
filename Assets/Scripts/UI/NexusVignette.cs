using System.Collections;
using UnityEngine;

public class NexusVignette : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup vignette;

    [Header("Settings")]
    [SerializeField] private float lowHealthThreshold = 0.25f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float maxAlpha = 0.6f;
    [SerializeField] private float pulseAmount = 0.3f;
    [SerializeField] private float pulseDuration = 0.25f;

    private Nexus nexus;
    private float targetAlpha;
    private Coroutine pulseRoutine;

    public void Initialize(Nexus nexusRef)
    {
        nexus = nexusRef;
        nexus.OnHealthChanged += HandleHealthChange;

        vignette.alpha = 0f;
    }

    private void OnDestroy()
    {
        if (nexus != null)
            nexus.OnHealthChanged -= HandleHealthChange;
    }

    private void Update()
    {
        vignette.alpha = Mathf.Lerp(vignette.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }

    private void HandleHealthChange(Nexus n, float _)
    {
        float normalized = n.HealthPointsNormalized();

        if (normalized < lowHealthThreshold)
        {
            targetAlpha = Mathf.Lerp(0f, maxAlpha, 1f - normalized / lowHealthThreshold);

            if (pulseRoutine != null)
                StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(Pulse());
        }
        else
        {
            targetAlpha = 0f;
        }
    }

    private IEnumerator Pulse()
    {
        float startAlpha = vignette.alpha;
        float peakAlpha = Mathf.Min(startAlpha + pulseAmount, 1f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / (pulseDuration * 0.5f);
            vignette.alpha = Mathf.Lerp(startAlpha, peakAlpha, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / (pulseDuration * 0.5f);
            vignette.alpha = Mathf.Lerp(peakAlpha, targetAlpha, t);
            yield return null;
        }

        vignette.alpha = targetAlpha;
    }
}
