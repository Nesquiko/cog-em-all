using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillModifierButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Skill Modifier")]
    [SerializeField] private SkillModifiers skillModifier;
    [SerializeField] private SkillTypes skillType;

    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private Image image;
    [SerializeField] private ScaleOnHover scaleOnHover;
    [SerializeField] private CursorPointer cursorPointer;

    [Header("Animation")]
    [SerializeField] private RectTransform rotateTarget;
    [SerializeField] private float rotationStep = 90f;
    [SerializeField] private float rotationDuration = 0.2f;
    [SerializeField] private float rotationOvershoot = 15f;

    [Header("Sprites")]
    [SerializeField] private Sprite basicSprite;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite disabledSprite;

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltip;
    [SerializeField] private TMP_Text tooltipTitle;
    [SerializeField] private TMP_Text tooltipDescription;

    [Header("References")]
    [SerializeField] private SkillModifierCatalog skillModifierCatalog;
    [SerializeField] private SkillModifierSystem skillModifierSystem;

    private ISkillModifier modifier;
    private bool activated = false;
    private bool locked = false;
    public bool Activated => activated;
    public bool Locked => locked;
    public SkillModifiers SkillModifier => skillModifier;

    public event Action<SkillModifiers> OnActivate;
    public event Action<SkillModifiers> OnDeactivate;
    public void ResetOnActivate() => OnActivate = null;
    public void ResetOnDeactivate() => OnDeactivate = null;

    private float currentRotation;
    private Coroutine rotationRoutine;

    private void Awake()
    {
        button.transition = Selectable.Transition.None;
    }

    public void Initialize(List<Modifier> modifiers)
    {
        var usagePerAbility = ModifiersCalculator.UsagePerAbility(modifiers);
        Lock(!usagePerAbility.ContainsKey(skillType));

        modifier = (ISkillModifier)skillModifierCatalog.FromSkillAndModifier(skillType, skillModifier);

        tooltip.SetActive(false);
        tooltipTitle.text = modifier.DisplayName;
        tooltipDescription.text = modifier.Description;

        UpdateVisualState();

        currentRotation = activated ? rotationStep : 0f;
        rotateTarget.localRotation = Quaternion.Euler(0f, 0f, currentRotation);
    }

    public void Lock(bool l)
    {
        locked = l;
        if (locked) activated = false;

        UpdateVisualState();
    }

    public void Activate(bool active)
    {
        if (locked) return;

        activated = active;

        UpdateVisualState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            HandleLeftClick();
        else if (eventData.button == PointerEventData.InputButton.Right)
            HandleRightClick();
    }

    private void HandleLeftClick()
    {
        if (locked) return;
        if (activated) return;
        if (!skillModifierSystem.CanUseModifierPoint) return;
        activated = true;
        Rotate(-1);
        OnActivate?.Invoke(skillModifier);
        UpdateVisualState();
    }

    private void HandleRightClick()
    {
        if (locked) return;
        if (!activated) return;
        if (!skillModifierSystem.CanRefundModifierPoint) return;
        activated = false;
        Rotate(1);
        OnDeactivate?.Invoke(skillModifier);
        UpdateVisualState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.SetActive(false);
    }

    private void UpdateVisualState()
    {
        button.interactable = !locked;
        scaleOnHover.enabled = !locked;
        cursorPointer.enabled = !locked;

        if (locked)
            image.sprite = disabledSprite;
        else if (activated)
            image.sprite = activeSprite;
        else
            image.sprite = basicSprite;
    }

    private void Rotate(int direction)
    {
        float targetRotation = currentRotation + direction * rotationStep;
    
        if (rotationRoutine != null)
            StopCoroutine(rotationRoutine);

        rotationRoutine = StartCoroutine(RotateRoutine(currentRotation, targetRotation));

        currentRotation = targetRotation;
    }

    private IEnumerator RotateRoutine(float from, float to)
    {
        float t = 0f;
        float overshootTarget = to + Mathf.Sign(to - from) * rotationOvershoot;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / rotationDuration;
            float e = Mathf.SmoothStep(0f, 1.1f, t);

            float angle = Mathf.LerpUnclamped(from, overshootTarget, e);
            rotateTarget.localRotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        t = 0f;
        float settleDuration = rotationDuration * 0.5f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / settleDuration;
            float e = Mathf.SmoothStep(0f, 1f, t);

            float angle = Mathf.LerpUnclamped(overshootTarget, to, e);
            rotateTarget.localRotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        rotateTarget.localRotation = Quaternion.Euler(0f, 0f, to);

        rotationRoutine = null;
    }
}
