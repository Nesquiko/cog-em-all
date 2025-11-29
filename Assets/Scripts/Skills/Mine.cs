using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour, ISkillPlaceable
{
    [SerializeField] private SkillTypes skillType = SkillTypes.Mine;
    [SerializeField] private Quaternion placementRotationOffset = Quaternion.Euler(0f, 0f, 0f);
    public SkillTypes SkillType() => skillType;
    public float GetCooldown() => 5f;
    public Quaternion PlacementRotationOffset() => placementRotationOffset;

    [Header("Settings")]
    [SerializeField] private float armDelay = 1.0f;
    [SerializeField] private float triggerDelay = 1.0f;
    [SerializeField] private float explosionRadius = 15.0f;
    [SerializeField] private float explosionDamage = 200.0f;
    [SerializeField] private int explosionCount = 1;
    [SerializeField] private float timeBetweenExplosions = 0.3f;
    [SerializeField] private LayerMask enemyMask;

    [SerializeField] private GameObject mineModel;
    [SerializeField] private MeshRenderer[] mineScrews;
    [SerializeField] private Material screwArmedMaterial;
    [SerializeField] private GameObject minimapIndicator;
    [SerializeField] private Vector3 minimapIndicatorScale;

    [Header("VFX")]
    [SerializeField] private ParticleSystem mineExplosion;

    [SerializeField] private SkillModifierCatalog skillModifierCatalog;

    private bool armed;
    private bool triggered;

    private DoubleTheBoomModifier doubleTheBoomModifier;
    private WideDestructionModifier wideDestructionModifier;
    private QuickFuseModifier quickFuseModifier;

    private HashSet<SkillModifiers> activeMineModifiers;
    private bool doubleTheBoomActive;
    private bool wideDestructionActive;
    private bool quickFuseActive;

    private void Awake()
    {
        activeMineModifiers = skillModifierCatalog.ActiveModifiersFromSkillType(skillType);

        InitializeModifiers();
    }

    private void InitializeModifiers()
    {
        doubleTheBoomModifier = (DoubleTheBoomModifier)skillModifierCatalog.FromSkillAndModifier(skillType, SkillModifiers.DoubleTheBoom);
        wideDestructionModifier = (WideDestructionModifier)skillModifierCatalog.FromSkillAndModifier(skillType, SkillModifiers.WideDestruction);
        quickFuseModifier = (QuickFuseModifier)skillModifierCatalog.FromSkillAndModifier(skillType, SkillModifiers.QuickFuse);

        doubleTheBoomActive = activeMineModifiers.Contains(SkillModifiers.DoubleTheBoom);
        wideDestructionActive = activeMineModifiers.Contains(SkillModifiers.WideDestruction);
        quickFuseActive = activeMineModifiers.Contains(SkillModifiers.QuickFuse);

        if (doubleTheBoomActive)
        {
            explosionCount = doubleTheBoomModifier.explosionCount;
            explosionDamage *= doubleTheBoomModifier.damageFractionPerExplosion;
        } 
        if (wideDestructionActive)
        {
            explosionRadius = wideDestructionModifier.explosionRadius;
        }
        if (quickFuseActive)
        {
            triggerDelay *= quickFuseModifier.triggerSpeedMultiplier;
        }
    }

    public void Initialize()
    {
        minimapIndicator.transform.localScale = minimapIndicatorScale;

        StartCoroutine(ArmAfterDelay());
    }

    private IEnumerator ArmAfterDelay()
    {
        yield return new WaitForSeconds(armDelay);
        armed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!armed || triggered) return;
        if (!other.TryGetComponent<IEnemy>(out var _)) return;

        triggered = true;
        StartCoroutine(TriggerExplosion());
    }

    private IEnumerator TriggerExplosion()
    {
        int screwCount = mineScrews.Length;
        if (screwCount == 0)
        {
            yield return new WaitForSeconds(triggerDelay);
            StartCoroutine(ExplodeSeries());
            yield break;
        }

        float interval = triggerDelay / screwCount;

        for (int i = 0; i < screwCount; i++)
        {
            var screw = mineScrews[i];
            if (screw != null)
                screw.material = screwArmedMaterial;

            yield return new WaitForSeconds(interval);
        }

        StartCoroutine(ExplodeSeries());
    }

    private IEnumerator ExplodeSeries()
    {
        for (int i = 0; i < explosionCount; i++)
        {
            ExplodeOnce();

            if (i < explosionCount - 1)
                yield return new WaitForSeconds(timeBetweenExplosions);
        }

        yield return new WaitForSecondsRealtime(0.1f);
        Destroy(mineModel);
        yield return new WaitForSecondsRealtime(2.1f);
        Destroy(gameObject);
    }

    private void ExplodeOnce()
    {
        Vector3 offset = Random.insideUnitSphere * 0.5f;
        mineExplosion.transform.position = transform.position + offset;

        mineExplosion.Play();

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, enemyMask, QueryTriggerInteraction.Collide);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IEnemy>(out var e))
                e.TakeDamage(explosionDamage);
        }
    }
}
