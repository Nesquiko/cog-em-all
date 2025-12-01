using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CapsuleCollider))]
public class TeslaTower : MonoBehaviour, ITower, ITowerSelectable, ITowerSellable
{
    [Header("Stats")]
    [SerializeField] private float beamDamage = 30f;
    [SerializeField] private float beamSpeed = 1000;
    [SerializeField] private float beamChainRadius = 10f;
    [SerializeField] private int beamMaxChains = 3;
    [SerializeField] private float beamStayTimeOnHit = 0.05f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float range = 30f;
    [SerializeField, Range(0f, 1f)] private float critChance = 0.15f;
    [SerializeField] private float critMultiplier = 2.0f;

    [Header("References")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private DecalProjector rangeProjector;
    [SerializeField] private Renderer[] highlightRenderers;

    [Header("UI References")]
    [SerializeField] private TowerOverlayCatalog towerOverlayCatalog;
    [SerializeField] private CursorSettings cursorSettings;

    [Header("Upgrades")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private TowerDataCatalog towerDataCatalog;

    [Header("Range on Hill")]
    [SerializeField] private bool hillRangeSkillActive = false;
    [SerializeField] private float heightRangeMultiplier = 0.05f;
    [SerializeField] private float baselineHeight = 0f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem upgradeVFX;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();
    private IEnemy target;
    private float fireCooldown = 0f;

    private GameObject towerOverlayGO;
    private TowerOverlay towerOverlay;

    private TowerSelectionManager towerSelectionManager;

    private Func<float, float> CalculateBaseBeamDamage;

    public float BeamSpeed => beamSpeed;
    public float BeamChainRadius => beamChainRadius;
    public int BeamMaxChains => beamMaxChains;
    public float BeamStayTimeOnHit => beamStayTimeOnHit;

    public TowerTypes TowerType() => TowerTypes.Tesla;

    public int CurrentLevel() => currentLevel;

    public bool CanUpgrade() => towerDataCatalog.CanUpgrade(TowerType(), CurrentLevel());

    public Faction GetFaction() => Faction.TheBrassArmy;

    void OnDrawGizmosSelected()
    {
        TowerMechanics.DrawRangeGizmos(transform.position, Color.cyan, EffectiveRange(range));
    }

    private void Awake()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        towerOverlayGO = Instantiate(towerOverlayCatalog.FromFactionAndTowerType(GetFaction(), TowerType()), canvas.transform, true);
        towerOverlay = towerOverlayGO.GetComponent<TowerOverlay>();
        towerOverlay.Initialize(gameObject);
        towerOverlay.Hide();

        towerSelectionManager = FindFirstObjectByType<TowerSelectionManager>();
    }

    private void Start()
    {
        Assert.IsNotNull(capsuleCollider);
        capsuleCollider.radius = EffectiveRange(range);

        Assert.IsNotNull(rangeProjector);
        ShowRange(false);
        SetRangeProjector(EffectiveRange(range));
    }

    private float EffectiveRange(float r)
    {
        if (!hillRangeSkillActive) return r;

        float height = transform.position.y - baselineHeight;
        float heightBonus = Mathf.Max(0f, 1f + height * heightRangeMultiplier);
        return r * heightBonus;
    }

    private void ShowRange(bool show) => rangeProjector.gameObject.SetActive(show);

    private void SetRangeProjector(float radius)
    {
        var size = rangeProjector.size;
        size.x = size.y = radius * 2f;
        rangeProjector.size = size;
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (target == null)
        {
            target = TowerMechanics.GetClosestEnemy(transform.position, enemiesInRange);
            if (target == null) return;
        }

        if (!TowerMechanics.IsEnemyInRange(transform.position, target, EffectiveRange(range)))
        {
            target = null;
            return;
        }

        if (fireCooldown <= 0f)
        {
            Shoot(target);
            fireCooldown = 1f / fireRate;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TowerMechanics.HandleTriggerEnter(other, enemiesInRange, HandleEnemyDeath);
    }

    private void OnTriggerExit(Collider other)
    {
        TowerMechanics.HandleTriggerExit(other, enemiesInRange, HandleEnemyDeath, target, out target);
    }

    private void HandleEnemyDeath(IEnemy deadEnemy)
    {
        target = TowerMechanics.HandleEnemyRemoval(deadEnemy, enemiesInRange, target);
    }

    private void Shoot(IEnemy enemy)
    {
        GameObject beamGO = Instantiate(beamPrefab, firePoint.position, Quaternion.identity);
        Beam beam = beamGO.GetComponent<Beam>();

        bool isCritical = UnityEngine.Random.value < critChance;
        float damage = CalculateBaseBeamDamage?.Invoke(beamDamage) ?? beamDamage;
        if (isCritical) damage *= critMultiplier;
        beam.Initialize(this, firePoint, enemy.Transform, damage, isCritical);
    }

    public void Select()
    {
        ShowRange(true);
        towerOverlay.Show();
        TowerMechanics.ApplyHighlight(highlightRenderers, TowerMechanics.SelectedColor);
    }

    public void Deselect()
    {
        ShowRange(false);
        towerOverlay.Hide();
        TowerMechanics.ClearHighlight(highlightRenderers);
    }

    public void OnHoverEnter()
    {
        Cursor.SetCursor(cursorSettings.hoverCursor, cursorSettings.hotspot, CursorMode.Auto);
        TowerMechanics.ApplyHighlight(highlightRenderers, TowerMechanics.HoverColor);
    }

    public void OnHoverExit()
    {
        Cursor.SetCursor(cursorSettings.defaultCursor, Vector2.zero, CursorMode.Auto);
        if (towerSelectionManager.CurrentSelected() == (ITower)this)
        {
            TowerMechanics.ApplyHighlight(highlightRenderers, TowerMechanics.SelectedColor);
        }
        else
        {
            TowerMechanics.ClearHighlight(highlightRenderers);
        }
    }

    public void SellAndDestroy()
    {
        towerSelectionManager.DeselectCurrent();

        Destroy(towerOverlayGO);
        Destroy(gameObject);
    }

    public void ApplyUpgrade(TowerDataBase baseData)
    {
        if (baseData is not TeslaTowerData data) return;

        upgradeVFX.Play();

        towerSelectionManager.DeselectCurrent();

        currentLevel = data.Level;

        beamDamage = data.beamDamage;
        beamSpeed = data.beamSpeed;
        beamChainRadius = data.beamChainRadius;
        beamMaxChains = data.beamMaxChains;
        beamStayTimeOnHit = data.beamStayTimeOnHit;
        fireRate = data.fireRate;
        range = data.range;
        critChance = data.critChance;
        critMultiplier = data.critMultiplier;

        capsuleCollider.radius = EffectiveRange(range);
        SetRangeProjector(EffectiveRange(range));
    }

    public void SetDamageCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateBaseBeamDamage = f;
    }

    private void OnDestroy()
    {
        TowerMechanics.UnsubscribeAll(enemiesInRange, HandleEnemyDeath);
        enemiesInRange.Clear();
    }
}
