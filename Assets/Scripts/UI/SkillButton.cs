using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SkillButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Placement")]
    [SerializeField] private SkillPlacementSystem skillPlacementSystem;
    [SerializeField] private GameObject skillPrefab;
    [SerializeField] private int hotkeyIndex = -1;

    [Header("Cooldown")]
    [SerializeField] private Image[] cooldownImages;
    [SerializeField] private float pulseScale = 1.25f;
    [SerializeField] private float pulseSpeed = 5f;

    private CanvasGroup canvasGroup;
    private Vector3 originalScale;

    private bool isEnabled = true;
    private bool isCoolingDown = false;

    public bool CanPlaceSkill => isEnabled && !isCoolingDown;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalScale = transform.localScale;

        foreach (var img in cooldownImages)
            img.fillAmount = 0f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEnabled || isCoolingDown) return;
        skillPlacementSystem.BeginPlacement(skillPrefab, hotkeyIndex);
    }

    public void Enable(bool enable)
    {
        isEnabled = enable;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        bool active = isEnabled && !isCoolingDown;
        canvasGroup.alpha = active ? 1f : 0.5f;
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }

    public void UpdateCooldownVisual(float progress)
    {
        int segmentCount = cooldownImages.Length;
        float perSegment = 1f / segmentCount;
        for (int i = 0; i < segmentCount; i++)
        {
            float fill = Mathf.Clamp01((progress - i * perSegment) / perSegment);
            cooldownImages[i].fillAmount = Mathf.Clamp01(fill);
        }

        if (progress == 1)
        {
            for (int i = 0; i < cooldownImages.Count(); i++)
                cooldownImages[i].fillAmount = 0f;
        }
    }

    public void SetCoolingDown(bool cooling)
    {
        isCoolingDown = cooling;
        UpdateVisualState();
    }

    public void PlayPulse()
    {
        StopAllCoroutines();
        StartCoroutine(PulseAnimation());
    }

    private IEnumerator PulseAnimation()
    {
        float t = 0f;
        Vector3 target = originalScale * pulseScale;

        while (t < 1f)
        {
            t += Time.deltaTime * pulseSpeed;
            transform.localScale = Vector3.Lerp(originalScale, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * pulseSpeed;
            transform.localScale = Vector3.Lerp(target, originalScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
