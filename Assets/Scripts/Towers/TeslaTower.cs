using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CapsuleCollider))]
public class TeslaTower : MonoBehaviour, ITower, ITowerSelectable, ITowerSellable, ITowerUpgradeable
{
    [Header("Stats")]
    [SerializeField] private float beamDamage = 30f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float range = 30f;
    [SerializeField, Range(0f, 1f)] private float critChance = 0.15f;
    [SerializeField] private float critMultiplier = 2.0f;

    [Header("References")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private GameObject rangeIndicator;
    [SerializeField] private Renderer[] highlightRenderers;

    [Header("UI References")]
    [SerializeField] private GameObject towerOverlayPrefab;
    [SerializeField] private CursorSettings cursorSettings;

    [Header("Upgrades")]
    [SerializeField] private int currentLevel = 1;

    [Header("VFX")]
    [SerializeField] private ParticleSystem upgradeVFX;

    private readonly Dictionary<int, Enemy> enemiesInRange = new();
    private Enemy target;
    private float fireCooldown = 0f;

    private GameObject towerOverlayGO;
    private TowerOverlay towerOverlay;

    private TowerSelectionManager towerSelectionManager;
    private TowerUpgradeManager towerUpgradeManager;

    private Func<float, float> CalculateBaseBeamDamage;

    public TowerTypes TowerType() => TowerTypes.Tesla;

    public int CurrentLevel() => currentLevel;

    public bool CanUpgrade() => towerUpgradeManager.CanUpgrade(TowerType(), CurrentLevel());

    void OnDrawGizmosSelected()
    {
        TowerMechanics.DrawRangeGizmos(transform.position, Color.cyan, range);
    }

    private void Awake()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        towerOverlayGO = Instantiate(towerOverlayPrefab, canvas.transform, true);
        towerOverlay = towerOverlayGO.GetComponent<TowerOverlay>();
        towerOverlay.Initialize(gameObject);
        towerOverlay.Hide();

        towerSelectionManager = FindFirstObjectByType<TowerSelectionManager>();
        towerUpgradeManager = FindFirstObjectByType<TowerUpgradeManager>();
    }

    private void Start()
    {
        Assert.IsNotNull(capsuleCollider);
        capsuleCollider.radius = range;

        Assert.IsNotNull(rangeIndicator);
        rangeIndicator.SetActive(false);
        rangeIndicator.transform.localScale = new(range * 2, rangeIndicator.transform.localScale.y, range * 2);
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (target == null)
        {
            target = TowerMechanics.GetClosestEnemy(transform.position, enemiesInRange);
            if (target == null) return;
        }

        if (!TowerMechanics.IsEnemyInRange(transform.position, target, range))
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

    private void HandleEnemyDeath(Enemy deadEnemy)
    {
        target = TowerMechanics.HandleEnemyRemoval(deadEnemy, enemiesInRange, target);
    }

    private void Shoot(Enemy enemy)
    {
        GameObject beamGO = Instantiate(beamPrefab, firePoint.position, Quaternion.identity);
        Beam beam = beamGO.GetComponent<Beam>();

        bool isCritical = UnityEngine.Random.value < critChance;
        float damage = CalculateBaseBeamDamage?.Invoke(beamDamage) ?? beamDamage;
        if (isCritical) damage *= critMultiplier;
        beam.Initialize(firePoint, enemy.transform, damage, isCritical);
    }

    public void Select()
    {
        rangeIndicator.SetActive(true);
        towerOverlay.Show();
        TowerMechanics.ApplyHighlight(highlightRenderers, TowerMechanics.SelectedColor);
    }

    public void Deselect()
    {
        rangeIndicator.SetActive(false);
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

    public void ApplyUpgrade(TowerUpgradeData data)
    {
        // TODO: upgrade has to receive tower-specific data + make sure every stat update works

        upgradeVFX.Play();

        towerSelectionManager.DeselectCurrent();

        currentLevel = data.level;

        beamDamage = data.damage;
        fireRate = data.fireRate;
        range = data.range;
        critChance = data.critChance;
        critMultiplier = data.critMultiplier;

        rangeIndicator.transform.localScale = new(range * 2, rangeIndicator.transform.localScale.y, range * 2);
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
