// Inspiration taken from this Unity discussion https://discussions.unity.com/t/how-do-i-specify-a-prefab-in-a-json-file/1678229/2
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum EnemyType
{
    Robot,
    Heavy,
    Fast
}

[CreateAssetMenu(fileName = "EnemyCatalog", menuName = "Scriptable Objects/EnemyCatalog")]
public class EnemyCatalog : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public EnemyType enemyType;
        public Enemy prefab;
    }

    [SerializeField]
    private List<Entry> entries = new List<Entry>();

    private Dictionary<EnemyType, Enemy> catalog;

    public Enemy Get(EnemyType enemyType)
    {
        Assert.IsNotNull(catalog);
        Assert.IsTrue(catalog.ContainsKey(enemyType));
        var prefab = catalog[enemyType];
        Assert.IsNotNull(prefab);
        return prefab;
    }

    void OnEnable()
    {
        BuildPrefabCatalog();
    }

    private void BuildPrefabCatalog()
    {
        Assert.IsNotNull(entries);
        if (catalog == null)
        {
            catalog = new Dictionary<EnemyType, Enemy>();
        }
        else
        {
            catalog.Clear();
        }

        foreach (var e in entries)
        {
            Assert.IsNotNull(e);
            Assert.IsFalse(catalog.ContainsKey(e.enemyType));
            catalog[e.enemyType] = e.prefab;
        }
    }
}
