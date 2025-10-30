using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TowerInfo : MonoBehaviour
{
    [SerializeField] private TowerDataCatalog towerDataCatalog;

    [Header("References")]
    [SerializeField] private TMP_Text towerTitle;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text rangeText;
    [SerializeField] private TMP_Text fireRateText;
    [SerializeField] private TMP_Text critChanceText;
    [SerializeField] private TMP_Text sellPriceText;

    [Header("Animation")]
    [SerializeField, Range(0.05f, 1f)] private float fadeDuration = 0.15f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;
    private bool isVisible = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
        canvasGroup.alpha = 0f;
    }

    public void Show(TowerTypes towerType)
    {
        UpdateTowerInfo(towerType);

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        gameObject.SetActive(true);
        fadeRoutine = StartCoroutine(FadeCanvas(1f));
        isVisible = true;
    }

    public void Hide()
    {
        if (!isVisible) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeCanvas(0f, disableOnEnd: true));
        isVisible = false;
    }

    public void UpdateTowerInfo(TowerTypes towerType)
    {
        TowerData data = towerDataCatalog.FromType(towerType);

        towerTitle.text = data.displayName;
        damageText.text = data.damage.ToString();
        rangeText.text = data.range.ToString();
        fireRateText.text = data.fireRate.ToString();
        critChanceText.text = $"{data.critChance}%";
        sellPriceText.text = data.sellPrice.ToString();
    }

    private IEnumerator FadeCanvas(float targetAlpha, bool disableOnEnd = false)
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

        if (disableOnEnd && Mathf.Approximately(targetAlpha, 0f))
        {
            gameObject.SetActive(false);
        }
    }
}
