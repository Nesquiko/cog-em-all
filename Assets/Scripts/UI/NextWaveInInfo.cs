// using System.Collections;
// using TMPro;
// using UnityEngine;
// using UnityEngine.Assertions;

// public class NextWaveCountdownInfo : MonoBehaviour
// {
//     [SerializeField] private TMP_Text nextWaveInSecondsText;

//     public IEnumerator StartCountdown(float nextWaveInSeconds)
//     {
//         Assert.IsTrue(nextWaveInSeconds != 0f);
//         gameObject.SetActive(true);

//         var remainingSeconds = nextWaveInSeconds;
//         while (remainingSeconds > 0f)
//         {
//             nextWaveInSecondsText.text = $"{remainingSeconds}";

//             yield return new WaitForSeconds(1f);
//             remainingSeconds -= 1f;
//         }

//         gameObject.SetActive(false);
//     }
// }

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CanvasGroup))]
public class NextWaveCountdownInfo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text nextWaveInSecondsText;

    [Header("Animation")]
    [SerializeField, Range(0.05f, 1f)] private float fadeDuration = 0.15f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        Assert.IsNotNull(nextWaveInSecondsText);
    }

    public IEnumerator StartCountdown(float nextWaveInSeconds)
    {
        Assert.IsTrue(nextWaveInSeconds > 0f);

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        gameObject.SetActive(true);
        yield return FadeCanvas(1f);

        float remainingSeconds = Mathf.Ceil(nextWaveInSeconds);
        while (remainingSeconds > 0f)
        {
            nextWaveInSecondsText.text = remainingSeconds.ToString("0");
            yield return new WaitForSeconds(1f);
            remainingSeconds -= 1f;
        }

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        yield return FadeCanvas(0f);
        gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}