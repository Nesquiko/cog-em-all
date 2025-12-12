using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftoverDebrisArea : MonoBehaviour
{
    [SerializeField] private GameObject debris;
    [SerializeField] private GameObject reinforcedDebris;
    [SerializeField] private GameObject spikedDebris;
    [SerializeField] private BoxCollider boxCollider;

    private float debrisDuration;
    private EnemyStatusEffect debrisSlow;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();

    public void Initialize(LeftoverDebrisModifier modifier, bool reinforced, bool spiked)
    {
        debrisDuration = modifier.debrisDuration;
        debrisSlow = modifier.debrisSlow;

        debris.SetActive(true);
        reinforcedDebris.SetActive(reinforced);
        spikedDebris.SetActive(spiked);

        Destroy(gameObject, debrisDuration);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IEnemy>(out var enemy)) return;
        int id = enemy.GetInstanceID();
        if (enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Add(id, enemy);

        enemy.ApplyEffect(debrisSlow);
    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IEnemy>(out var enemy)) return;
        int id = enemy.GetInstanceID();
        if (!enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Remove(id);

        enemy.RemoveEffect(debrisSlow.type);
    }

    private void OnDestroy()
    {
        foreach (var enemy in enemiesInRange.Values)
        {
            enemy?.RemoveEffect(debrisSlow.type);
        }
        enemiesInRange.Clear();
    }
}
