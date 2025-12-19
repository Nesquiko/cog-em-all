using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LoadingTipsData", menuName = "Scriptable Objects/Loading Tips", order = 0)]
public class LoadingTipsData : ScriptableObject
{
    [Tooltip("List of all loading screen tips that show randomly.")]
    [SerializeField, TextArea(2, 5)] private List<string> tips = new();

    [SerializeField] private float tipChangeInterval = 1.5f;

    public IReadOnlyList<string> Tips => tips;

    public int TipCount => tips.Count;

    public float TipChangeInterval => tipChangeInterval;
}
