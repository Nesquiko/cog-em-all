using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilSpill : MonoBehaviour, ISkillPlaceable
{
    [SerializeField] private SkillTypes skillType = SkillTypes.OilSpill;
    [SerializeField] private Quaternion placementRotationOffset = Quaternion.Euler(0f, 90f, 0f);
    public SkillTypes SkillType() => skillType;
    public float GetCooldown() => 5f;
    public Quaternion PlacementRotationOffset() => placementRotationOffset;

    [Header("Oil Settings")]
    [SerializeField] private float duration = 10f;
    [SerializeField] private float slowMultiplier = 0.4f;

    [Header("Ignite Settings")]
    [SerializeField] private float flameTickDamage = 6f;
    [SerializeField] private float flameTickInterval = 0.5f;

    [SerializeField] private GameObject minimapIndicator;
    [SerializeField] private Vector3 minimapIndicatorScale;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();
    private bool burning;

    public void Initialize()
    {
        minimapIndicator.transform.localScale = minimapIndicatorScale;

        StartCoroutine(ExpireAfter(duration));
    }

    private IEnumerator ExpireAfter(float time)
    {
        yield return new WaitForSeconds(time);

        foreach (var enemy in enemiesInRange.Values)
        {
            if (enemy != null)
                enemy.RemoveEffect(EffectType.Oiled);
        }
        enemiesInRange.Clear();

        Destroy(gameObject);
    }

    public void Ignite()
    {
        if (burning) return;
        burning = true;

        foreach (var e in enemiesInRange.Values)
        {
            if (e == null) continue;
            e.ApplyEffect(EnemyStatusEffect.OilBurn(flameTickDamage, flameTickInterval));
        }
    }

    public void Extinguish()
    {
        if (!burning) return;
        burning = false;

        foreach (var e in enemiesInRange.Values)
        {
            if (e == null) continue;
            e.RemoveEffect(EffectType.OilBurned);
        }
    }

    public void RegisterInRange(IEnemy e)
    {
        int id = e.GetInstanceID();
        if (enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Add(id, e);

        e.ApplyEffect(EnemyStatusEffect.Oiled(slowMultiplier));
        if (burning) e.ApplyEffect(EnemyStatusEffect.OilBurn(flameTickDamage, flameTickInterval));
    }

    public void UnregisterOutOfRange(IEnemy e)
    {
        int id = e.GetInstanceID();
        if (!enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Remove(id);

        e.RemoveEffect(EffectType.Oiled);
        e.RemoveEffect(EffectType.OilBurned);
    }
}
