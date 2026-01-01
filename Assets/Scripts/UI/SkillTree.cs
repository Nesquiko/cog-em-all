using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class SkillTree : MonoBehaviour
{
    [SerializeField] private Faction faction;
    public Faction Faction => faction;
    [SerializeField] private Image skillPointsDigitTens;
    [SerializeField] private Image skillPointsDigitOnes;
    [SerializeField] private FancyDigits digits;
    [SerializeField] private float pulseScale = 1.15f;
    [SerializeField] private float pulseDuration = 0.15f;
    [SerializeField] private Button resetSkillPointsButton;
    [SerializeField] private GameObject[] ranks;

    private int availableSkillPoints = 0;
    private int assignedSkillPoints = 0;

    private Coroutine pulseRoutine;

    private bool suppressEvents = false;

    public bool CanAssignSkillPoint => availableSkillPoints > 0;
    public bool CanRemoveSkillPoint => assignedSkillPoints > 0;

    private Dictionary<string, int> skillNodes = new();
    public Dictionary<string, int> SkillNodes => skillNodes;

    private int level = 0;
    public int Level => level;

    public void Initialize(FactionSaveState factionSaveState)
    {
        level = factionSaveState.level;
        skillNodes = factionSaveState.SkillNodes();
    }

    private void Start()
    {
        // first calculate how many skill points there should be
        availableSkillPoints = CalculateAvailableSkillPoints();
        // apply saved active skills
        assignedSkillPoints = AssignSkillPoints(addActions: true);
        // recalculate after using skill points on saved active skills
        availableSkillPoints = CalculateAvailableSkillPoints();
        UpdateVisual();

        RefreshAllConnections();
    }

    private int AssignSkillPoints(bool addActions)
    {
        int assigned = 0;

        foreach (var rank in ranks)
        {
            if (rank == null) continue;

            for (int i = 0; i < rank.transform.childCount; i++)
            {
                var nodeTransform = rank.transform.GetChild(i);

                if (!nodeTransform.TryGetComponent<SkillTreeNodeButton>(out var button))
                {
                    continue;
                }

                int activeRanks = 0;
                skillNodes.TryGetValue(button.SkillSlug, out activeRanks);
                button.SetActiveRanks(activeRanks);
                assigned += button.ActiveRanks;

                if (addActions)
                    button.OnActiveRanksChanged += UpdateSkillPoints;

                skillNodes[button.SkillSlug] = button.ActiveRanks;
            }
        }

        return assigned;
    }

    private int CalculateAvailableSkillPoints()
    {
        Assert.IsTrue(level >= assignedSkillPoints, $"Assigned more points than level (Level ({level}) is not greater than assigned skill points ({assignedSkillPoints}))");
        return level - assignedSkillPoints;
    }

    private void UpdateSkillPoints(SkillTreeNodeButton node, int delta)
    {
        if (suppressEvents) return;

        assignedSkillPoints += delta;
        availableSkillPoints -= delta;

        if (delta != 0)
            skillNodes[node.SkillSlug] = node.ActiveRanks;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        SetDigitSprites(availableSkillPoints);
        resetSkillPointsButton.interactable = assignedSkillPoints > 0;

        PulseDigits();
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
            skillPointsDigitOnes.transform.localScale = Vector3.Lerp(baseScale, pulse, t);
            if (skillPointsDigitTens.gameObject.activeSelf)
                skillPointsDigitTens.transform.localScale = Vector3.Lerp(baseScale, pulse, t);

            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / pulseDuration;
            skillPointsDigitOnes.transform.localScale = Vector3.Lerp(pulse, baseScale, t);

            if (skillPointsDigitTens.gameObject.activeSelf)
                skillPointsDigitTens.transform.localScale = Vector3.Lerp(pulse, baseScale, t);

            yield return null;
        }

        skillPointsDigitOnes.transform.localScale = baseScale;
        skillPointsDigitTens.transform.localScale = baseScale;

        pulseRoutine = null;
    }

    public void ResetSkillPoints()
    {
        suppressEvents = true;
        assignedSkillPoints = 0;

        foreach (var rank in ranks)
        {
            if (rank == null) continue;

            for (int i = 0; i < rank.transform.childCount; i++)
            {
                var node = rank.transform.GetChild(i);

                if (!node.TryGetComponent<SkillTreeNodeButton>(out var button)) continue;

                button.ResetActiveRanks();
                skillNodes[button.SkillSlug] = 0;
            }
        }

        suppressEvents = false;
        availableSkillPoints = CalculateAvailableSkillPoints();
        UpdateVisual();
    }

    private void RefreshAllConnections()
    {
        foreach (var rank in ranks)
        {
            if (rank == null) continue;

            for (int i = 0; i < rank.transform.childCount; i++)
            {
                if (rank.transform.GetChild(i).TryGetComponent<SkillTreeNodeButton>(out var button))
                {
                    button.UpdateState();
                }
            }
        }
    }

    private void SetDigitSprites(int number)
    {
        digits.GetDigits(
            number,
            out Sprite tens,
            out Sprite ones
        );

        skillPointsDigitOnes.sprite = ones;
        skillPointsDigitOnes.gameObject.SetActive(true);

        if (tens != null)
            skillPointsDigitTens.sprite = tens;
        skillPointsDigitTens.gameObject.SetActive(tens != null);
    }
}
