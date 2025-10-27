using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


[CreateAssetMenu(fileName = "TowerDataCatalog", menuName = "Scriptable Objects/TowerDataCatalog")]
public class TowerDataCatalog : ScriptableObject
{
    [Tooltip("List of towers available in the game.")]
    public List<TowerData> towers = new List<TowerData>();

    private Dictionary<TowerTypes, TowerData> catalog = new Dictionary<TowerTypes, TowerData>();

    public int TowerCount => towers?.Count ?? 0;

    private void OnEnable()
    {
        RebuildCatalog();
    }

    private void OnValidate()
    {
        RebuildCatalog();
    }

    private void RebuildCatalog()
    {
        catalog.Clear();

        foreach (var t in towers)
        {
            if (catalog.ContainsKey(t.type))
            {
                Debug.LogWarning($"duplicate TowerType detected: {t.type}");
                continue;
            }

            catalog[t.type] = t;
        }
    }

    public TowerData FromIndex(int i)
    {
        Assert.IsTrue(System.Enum.IsDefined(typeof(TowerTypes), i), $"invalid TowerType value: {i}");
        return Get((TowerTypes)i);
    }

    public TowerData Get(TowerTypes type)
    {
        Assert.IsNotNull(catalog);
        Assert.IsTrue(catalog.ContainsKey(type));
        var towerData = catalog[type];
        Assert.IsNotNull(towerData);
        return towerData;
    }
}