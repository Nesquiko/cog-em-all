using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilSpill : MonoBehaviour, ISkillPlaceable
{
    [SerializeField] private SkillTypes skillType = SkillTypes.OilSpill;
    [SerializeField] private Quaternion placementRotationOffset = Quaternion.Euler(0f, 90f, 0f);
    public SkillTypes SkillType => skillType;
    public Quaternion PlacementRotationOffset => placementRotationOffset;

    [SerializeField] private float duration = 10f;
    [SerializeField] private float slowMultiplier = 0.4f;

    [SerializeField] private GameObject minimapIndicator;
    [SerializeField] private Vector3 minimapIndicatorScale;

    private readonly Dictionary<int, Enemy> enemiesInRange = new(); 

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

    public void RegisterInRange(Enemy e)
    {
        int id = e.gameObject.GetInstanceID();
        if (enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Add(id, e);

        e.ApplyEffect(EnemyStatusEffect.Oiled(slowMultiplier));
    }

    public void UnregisterOutOfRange(Enemy e)
    {
        int id = e.gameObject.GetInstanceID();
        if (!enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Remove(id);

        e.RemoveEffect(EffectType.Oiled);
    }
}
