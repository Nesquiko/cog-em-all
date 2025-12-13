using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CapsuleCollider))]
public class TeslaTower : MonoBehaviour, ITower, ITowerSelectable, ITowerSellable, ITowerControllable, ITowerStimulable
{
    [Header("Stats")]
    [SerializeField] private float beamDamage = 30f;
    [SerializeField] private float beamSpeed = 1000;
    [SerializeField] private float beamChainRadius = 10f;
    [SerializeField] private int beamChains = 1;
    private int additionalBeamChains = 0;
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

    [Header("Tower Control Mode")]
    [SerializeField] private Transform controlPoint;
    [SerializeField] private float sensitivity = 0.075f;

    [Header("Execution")]
    [SerializeField] private bool executeActive = true;
    [SerializeField, Range(0.05f, 1f)] private float executeThreshold = 0.3f;

    [Header("Upgrades")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxAllowedLevel = 1;
    [SerializeField] private TowerDataCatalog towerDataCatalog;

    [Header("Disable Buffs on Hit")]
    [SerializeField] private bool disableBuffsOnHitActive = true;

    [Header("Range on Hill")]
    [SerializeField] private bool hillRangeSkillActive = false;
    [SerializeField] private float heightRangeMultiplier = 0.05f;
    [SerializeField] private float baselineHeight = 0f;

    [Header("Stun First Enemy")]
    [SerializeField] private bool stunFirstEnemy = true;

    [Header("Stim Mode")]
    [SerializeField] private float stimMultiplier = 2f;
    [SerializeField] private float stimDuration = 5f;
    [SerializeField] private float stimCooldown = 5f;

    [Header("Double Beam")]
    [SerializeField] private bool doubleBeamActive = true;
    [SerializeField] private float damageFactor = 0.75f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem upgradeVFX;
    [SerializeField] private ParticleSystem stimModeVFX;
    [SerializeField] private ParticleSystem[] stimCooldownVFX;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();
    private IEnemy target;
    private float fireCooldown = 0f;

    private bool stimActive = false;
    private bool stimCoolingDown = false;
    private float stimTimer;
    private float stimCooldownTimer;
    public bool StimActive() => stimActive;
    public bool StimCoolingDown() => stimCoolingDown;
    public bool CanActivateStim() => !stimActive && !stimCoolingDown;

    private float baseBeamDamage;
    private float baseBeamChainRadius;
    private float baseCritChance;
    private float baseCritMultiplier;
    private float baseFireRate;
    private float baseRange;

    private bool underPlayerControl;
    private float yaw;
    private float pitch;

    private GameObject towerOverlayGO;
    private TowerOverlay towerOverlay;

    private TowerSelectionManager towerSelectionManager;

    private Func<float, float> CalculateBaseBeamDamage;
    private Func<float, float> CalculateFireRate;
    private Func<float, float> CalculateCritChance;

    public float BeamSpeed => beamSpeed;
    public float BeamChainRadius => beamChainRadius;
    public int BeamMaxChains => beamChains + additionalBeamChains;
    public float BeamStayTimeOnHit => beamStayTimeOnHit;
    public bool ExecuteActive => underPlayerControl && executeActive;
    public float ExecuteThreshold => executeThreshold;
    public bool DisableBuffsOnHitActive => disableBuffsOnHitActive;

    public TowerTypes TowerType() => TowerTypes.Tesla;

    public int CurrentLevel() => currentLevel;
    public int MaxAllowedLevel() => maxAllowedLevel;
    public bool CanUpgrade() => towerDataCatalog.CanUpgrade(TowerType(), CurrentLevel(), MaxAllowedLevel());

    public Transform GetControlPoint() => controlPoint;

    private OperationDataDontDestroy operationData;

    void OnDrawGizmosSelected()
    {
        TowerMechanics.DrawRangeGizmos(transform.position, Color.cyan, EffectiveRange(range));
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
        Assert.IsNotNull(capsuleCollider);
        capsuleCollider.radius = EffectiveRange(range);

        Assert.IsNotNull(rangeProjector);
        ShowRange(false);
        SetRangeProjector(EffectiveRange(range));
    }

    public void SetAdditionalChainReach(int chainReach) => additionalBeamChains = chainReach;

    public void EnableControlMode() => towerOverlay.EnableControlMode();

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
        if (underPlayerControl) return;

        HandleStimUpdate();
        if (stimCoolingDown) return;

        fireCooldown -= Time.deltaTime;

        target = TowerMechanics.SelectTargetWithMarkPriority(
            transform.position,
            enemiesInRange,
            target,
            EffectiveRange(range)
        );

        if (target == null) return;

        if (fireCooldown <= 0f)
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
                foreach (var scVFX in stimCooldownVFX)
                    scVFX.Stop(withChildren: true);
            }
        }
    }

    private void EndStim()
    {
        stimActive = false;
        stimCoolingDown = true;
        stimCooldownTimer = stimCooldown;

        beamDamage = baseBeamDamage;
        beamChainRadius = baseBeamChainRadius;
        critChance = baseCritChance;
        critMultiplier = baseCritMultiplier;
        fireRate = baseFireRate;
        range = baseRange;

        capsuleCollider.radius = EffectiveRange(range);
        SetRangeProjector(EffectiveRange(range));

        stimModeVFX.Stop(withChildren: true);
        foreach (var scVFX in stimCooldownVFX)
            scVFX.Play();
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

    private List<IEnemy> GetValidTargets(int count)
    {
        List<IEnemy> validTargets = new();

        foreach (var enemy in enemiesInRange.Values)
        {
            if (enemy == null) continue;
            if (enemy.HealthPointsNormalized <= 0) continue;
            if (validTargets.Contains(enemy)) continue;

            if (enemy.Marked)
                validTargets.Add(enemy);

            if (validTargets.Count >= count)
                break;
        }

        foreach (var enemy in enemiesInRange.Values)
        {
            if (enemy == null) continue;
            if (enemy.HealthPointsNormalized <= 0) continue;
            if (validTargets.Contains(enemy)) continue;

            validTargets.Add(enemy);

            if (validTargets.Count >= count)
                break;
        }

        return validTargets;
    }

    private void Shoot(IEnemy primaryEnemy)
    {
        if (stimActive && doubleBeamActive)
        {
            var targets = GetValidTargets(2);
            if (targets.Count == 0) return;

            foreach (var t in targets)
            {
                FireBeamAtTarget(t);
            }
        }
        else
        {
            if (primaryEnemy == null) return;
            FireBeamAtTarget(primaryEnemy);
        }
    }

    private void FireBeamAtTarget(IEnemy enemy)
    {
        if (enemy == null) return;

        GameObject beamGO = Instantiate(beamPrefab, firePoint.position, Quaternion.identity);
        Beam beam = beamGO.GetComponent<Beam>();

        bool isCritical = UnityEngine.Random.value < critChance;
        float damage = (CalculateBaseBeamDamage?.Invoke(beamDamage) ?? beamDamage) * ((stimActive && doubleBeamActive) ? damageFactor : 1f);
        if (isCritical) damage *= critMultiplier;
        beam.Initialize(this, firePoint, enemy.Transform, damage, isCritical);

        if (stimActive && stunFirstEnemy)
            enemy.ApplyEffect(EnemyStatusEffect.Stun);
    }

    public int InstanceID() => gameObject.GetInstanceID();

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

    public void OnPlayerTakeControl(bool active)
    {
        underPlayerControl = active;
        fireCooldown = 0f;

        if (active)
        {
            target = null;

            Vector3 euler = transform.rotation.eulerAngles;
            yaw = euler.y;
            pitch = euler.x;

            if (controlPoint != null)
                controlPoint.rotation = transform.rotation;
        }
    }

    public void HandlePlayerAim(Vector2 mouseDelta)
    {
        yaw += mouseDelta.x * sensitivity;
        pitch -= mouseDelta.y * sensitivity;
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        Quaternion lookRot = Quaternion.Euler(pitch, yaw, 0f);

        if (controlPoint != null)
            controlPoint.rotation = lookRot;
    }

    public void HandlePlayerFire()
    {
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            ShootManual();
            fireCooldown = 1f / fireRate;
        }
    }

    private void ShootManual()
    {
        firePoint.forward = controlPoint.forward;

        Vector3 aimPoint = firePoint.position + firePoint.forward * 100f;
        GameObject fakeTarget = new("TeslaManualAimTarget");
        fakeTarget.transform.position = aimPoint;

        GameObject beamGO = Instantiate(beamPrefab, firePoint.position, Quaternion.LookRotation(firePoint.forward, Vector3.up));
        Beam beam = beamGO.GetComponent<Beam>();

        bool isCritical = UnityEngine.Random.value < critChance;
        float baseBeamDamage = CalculateBaseBeamDamage?.Invoke(beamDamage) ?? beamDamage;
        float dmg = baseBeamDamage * (isCritical ? critMultiplier : 1f);
        beam.Initialize(this, firePoint.transform, fakeTarget.transform, dmg, isCritical);

        if (controlPoint.TryGetComponent<CameraRecoil>(out var recoil)) recoil.PlayRecoil();

        Destroy(fakeTarget, 2f);
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
        beamChains = data.beamMaxChains;
        beamStayTimeOnHit = data.beamStayTimeOnHit;
        fireRate = CalculateFireRate(data.fireRate);
        range = data.range;
        critChance = CalculateCritChance(data.critChance);
        critMultiplier = data.critMultiplier;

        capsuleCollider.radius = EffectiveRange(range);
        SetRangeProjector(EffectiveRange(range));
    }

    public void SetDamageCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateBaseBeamDamage = f;
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

    public void ActivateStim()
    {
        if (stimActive || stimCoolingDown) return;

        stimActive = true;
        stimTimer = stimDuration;
        stimCoolingDown = false;

        baseBeamDamage = beamDamage;
        baseBeamChainRadius = beamChainRadius;
        baseCritChance = critChance;
        baseCritMultiplier = critMultiplier;
        baseFireRate = fireRate;
        baseRange = range;

        beamDamage *= stimMultiplier;
        beamChainRadius *= stimMultiplier;
        critChance *= Mathf.Clamp01(critChance * stimMultiplier);
        critMultiplier *= stimMultiplier;
        fireRate = CalculateFireRate(fireRate) * stimMultiplier;
        range *= stimMultiplier;

        capsuleCollider.radius = EffectiveRange(range);
        SetRangeProjector(EffectiveRange(range));

        stimModeVFX.Play();
    }

    private void OnDestroy()
    {
        TowerMechanics.UnsubscribeAll(enemiesInRange, HandleEnemyDeath);
        enemiesInRange.Clear();
    }

    public void ActivateGainRangeOnHill() => hillRangeSkillActive = true;

    public float Range() => range;
    public void SetRange(float range) => this.range = range;

}
