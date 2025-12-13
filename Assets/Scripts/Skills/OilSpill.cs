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
    public SkillActivationMode ActivationMode() => SkillActivationMode.Placement;

    [Header("Oil Settings")]
    [SerializeField] private float duration = 10f;
    [SerializeField] private float slowMultiplier = 0.4f;

    [Header("Ignite Settings")]
    [SerializeField] private float flameTickDamage = 6f;
    [SerializeField] private float flameTickInterval = 0.5f;

    [SerializeField] private GameObject minimapIndicator;
    [SerializeField] private Vector3 minimapIndicatorScale;

    [SerializeField] private SkillModifierCatalog skillModifierCatalog;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();
    private bool burning;

    private SatansWrathModifier satansWrathModifier;
    private GooeyGooModifier gooeyGooModifier;
    private StickityStickModifier stickityStickModifier;

    private HashSet<SkillModifiers> activeOilSpillModifiers;
    private bool satansWrathActive;
    private bool gooeyGooActive;
    private bool stickityStickActive;

    private void Awake()
    {
        activeOilSpillModifiers = SkillMechanics.ActiveModifiersFromSkillType(skillType);

        InitializeModifiers();
    }

    private void InitializeModifiers()
    {
        satansWrathModifier = (SatansWrathModifier)skillModifierCatalog.FromSkillAndModifier(skillType, SkillModifiers.SatansWrath);
        gooeyGooModifier = (GooeyGooModifier)skillModifierCatalog.FromSkillAndModifier(skillType, SkillModifiers.GooeyGoo);
        stickityStickModifier = (StickityStickModifier)skillModifierCatalog.FromSkillAndModifier(skillType, SkillModifiers.StickityStick);

        satansWrathActive = activeOilSpillModifiers.Contains(SkillModifiers.SatansWrath);
        gooeyGooActive = activeOilSpillModifiers.Contains(SkillModifiers.GooeyGoo);
        stickityStickActive = activeOilSpillModifiers.Contains(SkillModifiers.StickityStick);

        if (stickityStickActive)
        {
            slowMultiplier *= stickityStickModifier.speedMultiplier;
        }
    }

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
            if (enemy.Equals(null) || enemy.Transform.Equals(null)) continue;
            enemy.RemoveEffect(EffectType.Oiled);
            if (gooeyGooActive) enemy.ApplyEffect(gooeyGooModifier.gooeySlow);
        }
        enemiesInRange.Clear();

        Destroy(gameObject);
    }

    public void Ignite()
    {
        if (!satansWrathActive) return;
        if (burning) return;
        burning = true;

        foreach (var e in enemiesInRange.Values)
        {
            if (e.Equals(null) || e.Transform.Equals(null)) continue;
            e.ApplyEffect(EnemyStatusEffect.OilBurn(flameTickDamage, flameTickInterval));
        }
    }

    public void Extinguish()
    {
        if (!satansWrathActive) return;
        if (!burning) return;
        burning = false;

        foreach (var e in enemiesInRange.Values)
        {
            if (e.Equals(null) || e.Transform.Equals(null)) continue;
            e.RemoveEffect(EffectType.OilBurned);
        }
    }

    public void RegisterInRange(IEnemy e)
    {
        int id = e.GetInstanceID();
        if (enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Add(id, e);

        e.ApplyEffect(EnemyStatusEffect.Oiled(slowMultiplier));
        if (satansWrathActive && burning) e.ApplyEffect(EnemyStatusEffect.OilBurn(flameTickDamage, flameTickInterval));
    }

    public void UnregisterOutOfRange(IEnemy e)
    {
        int id = e.GetInstanceID();
        if (!enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Remove(id);

        e.RemoveEffect(EffectType.Oiled);
        e.RemoveEffect(EffectType.OilBurned);
        if (gooeyGooActive) e.ApplyEffect(gooeyGooModifier.gooeySlow);
    }

    private void OnDestroy()
    {
        foreach (var enemy in enemiesInRange.Values)
        {
            enemy?.RemoveEffect(EffectType.Oiled);
            enemy?.RemoveEffect(EffectType.OilBurned);
        }
        enemiesInRange.Clear();
    }
}
