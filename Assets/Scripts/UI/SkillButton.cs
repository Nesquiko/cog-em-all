using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup), typeof(CursorPointer), typeof(ScaleOnHover))]
public class SkillButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Placement")]
    [SerializeField] private SkillTypes skillType;
    [SerializeField] private SkillPlacementSystem skillPlacementSystem;
    [SerializeField] private GameObject skillPrefab;
    [SerializeField] private ScaleOnHover scaleOnHover;
    [SerializeField] private CursorPointer cursorPointer;
    [SerializeField] private int hotkeyIndex = -1;

    [Header("Cooldown")]
    [SerializeField] private Image[] cooldownImages;
    [SerializeField] private float pulseScale = 1.25f;
    [SerializeField] private float pulseSpeed = 5f;

    [Header("Usage Indicators")]
    [SerializeField] private Transform usageState;
    [SerializeField] private GameObject usageIndicatorPrefab;

    private readonly List<GameObject> usageIndicators = new();

    private int maxUsages = -1;  // start unlimited
    private int remainingUsages = -1;

    private CanvasGroup canvasGroup;
    private Vector3 originalScale;

    private bool isEnabled = true;
    private bool isCoolingDown = false;
    private bool permanentlyDisabled = false;

    public bool CanPlaceSkill => isEnabled && !isCoolingDown && !permanentlyDisabled;
    public bool ShouldRunCooldown => remainingUsages > 0 && !permanentlyDisabled;

    private readonly HashSet<SkillTypes> infiniteUsageSkills = new()
    {
        SkillTypes.AirshipAirstrike, SkillTypes.AirshipFreezeZone, SkillTypes.AirshipDisableZone, SkillTypes.MarkEnemy, SkillTypes.SuddenDeath
    };

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalScale = transform.localScale;

        InitializeUsages();

        foreach (var img in cooldownImages)
            img.fillAmount = 0f;
    }

    private void InitializeUsages()
    {
        OperationDataDontDestroy operationData = OperationDataDontDestroy.GetOrReadDev();
        Dictionary<SkillTypes, int> usagePerAbility = ModifiersCalculator.UsagePerAbility(operationData.Modifiers);
        if (!usagePerAbility.TryGetValue(skillType, out var max))
        {
            maxUsages = -1;
            remainingUsages = -1;
            Enable(false, permanently: true);
            return;
        }

        maxUsages = max;
        remainingUsages = max;
        CreateUsageIndicators();
    }

    private void CreateUsageIndicators()
    {
        foreach (Transform child in usageState)
            Destroy(child.gameObject);

        usageIndicators.Clear();

        if (infiniteUsageSkills.Contains(skillType)) return;

        if (maxUsages <= 0) return;

        for (int i = 0; i < maxUsages; i++)
        {
            GameObject indicator = Instantiate(
                usageIndicatorPrefab,
                usageState
            );
            usageIndicators.Add(indicator);
        }

        UpdateUsageIndicators();
    }

    private void UpdateUsageIndicators()
    {
        if (usageIndicators.Count == 0 || infiniteUsageSkills.Contains(skillType)) return;
        if (remainingUsages == -1)
        {
            foreach (var i in usageIndicators)
            {
                var fillImage = i.transform.GetChild(0).GetComponent<Image>();
                fillImage.fillAmount = 1f;
                return;
            }
        }

        for (int i = 0; i < usageIndicators.Count; i++)
        {
            var fillImage = usageIndicators[i].transform.GetChild(0).GetComponent<Image>();
            fillImage.fillAmount = i < remainingUsages ? 1f : 0f;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEnabled || isCoolingDown || permanentlyDisabled) return;
        if (!HasRemainingUsages()) return;
        skillPlacementSystem.BeginPlacement(skillPrefab, hotkeyIndex);
    }

    private bool HasRemainingUsages() => remainingUsages == -1 || remainingUsages > 0;

    public void ConsumeUsage()
    {
        if (remainingUsages == -1) return;
        remainingUsages--;

        UpdateUsageIndicators();

        if (remainingUsages <= 0)
            Enable(false, permanently: true);
    }

    public void Enable(bool enable, bool permanently = false)
    {
        if (permanentlyDisabled) return;
        if (!enable && permanently)
            permanentlyDisabled = true;

        isEnabled = enable;

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        bool active = isEnabled && !isCoolingDown && !permanentlyDisabled;
        canvasGroup.alpha = active ? 1f : 0.5f;
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
        scaleOnHover.enabled = active;
        cursorPointer.enabled = active;

        UpdateUsageIndicators();
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
        if (permanentlyDisabled) return;
        isCoolingDown = cooling;
        UpdateVisualState();
    }

    public void PlayPulse()
    {
        if (!isActiveAndEnabled || permanentlyDisabled) return;
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
