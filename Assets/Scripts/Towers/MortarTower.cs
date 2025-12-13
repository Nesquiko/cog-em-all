using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;

public class MortarTower : MonoBehaviour, ITower, ITowerSelectable, ITowerSellable, ITowerStimulable, IAppliesDOT
{
    [Header("Stats")]
    [SerializeField] private float shellDamage = 120f;
    [SerializeField] private float shellSplashRadius = 10f;
    [SerializeField] private float shellLifetime = 5f;
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
    [SerializeField] private Transform barrel;
    [SerializeField] private Transform firePoint;
    [SerializeField] private CapsuleCollider outerCollider;
    [SerializeField] private CapsuleCollider innerCollider;
    [SerializeField] private DecalProjector outerRangeProjector;
    [SerializeField] private DecalProjector innerRangeProjector;
    [SerializeField] private Renderer[] highlightRenderers;

    [Header("UI References")]
    [SerializeField] private TowerOverlayCatalog towerOverlayCatalog;
    [SerializeField] private CursorSettings cursorSettings;

    [Header("Upgrades")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxAllowedLevel = 1;
    [SerializeField] private TowerDataCatalog towerDataCatalog;
    [SerializeField] private GameObject level2;
    [SerializeField] private GameObject level3;

    [Header("Slow on Hit")]
    [SerializeField] private bool slowOnHitActive = false;

    [Header("Bleed on Hit")]
    [SerializeField] private bool bleedOnHitActive = false;
    [SerializeField] private float bleedDuration = EnemyStatusEffect.BleedDefaultDuration;

    [Header("Range on Hill")]
    [SerializeField] private bool hillRangeSkillActive = false;
    [SerializeField] private float heightRangeMultiplier = 0.05f;
    [SerializeField] private float baselineHeight = 0f;

    [Header("Stim Mode")]
    [SerializeField] private float stimMultiplier = 2f;
    [SerializeField] private float stimDuration = 5f;
    [SerializeField] private float stimCooldown = 5f;

    [Header("Double Payload")]
    [SerializeField] private bool doublePayloadActive = false;
    public void EnableDoublePayload() => doublePayloadActive = true;
    [SerializeField] private float secondPayloadDelay = 0.3f;
    [SerializeField] private float damageFactor = 0.75f;

    [Header("Recoil")]
    [SerializeField] private float recoilDistance = 0.5f;
    [SerializeField] private float recoilSpeed = 20f;
    [SerializeField] private float recoilReturnSpeed = 5f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem upgradeVFX;
    [SerializeField] private ParticleSystem stimModeVFX;
    [SerializeField] private ParticleSystem stimCooldownVFX;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();
    private readonly HashSet<int> tooClose = new();
    private IEnemy target;
    private float fireCooldown;

    private Vector3 barrelDefaultPosition;
    private Coroutine recoilRoutine;

    private bool stimActive = false;
    private bool stimCoolingDown = false;
    private float stimTimer;
    private float stimCooldownTimer;
    public bool StimActive() => stimActive;
    public bool StimCoolingDown() => stimCoolingDown;
    public bool CanActivateStim() => !stimActive && !stimCoolingDown;

    private float baseShellDamage;
    private float baseShellSplashRadius;
    private float baseCritChance;
    private float baseCritMultiplier;
    private float baseFireRate;
    private float baseMaxRange;

    private GameObject towerOverlayGO;
    private TowerOverlay towerOverlay;

    private TowerSelectionManager towerSelectionManager;

    private Func<float, float> CalculateBaseShellDamage;
    private Func<float, float> CalculateFireRate;
    private Func<float, float> CalculateCritChance;

    public float ShellSplashRadius => shellSplashRadius;
    public float ShellLifetime => shellLifetime;
    public bool SlowOnHitActive => slowOnHitActive;
    public bool BleedOnHitActive => bleedOnHitActive;
    public float BleedDuration => bleedDuration;

    public TowerTypes TowerType() => TowerTypes.Mortar;

    public int CurrentLevel() => currentLevel;
    public int MaxAllowedLevel() => maxAllowedLevel;
    public bool CanUpgrade() => towerDataCatalog.CanUpgrade(TowerType(), CurrentLevel(), MaxAllowedLevel());

    public event Action<TowerTypes, float> OnDamageDealt;
    public event Action<TowerTypes> OnEnemyKilled;
    public event Action<TowerTypes> OnUpgrade;

    private OperationDataDontDestroy operationData;

    private void OnDrawGizmosSelected()
    {
        TowerMechanics.DrawRangeGizmos(
            transform.position,
            new (float, Color?)[]
            {
                (EffectiveRange(maxRange), Color.cyan),
                (EffectiveRange(minRange), Color.red)
            }
        );

        Handles.color = Color.cyan;
        Handles.DrawWireDisc(transform.position, Vector3.up, EffectiveRange(maxRange));

        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, EffectiveRange(minRange));
    }

    private void Awake()
    {
        operationData = OperationDataDontDestroy.GetOrReadDev();
        maxAllowedLevel = TowerMechanics.GetMaxAllowedLevel(TowerType());

        Canvas canvas = FindFirstObjectByType<Canvas>();
        towerOverlayGO = Instantiate(towerOverlayCatalog.FromFactionAndTowerType(operationData.Faction, TowerType()), canvas.transform, true);
        towerOverlay = towerOverlayGO.GetComponent<TowerOverlay>();
        towerOverlay.Initialize(gameObject);
        towerOverlay.Hide();

        towerSelectionManager = FindFirstObjectByType<TowerSelectionManager>();
    }

    private void Start()
    {
        Assert.IsNotNull(outerCollider);
        outerCollider.radius = EffectiveRange(maxRange);

        Assert.IsNotNull(innerCollider);
        innerCollider.radius = EffectiveRange(minRange);

        Assert.IsNotNull(outerRangeProjector);
        ShowRange(outerRangeProjector, false);
        SetRangeProjector(outerRangeProjector, EffectiveRange(maxRange));

        Assert.IsNotNull(innerRangeProjector);
        ShowRange(innerRangeProjector, false);
        SetRangeProjector(innerRangeProjector, EffectiveRange(minRange));

        Assert.IsNotNull(barrel);
        barrelDefaultPosition = barrel.localPosition;
    }

    private float EffectiveRange(float r)
    {
        if (!hillRangeSkillActive) return r;

        float height = transform.position.y - baselineHeight;
        float heightBonus = Mathf.Max(0f, 1f + height * heightRangeMultiplier);
        return r * heightBonus;
    }

    private void ShowRange(DecalProjector projector, bool show) => projector.gameObject.SetActive(show);

    private void SetRangeProjector(DecalProjector projector, float radius)
    {
        var size = projector.size;
        size.x = size.y = radius * 2f;
        projector.size = size;
    }

    private void Update()
    {
        HandleStimUpdate();
        if (stimCoolingDown) return;

        target = GetValidTarget();
        if (target == null) return;

        RotateTowardTarget(target.Transform);

        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f && IsAimedAtTarget(target.Transform))
        {
            Shoot(target);
            fireCooldown = 1f / fireRate;
        }
    }

    private void HandleStimUpdate()
    {
        if (stimActive)
        {
            stimTimer -= Time.deltaTime;
            if (stimTimer <= 0f)
                EndStim();
        }
        else if (stimCoolingDown)
        {
            stimCooldownTimer -= Time.deltaTime;
            if (stimCooldownTimer <= 0f)
            {
                stimCoolingDown = false;
                stimCooldownVFX.Stop(withChildren: true);
            }
        }
    }

    private void EndStim()
    {
        stimActive = false;
        stimCoolingDown = true;
        stimCooldownTimer = stimCooldown;

        shellDamage = baseShellDamage;
        shellSplashRadius = baseShellSplashRadius;
        critChance = baseCritChance;
        critMultiplier = baseCritMultiplier;
        fireRate = CalculateFireRate(baseFireRate);
        maxRange = baseMaxRange;

        outerCollider.radius = EffectiveRange(maxRange);
        SetRangeProjector(outerRangeProjector, EffectiveRange(maxRange));

        stimModeVFX.Stop(withChildren: true);
        stimCooldownVFX.Play();
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

    private void HandleDamageDealt(float damage) => OnDamageDealt?.Invoke(TowerType(), damage);

    private void HandleEnemyKilled() => OnEnemyKilled?.Invoke(TowerType());

    private void Shoot(IEnemy enemy)
    {
        StartCoroutine(ShootRoutine(enemy));
    }

    private IEnumerator ShootRoutine(IEnemy enemy)
    {
        FireSingleShell(enemy);

        if (stimActive && doublePayloadActive)
        {
            yield return new WaitForSeconds(secondPayloadDelay);
            FireSingleShell(enemy);
        }
    }

    private void FireSingleShell(IEnemy enemy)
    {
        if (enemy == null) return;

        GameObject shellGO = Instantiate(shellPrefab, firePoint.position, firePoint.rotation);
        Shell shell = shellGO.GetComponent<Shell>();

        shell.OnDamageDealt += HandleDamageDealt;
        shell.OnEnemyKilled += HandleEnemyKilled;
        shell.Initialize(this);

        if (recoilRoutine != null)
            StopCoroutine(recoilRoutine);
        recoilRoutine = StartCoroutine(RecoilKick());

        bool isCritical = UnityEngine.Random.value < critChance;
        float dmg = (CalculateBaseShellDamage?.Invoke(shellDamage) ?? shellDamage) * ((stimActive && doublePayloadActive) ? damageFactor : 1f);
        if (isCritical)
            dmg *= critMultiplier;

        shell.Launch(enemy.Transform.position, dmg, isCritical, arcHeight);
    }

    private IEnemy GetValidTarget()
    {
        foreach (var (id, enemy) in enemiesInRange)
        {
            if (tooClose.Contains(id)) continue;

            if (enemy.Marked) return enemy;

            return enemy;
        }
        return null;
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
        Vector3 start = barrel.localPosition;
        Vector3 back = barrelDefaultPosition - Vector3.up * recoilDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            barrel.localPosition = Vector3.Lerp(start, back, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilReturnSpeed;
            barrel.localPosition = Vector3.Lerp(back, barrelDefaultPosition, t);
            yield return null;
        }

        barrel.localPosition = barrelDefaultPosition;
    }

    public int InstanceID() => gameObject.GetInstanceID();

    public void Select()
    {
        ShowRange(outerRangeProjector, true);
        ShowRange(innerRangeProjector, true);
        towerOverlay.Show();
        TowerMechanics.ApplyHighlight(highlightRenderers, TowerMechanics.SelectedColor);
    }

    public void Deselect()
    {
        ShowRange(outerRangeProjector, false);
        ShowRange(innerRangeProjector, false);
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
        if (baseData is not MortarTowerData data) return;

        upgradeVFX.Play();

        towerSelectionManager.DeselectCurrent();

        currentLevel = data.Level;
        if (currentLevel == 2) level2.SetActive(true);
        if (currentLevel == 3) level3.SetActive(true);

        shellDamage = data.shellDamage;
        shellSplashRadius = data.shellSplashRadius;
        shellLifetime = data.shellLifetime;
        fireRate = CalculateFireRate(data.fireRate);
        minRange = data.minRange;
        maxRange = data.maxRange;
        critChance = CalculateCritChance(data.critChance);
        critMultiplier = data.critMultiplier;
        rotationSpeed = data.rotationSpeed;
        launchSpeed = data.launchSpeed;
        arcHeight = data.arcHeight;

        outerCollider.radius = EffectiveRange(data.maxRange);
        innerCollider.radius = EffectiveRange(data.minRange);
        SetRangeProjector(outerRangeProjector, EffectiveRange(data.maxRange));
        SetRangeProjector(innerRangeProjector, EffectiveRange(data.minRange));

        OnUpgrade?.Invoke(TowerType());
    }

    public void SetDamageCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateBaseShellDamage = f;
    }

    public void SetFireRateCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateFireRate = f;
        fireRate = CalculateFireRate(fireRate);
    }

    public void SetCritChangeCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateCritChance = f;
        critChance = CalculateCritChance(critChance);
    }

    public void RecalctCritChance() => critChance = CalculateCritChance(critChance);

    public void SetDotEnabled(bool enabled) => bleedOnHitActive = enabled;
    public void SetDotDuration(float duration) => bleedDuration = duration;


    public void EnableSlowOnhit() => slowOnHitActive = true;

    public void ActivateStim()
    {
        if (stimActive || stimCoolingDown) return;

        stimActive = true;
        stimTimer = stimDuration;
        stimCoolingDown = false;

        baseShellDamage = shellDamage;
        baseShellSplashRadius = shellSplashRadius;
        baseCritChance = critChance;
        baseCritMultiplier = critMultiplier;
        baseFireRate = fireRate;
        baseMaxRange = maxRange;

        shellDamage *= stimMultiplier;
        shellSplashRadius *= stimMultiplier;
        critChance *= Mathf.Clamp01(critChance * stimMultiplier);
        critMultiplier *= stimMultiplier;
        fireRate = CalculateFireRate(fireRate) * stimMultiplier;
        maxRange *= stimMultiplier;

        outerCollider.radius = EffectiveRange(maxRange);
        SetRangeProjector(outerRangeProjector, EffectiveRange(maxRange));

        stimModeVFX.Play();
    }

    private void OnDestroy()
    {
        TowerMechanics.UnsubscribeAll(enemiesInRange, HandleEnemyDeath);
        enemiesInRange.Clear();
    }

    public void ActivateGainRangeOnHill() => hillRangeSkillActive = true;

    public float Range() => maxRange;
    public void SetRange(float range) => maxRange = range;
}