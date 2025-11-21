using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftoverDebrisArea : MonoBehaviour
{
    [SerializeField] private GameObject visual;
    [SerializeField] private SphereCollider sphereCollider;

    private float debrisAreaRange;
    private float debrisDuration;
    private EnemyStatusEffect debrisSlow;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();

    public void Initialize(LeftoverDebrisModifier modifier)
    {
        debrisAreaRange = modifier.debrisAreaRange;
        debrisDuration = modifier.debrisDuration;
        debrisSlow = modifier.debrisSlow;

        float visualY = visual.transform.localScale.y;
        visual.transform.localScale = new(debrisAreaRange, visualY, debrisAreaRange);
        sphereCollider.radius = debrisAreaRange / 2f;

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
