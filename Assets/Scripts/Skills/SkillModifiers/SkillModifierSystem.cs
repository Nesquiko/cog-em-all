using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillModifierSystem : MonoBehaviour
{
    [SerializeField] private SkillModifierButton[] modifierButtons;
    [SerializeField] private ModifiersDatabase modifiersDatabase;
    [SerializeField] private Image digitTens;
    [SerializeField] private Image digitOnes;
    [SerializeField] private FancyDigits digits;
    [SerializeField] private float pulseScale = 1.15f;
    [SerializeField] private float pulseDuration = 0.15f;

    private int maxModifierPoints;
    private int assignedModifierPoints = 0;
    private int availableModifierPoints;

    private Coroutine pulseRoutine;

    public bool CanUseModifierPoint => availableModifierPoints > 0;
    public bool CanRefundModifierPoint => assignedModifierPoints > 0;

    private SaveContextDontDestroy saveContext;
    private FactionSaveState lastPlayedFaction;

    public void Initialize()
    {
        saveContext = SaveContextDontDestroy.GetOrCreateDev();
        (_, lastPlayedFaction) = saveContext.LastFactionSaveState();
        var modifiers = modifiersDatabase.GetModifiersBySlugs(lastPlayedFaction.SkillNodes(filtered: true));

        maxModifierPoints = GetMaxModifierPointsFromLevel(lastPlayedFaction.level);
        availableModifierPoints = maxModifierPoints;

        foreach (var modifierButton in modifierButtons)
        {
            modifierButton.Initialize(modifiers);

            modifierButton.ResetOnActivate();
            modifierButton.OnActivate += OnModifierButtonActivate;
            modifierButton.ResetOnDeactivate();
            modifierButton.OnDeactivate += OnModifierButtonDeactivate;

            bool shouldBeActive = lastPlayedFaction.LastActiveAbilitModifiers.Contains(modifierButton.SkillModifier);

            if (shouldBeActive && !modifierButton.Locked)
            {
                modifierButton.Activate(true);
                assignedModifierPoints++;
                availableModifierPoints--;
            }
            else
            {
                modifierButton.Activate(false);
            }
        }

        UpdatePoints(withPulse: true);
    }

    private int GetMaxModifierPointsFromLevel(int factionLevel)
    {
        return Mathf.FloorToInt(factionLevel / 5);
    }

    private void UpdatePoints(bool withPulse = false)
    {
        SetDigitSprites(availableModifierPoints);
        if (withPulse) PulseDigits();
    }

    private void PulseDigits()
    {
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        Vector3 baseScale = Vector3.one;
        Vector3 pulse = Vector3.one * pulseScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / pulseDuration;
            digitOnes.transform.localScale = Vector3.Lerp(baseScale, pulse, t);
            if (digitTens.gameObject.activeSelf)
                digitTens.transform.localScale = Vector3.Lerp(baseScale, pulse, t);

            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / pulseDuration;
            digitOnes.transform.localScale = Vector3.Lerp(pulse, baseScale, t);
            if (digitTens.gameObject.activeSelf)
                digitTens.transform.localScale = Vector3.Lerp(pulse, baseScale, t);

            yield return null;
        }

        digitOnes.transform.localScale = baseScale;
        digitTens.transform.localScale = baseScale;

        pulseRoutine = null;
    }

    private void OnModifierButtonActivate(SkillModifiers modifier)
    {
        availableModifierPoints--;
        assignedModifierPoints++;

        SaveActivatedModifiers();
        UpdatePoints();
    }

    private void OnModifierButtonDeactivate(SkillModifiers modifier)
    {
        availableModifierPoints++;
        assignedModifierPoints--;

        SaveActivatedModifiers();
        UpdatePoints();
    }

    public void SaveActivatedModifiers()
    {
        HashSet<SkillModifiers> result = new();
        foreach (var modifierButton in modifierButtons)
        {
            if (modifierButton.Activated) result.Add(modifierButton.SkillModifier);
        }

        lastPlayedFaction.SetLastActiveAbilityModifier(result);
        saveContext.Save();
    }

    private void OnDestroy()
    {
        foreach (var modifierButton in modifierButtons)
        {
            modifierButton.OnActivate -= OnModifierButtonActivate;
            modifierButton.OnDeactivate -= OnModifierButtonDeactivate;
        }
    }

    private void SetDigitSprites(int number)
    {
        digits.GetDigits(
            number,
            out Sprite tens,
            out Sprite ones
        );

        digitOnes.sprite = ones;
        digitOnes.gameObject.SetActive(true);

        if (tens != null)
            digitTens.sprite = tens;
        digitTens.gameObject.SetActive(tens != null);
    }
}
