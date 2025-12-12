using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, IDamageSource, ISkillPlaceable, IDamageable
{
    [SerializeField] private SkillTypes skillType = SkillTypes.Wall;
    [SerializeField] private Quaternion placementRotationOffset = Quaternion.Euler(0f, 0f, 0f);
    public SkillTypes SkillType() => skillType;
    public float GetCooldown() => 5f;
    public Quaternion PlacementRotationOffset() => placementRotationOffset;
    public SkillActivationMode ActivationMode() => SkillActivationMode.Placement;
    public Transform Transform() => transform;
    public DamageSourceType Type() => DamageSourceType.Wall;

    [SerializeField] private float maxHealthPoints = 500f;
    [SerializeField] private GameObject wallHealthBar;
    [SerializeField] private GameObject wallModel;
    [SerializeField] private GameObject minimapIndicator;

    [SerializeField] private Vector3 wallHealthBarScale;
    [SerializeField] private Vector3 minimapIndicatorScale;

    [Header("Modifiers")]
    [SerializeField] private GameObject wallReinforced;
    [SerializeField] private GameObject wallSpiked;

    [SerializeField] private SkillModifierCatalog skillModifierCatalog;

    private bool isDying;

    private float healthPoints;
    public float HealthPointsNormalized() => healthPoints / maxHealthPoints;

    public bool IsDestroyed() => isDying || healthPoints <= 0f;

    public event Action<Wall> OnDestroyed;
    public event Action<Wall> OnHealthChanged;

    private SteelReinforcementModifier steelReinforcementModifier;
    private SharpThornsModifier sharpThornsModifier;
    private LeftoverDebrisModifier leftoverDebrisModifier;

    private HashSet<SkillModifiers> activeWallModifiers;
    private bool steelReinforcementActive;
    private bool sharpThornsActive;
    private bool leftoverDebrisActive;

    private void Awake()
    {
        healthPoints = maxHealthPoints;
        OnHealthChanged?.Invoke(this);

        activeWallModifiers = skillModifierCatalog.ActiveModifiersFromSkillType(skillType);

        InitializeModifiers();
    }

    private void InitializeModifiers()
    {
        steelReinforcementModifier = (SteelReinforcementModifier)skillModifierCatalog.FromSkillAndModifier(skillType, SkillModifiers.SteelReinforcement);
        sharpThornsModifier = (SharpThornsModifier)skillModifierCatalog.FromSkillAndModifier(skillType, SkillModifiers.SharpThorns);
        leftoverDebrisModifier = (LeftoverDebrisModifier)skillModifierCatalog.FromSkillAndModifier(skillType, SkillModifiers.LeftoverDebris);

        steelReinforcementActive = activeWallModifiers.Contains(SkillModifiers.SteelReinforcement);
        sharpThornsActive = activeWallModifiers.Contains(SkillModifiers.SharpThorns);
        leftoverDebrisActive = activeWallModifiers.Contains(SkillModifiers.LeftoverDebris);

        wallReinforced.SetActive(steelReinforcementActive);
        wallSpiked.SetActive(sharpThornsActive);
        
        if (steelReinforcementActive)
        {
            maxHealthPoints *= steelReinforcementModifier.healthPointsMultiplier;
            healthPoints = maxHealthPoints;
            OnHealthChanged?.Invoke(this);
        }
    }

    public void Initialize()
    {
        wallHealthBar.transform.localScale = wallHealthBarScale;
        minimapIndicator.transform.localScale = minimapIndicatorScale;
    }

    public void TakeDamage(float damage, IEnemy attacker)
    {
        if (isDying) return;

        healthPoints -= damage;
        if (wallHealthBar != null)
        {
            wallHealthBar.SetActive(true);
        }

        OnHealthChanged?.Invoke(this);

        if (sharpThornsActive)
        {
            float reflectedDamage = damage * sharpThornsModifier.fractionToReturn;
            attacker?.TakeDamage(reflectedDamage, Type());
        }

        if (healthPoints <= 0f)
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        if (isDying) yield break;
        isDying = true;

        healthPoints = 0;
        OnDestroyed?.Invoke(this);

        yield return new WaitForSeconds(0.1f);

        Destroy(wallModel);
        Destroy(wallHealthBar);

        if (leftoverDebrisActive)
        {
            GameObject areaGO = Instantiate(leftoverDebrisModifier.debrisAreaPrefab, transform.position, Quaternion.identity);
            if (areaGO.TryGetComponent<LeftoverDebrisArea>(out var area))
            {
                area.Initialize(leftoverDebrisModifier, reinforced: steelReinforcementActive, spiked: sharpThornsActive);
            }
        }

        yield return new WaitForSeconds(2.1f);

        Destroy(gameObject);
    }

    public bool IsFullHealth => Mathf.Approximately(healthPoints, maxHealthPoints);
}
