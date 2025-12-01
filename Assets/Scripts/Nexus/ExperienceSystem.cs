using System;
using UnityEngine;

public class ExperienceSystem : MonoBehaviour
{
    [Header("XP Progression")]
    [SerializeField] private float baseXP = 100f;
    [SerializeField] private float xpMultiplier = 1.45f;
    [SerializeField] private int maxLevel = 15;

    [Header("Operation Scaling")]
    [SerializeField] private float r = 0.054f;
    [SerializeField] private float p = 1.2f;
    [SerializeField] private float baseOperationReward = 100f;

    [Header("Current State")]
    [SerializeField] private int level = 1;
    [SerializeField] private float currentXP = 0f;
    [SerializeField] private int currentOperation = 1;

    public event Action<int> OnLevelUp;
    public event Action<float, float> OnXPChanged;

    public int Level => level;
    public float XP => currentXP;
    public float XPToNextLevel => baseXP * (Mathf.Pow(xpMultiplier, level) - 1f);
    public float XPProgress => Mathf.Clamp01(currentXP / XPToNextLevel);

    public float GetDeltaXP(int n) => baseXP * (Mathf.Pow(xpMultiplier, n) - 1f);

    public float GetTotalXPToLevel(int n) => baseXP * (Mathf.Pow(xpMultiplier, n) - 1f) / (xpMultiplier - 1f);

    public float GetDifficultyMultiplier(int operationIndex) => 1f + r * Mathf.Pow(operationIndex, p);

    public float GetXPReward(int operationIndex) => baseOperationReward * GetDifficultyMultiplier(operationIndex);

    public void AddXP(float amount)
    {
        currentXP += amount;
        OnXPChanged?.Invoke(currentXP, XPToNextLevel);

        while (level < maxLevel && currentXP >= XPToNextLevel)
        {
            currentXP -= XPToNextLevel;
            level++;
            OnLevelUp?.Invoke(level);
        }
    }

    public float CompleteOperation()
    {
        float reward = GetXPReward(currentOperation);
        AddXP(reward);

        currentOperation++;
        return reward;
    }

    [ContextMenu("Simulate To Max Level")]
    public void Simulate()
    {
        float totalXP = 0f;
        int ops = 0;

        while (level < maxLevel)
        {
            float gain = CompleteOperation();
            totalXP += gain;
            ops++;
        }

        Debug.Log($"Reached level {maxLevel} afer {ops} operations, total XP earned: {totalXP:F0}");
    }

    private void OnValidate()
    {
        if (p < 0f) p = 0f;
        if (r < 0f) r = 0f;
        if (xpMultiplier < 1.0f) xpMultiplier = 1.0f;
    }
}
