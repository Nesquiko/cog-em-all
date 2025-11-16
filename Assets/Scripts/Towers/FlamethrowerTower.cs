using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class FlamethrowerTower : MonoBehaviour, ITower, ITowerSelectable, ITowerSellable, ITowerRotateable
{
    [Header("Stats")]
    [SerializeField] private float flameDamagePerPulse = 20f;
    [SerializeField] private float flamePulseInterval = 0.25f;
    [SerializeField] private float flameDuration = 3f;
    [SerializeField] private float range = 10f;
    [SerializeField] private float flameAngle = 60f;
    [SerializeField] private float cooldownDuration = 2f;
    [SerializeField, Range(0f, 1f)] private float critChance = 0.15f;
    [SerializeField] private float critMultiplier = 2.0f;

    [Header("References")]
    [SerializeField] private GameObject flamePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject flameCollider;
    [SerializeField] private GameObject rangeIndicator;
    [SerializeField] private Renderer[] highlightRenderers;

    [Header("UI References")]
    [SerializeField] private GameObject towerOverlayPrefab;
    [SerializeField] private GameObject towerRotationOverlayPrefab;
    [SerializeField] private CursorSettings cursorSettings;

    [Header("Upgrades")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private TowerDataCatalog towerDataCatalog;

    [Header("VFX")]
    [SerializeField] private ParticleSystem upgradeVFX;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();
    private IEnemy target;

    private bool underPlayerRotation = false;
    private bool isCoolingDown = false;
    private Flame activeFlame;

    public float DamagePerPulse => flameDamagePerPulse;
    public float FlamePulseInterval => flamePulseInterval;
    public float FlameDuration => flameDuration;
    public float CritChance => critChance;
    public float CritMultiplier => critMultiplier;

    private GameObject towerOverlayGO;
    private TowerOverlay towerOverlay;

    private GameObject towerRotationOverlayGO;
    private TowerRotationOverlay towerRotationOverlay;

    private TowerSelectionManager towerSelectionManager;

    private Func<float, float> CalculateBaseFlameDamagePerPulse;

    public TowerTypes TowerType() => TowerTypes.Flamethrower;

    public int CurrentLevel() => currentLevel;

    public bool CanUpgrade() => towerDataCatalog.CanUpgrade(TowerType(), CurrentLevel());

    private void OnDrawGizmosSelected()
    {
        float radius = range;
        float angle = flameAngle;

        Handles.color = Color.cyan;
        Vector3 position = transform.position;

        Vector3 startDirection = Quaternion.Euler(0, -angle / 2f, 0) * transform.forward;
        Vector3 endDirection = Quaternion.Euler(0, angle / 2f, 0) * transform.forward;

        Handles.DrawWireArc(
            position,
            Vector3.up,
            startDirection,
            angle,
            radius
        );

        Handles.DrawLine(position, position + startDirection * radius);
        Handles.DrawLine(position, position + endDirection * radius);
    }

    private void Awake()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();

        towerOverlayGO = Instantiate(towerOverlayPrefab, canvas.transform, true);
        towerOverlay = towerOverlayGO.GetComponent<TowerOverlay>();
        towerOverlay.Initialize(gameObject);
        towerOverlay.Hide();

        towerRotationOverlayGO = Instantiate(towerRotationOverlayPrefab, canvas.transform, true);
        towerRotationOverlay = towerRotationOverlayGO.GetComponent<TowerRotationOverlay>();
        towerRotationOverlay.Initialize(gameObject);
        towerRotationOverlay.Hide();

        towerSelectionManager = FindFirstObjectByType<TowerSelectionManager>();
    }

    private void Start()
    {
        Assert.IsNotNull(flameCollider);
        flameCollider.transform.localScale = new(range, range, range);

        Assert.IsNotNull(rangeIndicator);
        rangeIndicator.SetActive(false);
        rangeIndicator.transform.localScale = new(range, rangeIndicator.transform.localScale.y, range);

        Vector3 flamePosition = new(firePoint.position.x, 0f, firePoint.position.z);
        GameObject flame = Instantiate(flamePrefab, flamePosition, firePoint.rotation);
        activeFlame = flame.GetComponent<Flame>();
        activeFlame.Initialize(this, range);
        flame.SetActive(false);
    }

    private void Update()
    {
        if (underPlayerRotation) return;

        if (!isCoolingDown && enemiesInRange.Count > 0 && activeFlame != null)
        {
            Shoot();
        }
    }

    private void LateUpdate()
    {
        if (activeFlame != null)
        {
            activeFlame.transform.SetPositionAndRotation(new(firePoint.position.x, 0f, firePoint.position.z), firePoint.rotation);
        }
    }

    private void Shoot()
    {
        if (isCoolingDown || activeFlame == null) return;

        activeFlame.gameObject.SetActive(true);
        activeFlame.Initialize(this, range);
        activeFlame.StartFlame(CalculateBaseFlameDamagePerPulse);

        StartCoroutine(CooldownRoutine(flameDuration));
    }

    public List<IEnemy> GetCurrentEnemiesInRange()
    {
        List<IEnemy> currentEnemies = new();
        foreach (var enemy in enemiesInRange.Values)
        {
            if (enemy == null) continue;
            currentEnemies.Add(enemy);
        }
        return currentEnemies;
    }

    private IEnumerator CooldownRoutine(float duration)
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(duration);

        activeFlame.StopFlame();
        activeFlame.gameObject.SetActive(false);

        yield return new WaitForSeconds(cooldownDuration);
        isCoolingDown = false;
    }

    public void RegisterInRange(IEnemy e)
    {
        int id = e.GetInstanceID();
        if (enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Add(id, e);
        e.OnDeath += HandleEnemyDeath;
    }

    public void UnregisterOutOfRange(IEnemy e)
    {
        int id = e.GetInstanceID();
        enemiesInRange.Remove(id);
        e.OnDeath -= HandleEnemyDeath;
    }

    private void HandleEnemyDeath(IEnemy deadEnemy)
    {
        target = TowerMechanics.HandleEnemyRemoval(deadEnemy, enemiesInRange, target);
    }

    public void Select()
    {
        rangeIndicator.SetActive(true);
        towerOverlay.Show();
        towerRotationOverlay.Hide();
        TowerMechanics.ApplyHighlight(highlightRenderers, TowerMechanics.SelectedColor);
    }

    public void Deselect()
    {
        rangeIndicator.SetActive(false);
        towerOverlay.Hide();
        towerRotationOverlay.Hide();
        EndManualRotation();
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

    public void ShowTowerOverlay()
    {
        EndManualRotation();
        towerOverlay.Show();
        towerRotationOverlay.Hide();
    }

    public void ShowTowerRotationOverlay()
    {
        BeginManualRotation();
        towerRotationOverlay.Show();
        towerOverlay.Hide();
    }

    public void BeginManualRotation()
    {
        underPlayerRotation = true;

        if (activeFlame != null && activeFlame.gameObject.activeSelf)
        {
            activeFlame.StopFlame();
            activeFlame.gameObject.SetActive(false);
            StopAllCoroutines();
            isCoolingDown = false;
        }
    }

    public void EndManualRotation()
    {
        underPlayerRotation = false;
    }

    public void SellAndDestroy()
    {
        towerSelectionManager.DeselectCurrent();

        if (activeFlame != null)
        {
            activeFlame.StopFlame();
            Destroy(activeFlame.gameObject);
        }

        Destroy(towerOverlayGO);
        Destroy(towerRotationOverlayGO);
        Destroy(gameObject);
    }

    public void ApplyUpgrade(TowerDataBase baseData)
    {
        if (baseData is not FlamethrowerTowerData data) return;

        upgradeVFX.Play();

        towerSelectionManager.DeselectCurrent();

        currentLevel = data.Level;

        flameDamagePerPulse = data.flameDamagePerPulse;
        flamePulseInterval = data.flamePulseInterval;
        flameDuration = data.flameDuration;
        range = data.range;
        flameAngle = data.flameAngle;
        cooldownDuration = data.cooldownDuration;
        critChance = data.critChance;
        critMultiplier = data.critMultiplier;

        activeFlame.UpdateRange(data.range);
        rangeIndicator.transform.localScale = new(range * 2, rangeIndicator.transform.localScale.y, range * 2);
    }

    public void SetDamageCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateBaseFlameDamagePerPulse = f;
    }

    private void OnDestroy()
    {
        TowerMechanics.UnsubscribeAll(enemiesInRange, HandleEnemyDeath);
        enemiesInRange.Clear();
    }
}