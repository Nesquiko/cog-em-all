using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "UITexts", menuName = "Scriptable Objects/UITexts")]
public class UITexts : ScriptableObject
{
    [Serializable]
    public class FactionNameEntry
    {
        public Faction faction;
        public string displayName;
    }

    [SerializeField] private FactionNameEntry[] factionNames;

    private Dictionary<Faction, string> cache;

    private void OnEnable()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        cache = new Dictionary<Faction, string>();
        if (factionNames == null) return;

        foreach (var entry in factionNames)
        {
            Assert.IsFalse(cache.ContainsKey(entry.faction), $"duplicate faction '{entry.faction}' in faction names");
            cache[entry.faction] = entry.displayName;
        }
    }

    public string GetFactionName(Faction faction)
    {
        if (cache == null || cache.Count == 0) BuildCache();
        Assert.IsTrue(cache.TryGetValue(faction, out var name));
        return name;
    }
}
