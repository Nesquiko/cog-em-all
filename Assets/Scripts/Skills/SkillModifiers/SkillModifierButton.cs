using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillModifierButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private SkillModifiers skillModifier;
    [SerializeField] private SkillTypes skillType;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ScaleOnHover scaleOnHover;
    [SerializeField] private CursorPointer cursorPointer;
    [SerializeField] private GameObject activatedOverlay;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject tooltip;
    [SerializeField] private TMP_Text tooltipTitle;
    [SerializeField] private TMP_Text tooltipDescription;
    [SerializeField] private SkillModifierCatalog skillModifierCatalog;
    [SerializeField] private SkillModifierSystem skillModifierSystem;

    private ISkillModifier modifier;
    private bool activated = false;
    private bool locked = false;
    public bool Activated => activated;
    public bool Locked => locked;
    public SkillModifiers SkillModifier => skillModifier;

    public event Action<SkillModifiers> OnActivate;
    public void ResetOnActivate() => OnActivate = null;
    public event Action<SkillModifiers> OnDeactivate;
    public void ResetOnDeactivate() => OnDeactivate = null;

    public void Initialize(List<Modifier> modifiers)
    {
        var usagePerAbility = ModifiersCalculator.UsagePerAbility(modifiers);
        Lock(!usagePerAbility.ContainsKey(skillType));

        modifier = (ISkillModifier)skillModifierCatalog.FromSkillAndModifier(skillType, skillModifier);

        tooltip.SetActive(false);
        tooltipTitle.text = modifier.DisplayName;
        tooltipDescription.text = modifier.Description;

        UpdateVisualState();
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
        OnActivate?.Invoke(skillModifier);
        UpdateVisualState();
    }

    private void HandleRightClick()
    {
        if (locked) return;
        if (!activated) return;
        if (!skillModifierSystem.CanRefundModifierPoint) return;
        activated = false;
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
        lockedOverlay.SetActive(locked);
        scaleOnHover.enabled = !locked;
        cursorPointer.enabled = !locked;

        canvasGroup.alpha = activated ? 1f : 0.75f;
        activatedOverlay.SetActive(activated);
    }
}
