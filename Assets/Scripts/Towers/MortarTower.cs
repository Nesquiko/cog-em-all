using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class MortarTower : MonoBehaviour, ITower, ITowerSelectable, ITowerSellable, ITowerUpgradeable
{
    [Header("Stats")]
    [SerializeField] private float shellDamage = 120f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float minRange = 20f;
    [SerializeField] private float maxRange = 60f;
    [SerializeField, Range(0f, 1f)] private float critChance = 0.15f;
    [SerializeField] private float critMultiplier = 2.0f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float launchSpeed = 30f;
    [SerializeField] private float arcHeight = 15f;

    [Header("References")]
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private Transform basePivot;
    [SerializeField] private Transform cannonPivot;
    [SerializeField] private Transform firePoint;
    [SerializeField] private CapsuleCollider outerCollider;
    [SerializeField] private CapsuleCollider innerCollider;
    [SerializeField] private GameObject outerRangeIndicator;
    [SerializeField] private GameObject innerRangeIndicator;
    [SerializeField] private Renderer[] highlightRenderers;

    [Header("UI References")]
    [SerializeField] private GameObject towerOverlayPrefab;
    [SerializeField] private CursorSettings cursorSettings;

    [Header("Upgrades")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private TowerDataCatalog towerDataCatalog;

    [Header("Recoil")]
    [SerializeField] private float recoilDistance = 0.5f;
    [SerializeField] private float recoilSpeed = 20f;
    [SerializeField] private float recoilReturnSpeed = 5f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem upgradeVFX;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();
    private readonly HashSet<int> tooClose = new();
    private IEnemy target;
    private float fireCooldown;

    private Vector3 cannonPivotDefaultPosition;
    private Coroutine recoilRoutine;

    private GameObject towerOverlayGO;
    private TowerOverlay towerOverlay;

    private TowerSelectionManager towerSelectionManager;

    private Func<float, float> CalculateBaseShellDamage;

    public TowerTypes TowerType() => TowerTypes.Mortar;

    public int CurrentLevel() => currentLevel;

    public bool CanUpgrade() => towerDataCatalog.CanUpgrade(TowerType(), CurrentLevel());

    private void OnDrawGizmosSelected()
    {
        TowerMechanics.DrawRangeGizmos(
            transform.position,
            new (float, Color?)[]
            {
                (maxRange, Color.cyan),
                (minRange, Color.red)
            }
        );

        Handles.color = Color.cyan;
        Handles.DrawWireDisc(transform.position, Vector3.up, maxRange);

        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, minRange);
    }

    private void Awake()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        towerOverlayGO = Instantiate(towerOverlayPrefab, canvas.transform, true);
        towerOverlay = towerOverlayGO.GetComponent<TowerOverlay>();
        towerOverlay.Initialize(gameObject);
        towerOverlay.Hide();

        towerSelectionManager = FindFirstObjectByType<TowerSelectionManager>();
    }

    private void Start()
    {
        Assert.IsNotNull(outerCollider);
        outerCollider.radius = maxRange;

        Assert.IsNotNull(innerCollider);
        innerCollider.radius = minRange;

        Assert.IsNotNull(outerRangeIndicator);
        outerRangeIndicator.SetActive(false);
        outerRangeIndicator.transform.localScale = new(maxRange * 2, outerRangeIndicator.transform.localScale.y, maxRange * 2);

        Assert.IsNotNull(innerRangeIndicator);
        innerRangeIndicator.SetActive(false);
        innerRangeIndicator.transform.localScale = new(minRange * 2, innerRangeIndicator.transform.localScale.y, minRange * 2);

        Assert.IsNotNull(cannonPivot);
        cannonPivotDefaultPosition = cannonPivot.localPosition;
    }

    private void Update()
    {
        if (target == null || !IsEnemyValid(target.Transform.position))
        {
            target = GetValidTarget();
            if (target == null) return;
        }

        RotateTowardTarget(target.Transform);

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f && IsAimedAtTarget(target.Transform))
        {
            Shoot(target);
            fireCooldown = 1f / fireRate;
        }
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
        tooClose.Remove(id);
        e.OnDeath -= HandleEnemyDeath;
    }

    public void RegisterTooClose(IEnemy e)
    {
        tooClose.Add(e.GetInstanceID());
    }

    public void UnregisterTooClose(IEnemy e)
    {
        tooClose.Remove(e.GetInstanceID());
    }

    private void HandleEnemyDeath(IEnemy deadEnemy)
    {
        target = TowerMechanics.HandleEnemyRemoval(deadEnemy, enemiesInRange, target);
    }

    private void Shoot(IEnemy enemy)
    {
        GameObject shellGO = Instantiate(shellPrefab, firePoint.position, firePoint.rotation);
        Shell shell = shellGO.GetComponent<Shell>();
        if (recoilRoutine != null) StopCoroutine(recoilRoutine);
        recoilRoutine = StartCoroutine(RecoilKick());

        bool isCritical = UnityEngine.Random.value < critChance;
        float dmg = CalculateBaseShellDamage?.Invoke(shellDamage) ?? shellDamage;
        if (isCritical) dmg *= critMultiplier;

        shell.Launch(enemy.Transform.position, dmg, isCritical, arcHeight);
    }

    private bool IsEnemyValid(Vector3 enemyPosition)
    {
        float distance = Vector3.Distance(transform.position, enemyPosition);
        return distance >= minRange && distance <= maxRange;
    }

    private IEnemy GetValidTarget()
    {
        IEnemy best = null;
        float bestDistance = Mathf.Infinity;

        foreach (var (id, enemy) in enemiesInRange)
        {
            if (tooClose.Contains(id)) continue;

            float distance = Vector3.Distance(transform.position, enemy.Transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = enemy;
            }
        }
        return best;
    }

    private bool IsAimedAtTarget(Transform targetTransform, float yawTolerance = 0.5f, float pitchTolerance = 20f)
    {
        if (targetTransform == null) return false;

        Vector3 baseForward = basePivot.forward;
        Vector3 dirToTarget = (targetTransform.position - basePivot.position).normalized;
        dirToTarget.y = 0f;

        float yawDot = Vector3.Dot(baseForward, dirToTarget);
        if (yawDot < yawTolerance)
            return false;

        Vector3 cannonForward = cannonPivot.forward;
        Vector3 dirToTargetFull = (targetTransform.position - firePoint.position).normalized;
        float pitchAngle = Vector3.Angle(cannonForward, dirToTargetFull);

        return pitchAngle <= pitchTolerance;
    }

    private void RotateTowardTarget(Transform targetTransform)
    {
        if (targetTransform == null) return;

        rotationSpeed = Mathf.Lerp(rotationSpeed, 1.5f, Time.deltaTime * 0.5f);

        Vector3 flatDirection = targetTransform.position - basePivot.position;
        flatDirection.y = 0f;
        if (flatDirection.sqrMagnitude > 0.001f)
        {
            Quaternion baseRotation = Quaternion.LookRotation(flatDirection);
            basePivot.rotation = Quaternion.Lerp(
                basePivot.rotation,
                baseRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        float distance = Vector3.Distance(basePivot.position, targetTransform.position);
        float g = 9.81f;
        float v = launchSpeed;

        float ratio = (g * distance) / (v * v);
        if (ratio > 1f) return;
        float angleRad = 0.5f * Mathf.Asin(Mathf.Clamp(ratio, -1f, 1f));
        float angleDeg = Mathf.Clamp(angleRad * Mathf.Rad2Deg, 5f, 80f);

        Quaternion desiredRotation = Quaternion.Euler(angleDeg, 0f, 0f);
        cannonPivot.localRotation = Quaternion.Lerp(
            cannonPivot.localRotation,
            desiredRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private IEnumerator RecoilKick()
    {
        Vector3 start = cannonPivot.localPosition;
        Vector3 back = cannonPivotDefaultPosition + cannonPivot.forward * recoilDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            cannonPivot.localPosition = Vector3.Lerp(start, back, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilReturnSpeed;
            cannonPivot.localPosition = Vector3.Lerp(back, cannonPivotDefaultPosition, t);
            yield return null;
        }

        cannonPivot.localPosition = cannonPivotDefaultPosition;
    }

    public void Select()
    {
        outerRangeIndicator.SetActive(true);
        innerRangeIndicator.SetActive(true);
        towerOverlay.Show();
        TowerMechanics.ApplyHighlight(highlightRenderers, TowerMechanics.SelectedColor);
    }

    public void Deselect()
    {
        outerRangeIndicator.SetActive(false);
        innerRangeIndicator.SetActive(false);
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

    public void ApplyUpgrade(TowerDataBase data)
    {
        // TODO: upgrade has to receive tower-specific data + make sure every stat update works
        // the comments below are a by-product of this

        upgradeVFX.Play();

        towerSelectionManager.DeselectCurrent();

        /*currentLevel = data.level;

        shellDamage = data.damage;
        fireRate = data.fireRate;
        maxRange = data.range;
        critChance = data.critChance;
        critMultiplier = data.critMultiplier;

        outerCollider.radius = data.range;
        // innerCollider.radius = data.range;
        outerRangeIndicator.transform.localScale = new(maxRange * 2, outerRangeIndicator.transform.localScale.y, maxRange * 2);*/
        // innerRangeIndicator.transform.localScale = new(minRange * 2, innerRangeIndicator.transform.localScale.y, minRange * 2);
    }

    public void SetDamageCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateBaseShellDamage = f;
    }

    private void OnDestroy()
    {
        TowerMechanics.UnsubscribeAll(enemiesInRange, HandleEnemyDeath);
        enemiesInRange.Clear();
    }
}
