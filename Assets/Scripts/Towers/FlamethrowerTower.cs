using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;

public class FlamethrowerTower : MonoBehaviour, ITower, ITowerSelectable, ITowerSellable, ITowerRotateable, ITowerStimulable, IAppliesDOT
{
    [Header("Stats")]
    [SerializeField] private float flameDamagePerPulse = 20f;
    [SerializeField] private float flamePulseInterval = 0.25f;
    [SerializeField] private float flameDuration = 3f;
    private Func<float, float> CalculateFlameDuration;
    [SerializeField] private float range = 10f;
    [SerializeField] private float flameAngle = 60f;
    [SerializeField] private float cooldownDuration = 2f;
    [SerializeField, Range(0f, 1f)] private float critChance = 0.15f;
    [SerializeField] private float critMultiplier = 2.0f;

    [Header("References")]
    [SerializeField] private GameObject flamePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject flamethrowerHead;
    [SerializeField] private GameObject flameCollider;
    [SerializeField] private DecalProjector rangeProjector;
    [SerializeField] private Renderer[] highlightRenderers;

    [Header("UI References")]
    [SerializeField] private TowerOverlayCatalog towerOverlayCatalog;
    [SerializeField] private GameObject towerRotationOverlayPrefab;
    [SerializeField] private CursorSettings cursorSettings;

    [Header("Upgrades")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxAllowedLevel = 1;
    [SerializeField] private TowerDataCatalog towerDataCatalog;

    [Header("Burn on Hit")]
    [SerializeField] private bool burnOnHitActive = false;

    [Header("Range on Hill")]
    [SerializeField] private bool hillRangeSkillActive = false;
    [SerializeField] private float heightRangeMultiplier = 0.05f;
    [SerializeField] private float baselineHeight = 0f;

    [Header("Stim Mode")]
    [SerializeField] private float stimMultiplier = 2f;
    [SerializeField] private float stimDuration = 5f;
    [SerializeField] private float stimCooldown = 5f;

    [Header("Sweep")]
    [SerializeField] private bool sweepEnabled = false;
    public void EnableSweep() => sweepEnabled = true;
    [SerializeField] private float sweepAmplitudeDegrees = 45f;
    [SerializeField] private float sweepCycleSeconds = 1.5f;
    [SerializeField] private float sweepReturnSpeed = 30f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem upgradeVFX;
    [SerializeField] private ParticleSystem stimModeVFX;
    [SerializeField] private ParticleSystem stimCooldownVFX;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();
    private IEnemy target;

    private bool underPlayerRotation = false;
    private bool isCoolingDown = false;
    private Flame activeFlame;
    private bool hasFired = false;

    private bool isSweeping;
    private float sweepBaseYaw;
    private float sweepTime;
    private Coroutine sweepReturnRoutine;

    public float DamagePerPulse => flameDamagePerPulse;
    public float FlamePulseInterval => flamePulseInterval;
    public float CritChance => critChance;
    public float CritMultiplier => critMultiplier;
    public bool BurnOnHitActive => burnOnHitActive;
    private float burnDuration = EnemyStatusEffect.BurnDefaultDuration;
    public float BurnDuration => burnDuration;

    private bool stimActive = false;
    private bool stimCoolingDown = false;
    private float stimTimer;
    private float stimCooldownTimer;
    public bool StimActive() => stimActive;
    public bool StimCoolingDown() => stimCoolingDown;
    public bool CanActivateStim() => !stimActive && !stimCoolingDown && (isCoolingDown || !hasFired);

    private float baseFlameDamagePerPulse;
    private float baseFlamePulseInterval;
    private float baseCritChance;
    private float baseCritMultiplier;

    private GameObject towerOverlayGO;
    private TowerOverlay towerOverlay;

    private GameObject towerRotationOverlayGO;
    private TowerRotationOverlay towerRotationOverlay;

    private TowerSelectionManager towerSelectionManager;

    private Func<float, float> CalculateBaseFlameDamagePerPulse;
    private Func<float, float> CalculateCritChance;

    private AudioSource activeFlameAudioSource;

    public TowerTypes TowerType() => TowerTypes.Flamethrower;

    public int CurrentLevel() => currentLevel;
    public int MaxAllowedLevel() => maxAllowedLevel;
    public bool CanUpgrade() => towerDataCatalog.CanUpgrade(TowerType(), CurrentLevel(), MaxAllowedLevel());

    public event Action<TowerTypes, float> OnDamageDealt;
    public event Action<TowerTypes> OnEnemyKilled;
    public event Action<TowerTypes> OnUpgrade;

    private OperationDataDontDestroy operationData;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float radius = EffectiveRange(range);
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
#endif

    private void Awake()
    {
        operationData = OperationDataDontDestroy.GetOrReadDev();
        maxAllowedLevel = TowerMechanics.GetMaxAllowedLevel(TowerType());

        Canvas canvas = FindFirstObjectByType<Canvas>();

        towerOverlayGO = Instantiate(towerOverlayCatalog.FromFactionAndTowerType(operationData.Faction, TowerType()), canvas.transform, true);
        towerOverlay = towerOverlayGO.GetComponent<TowerOverlay>();
        towerOverlay.Initialize(gameObject);
        towerOverlay.Hide();

        towerRotationOverlayGO = Instantiate(towerRotationOverlayPrefab, canvas.transform, true);
        towerRotationOverlay = towerRotationOverlayGO.GetComponent<TowerRotationOverlay>();
        towerRotationOverlay.Initialize(flamethrowerHead.transform);
        towerRotationOverlay.Hide();

        towerSelectionManager = FindFirstObjectByType<TowerSelectionManager>();
    }

    private void Start()
    {
        Assert.IsNotNull(flameCollider);
        flameCollider.transform.localScale = new(EffectiveRange(range), EffectiveRange(range), EffectiveRange(range));

        Assert.IsNotNull(rangeProjector);
        ShowRange(false);
        SetRangeProjector(EffectiveRange(range));

        Vector3 flamePosition = new(firePoint.position.x, 2f, firePoint.position.z);
        GameObject flame = Instantiate(flamePrefab, flamePosition, firePoint.rotation);
        activeFlame = flame.GetComponent<Flame>();
        activeFlame.OnDamageDealt += HandleDamageDealt;
        activeFlame.OnEnemyKilled += HandleEnemyKilled;
        activeFlame.Initialize(this, EffectiveRange(range));
        activeFlame.SetBurnDuration(burnDuration);
        flame.SetActive(false);
    }

    public void SetDotEnabled(bool enabled) => burnOnHitActive = enabled;
    public void SetDotDuration(float burnDuration) => this.burnDuration = burnDuration;

    private float EffectiveRange(float r)
    {
        if (!hillRangeSkillActive) return r;

        float height = transform.position.y - baselineHeight;
        float heightBonus = Mathf.Max(0f, 1f + height * heightRangeMultiplier);
        return r * heightBonus;
    }

    private void ShowRange(bool show)
    {
        rangeProjector.gameObject.SetActive(show);
    }

    private void SetRangeProjector(float range)
    {
        var size = rangeProjector.size;
        size.x = size.y = range;
        rangeProjector.size = size;
    }

    private void Update()
    {
        if (underPlayerRotation) return;

        if (stimActive || stimCoolingDown) return;

        if (!isCoolingDown && enemiesInRange.Count > 0 && activeFlame != null)
        {
            Shoot();
        }
    }

    private void EndStim()
    {
        stimActive = false;
        stimCoolingDown = true;
        stimCooldownTimer = stimCooldown;

        flameDamagePerPulse = baseFlameDamagePerPulse;
        flamePulseInterval = baseFlamePulseInterval;
        critChance = baseCritChance;
        critMultiplier = baseCritMultiplier;

        stimModeVFX.Stop(withChildren: true);

        if (activeFlame != null)
        {
            activeFlame.StopFlame();
            activeFlame.gameObject.SetActive(false);
        }

        if (sweepEnabled)
            EndSweep();

        stimCooldownVFX.Play();

        StartCoroutine(StimCooldownRoutine());
    }

    private IEnumerator StimCooldownRoutine()
    {
        while (stimCooldownTimer > 0f)
        {
            stimCooldownTimer -= Time.deltaTime;
            yield return null;
        }

        stimCoolingDown = false;
        stimCooldownVFX.Stop(withChildren: true);
    }

    private void LateUpdate()
    {
        if (activeFlame != null)
        {
            flameCollider.transform.SetPositionAndRotation(new(firePoint.position.x, 0f, firePoint.position.z), firePoint.rotation);
            rangeProjector.transform.SetPositionAndRotation(new(firePoint.position.x, 0f, firePoint.position.z), firePoint.rotation);
            activeFlame.transform.SetPositionAndRotation(new(firePoint.position.x, 0f, firePoint.position.z), firePoint.rotation);
        }
    }

    private void Shoot()
    {
        if (isCoolingDown || activeFlame == null) return;

        if (!hasFired) hasFired = true;

        activeFlame.gameObject.SetActive(true);
        activeFlame.Initialize(this, EffectiveRange(range));
        activeFlame.StartFlame(CalculateBaseFlameDamagePerPulse);

        activeFlameAudioSource = SoundManagersDontDestroy.GerOrCreate().SoundFX.PlayLoopedSoundFX(SoundFXType.FlamethrowerShoot, transform);

        StartCoroutine(CooldownRoutine(CalculateFlameDuration(flameDuration)));
    }

    public void SetFlameDurationCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateFlameDuration = f;
    }

    public List<IEnemy> GetCurrentEnemiesInRange() => enemiesInRange.Values.Where(e => e != null).ToList();

    private IEnumerator CooldownRoutine(float duration)
    {
        isCoolingDown = true;
        towerOverlay.AdjustOverlayButtons();
        yield return new WaitForSeconds(duration);

        activeFlame.StopFlame();
        activeFlame.gameObject.SetActive(false);

        SoundManagersDontDestroy.GerOrCreate().SoundFX.StopSoundFX(activeFlameAudioSource, 0.25f);
        activeFlameAudioSource = null;

        yield return new WaitForSeconds(cooldownDuration);
        isCoolingDown = false;
        towerOverlay.AdjustOverlayButtons();
    }

    private void BeginSweep()
    {
        isSweeping = true;
        sweepTime = 0f;
        sweepBaseYaw = flamethrowerHead.transform.eulerAngles.y;

        if (sweepReturnRoutine != null)
        {
            StopCoroutine(sweepReturnRoutine);
            sweepReturnRoutine = null;
        }
    }

    private void EndSweep()
    {
        if (!isSweeping) return;
        isSweeping = false;

        if (sweepReturnRoutine != null)
            StopCoroutine(sweepReturnRoutine);

        sweepReturnRoutine = StartCoroutine(ReturnHeadToBaseYaw());
    }

    private IEnumerator ReturnHeadToBaseYaw()
    {
        float currentYaw = flamethrowerHead.transform.eulerAngles.y;

        while (true)
        {
            if (isSweeping)
            {
                sweepReturnRoutine = null;
                yield break;
            }

            float targetYaw = sweepBaseYaw;
            float delta = Mathf.DeltaAngle(currentYaw, targetYaw);

            if (Mathf.Abs(delta) < 0.05f)
            {
                currentYaw = targetYaw;
                Vector3 doneE = flamethrowerHead.transform.eulerAngles;
                doneE.y = currentYaw;
                flamethrowerHead.transform.eulerAngles = doneE;
                break;
            }

            float step = Mathf.Sign(delta) * Mathf.Min(Mathf.Abs(delta), sweepReturnSpeed * Time.deltaTime);
            currentYaw += step;

            Vector3 e = flamethrowerHead.transform.eulerAngles;
            e.y = currentYaw;
            flamethrowerHead.transform.eulerAngles = e;

            yield return null;
        }

        sweepReturnRoutine = null;
    }

    private void UpdateSweep()
    {
        if (!sweepEnabled) return;

        sweepTime += Time.deltaTime;
        if (sweepCycleSeconds <= 0f) return;

        float phase = (sweepTime / sweepCycleSeconds) * Mathf.PI * 2f;
        float offset = sweepAmplitudeDegrees * Mathf.Sin(phase);

        Vector3 e = flamethrowerHead.transform.eulerAngles;
        e.y = sweepBaseYaw + offset;
        flamethrowerHead.transform.eulerAngles = e;
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

    private void HandleDamageDealt(float damage) => OnDamageDealt?.Invoke(TowerType(), damage);

    private void HandleEnemyKilled() => OnEnemyKilled?.Invoke(TowerType());

    public int InstanceID() => gameObject.GetInstanceID();

    public void Select()
    {
        ShowRange(true);
        towerOverlay.Show();
        towerRotationOverlay.Hide();
        TowerMechanics.ApplyHighlight(highlightRenderers, TowerMechanics.SelectedColor);
    }

    public void Deselect()
    {
        ShowRange(false);
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
        critChance = CalculateCritChance(data.critChance);
        critMultiplier = data.critMultiplier;

        if (activeFlame != null)
            activeFlame.UpdateRange(EffectiveRange(range));
        SetRangeProjector(EffectiveRange(range));

        OnUpgrade?.Invoke(TowerType());
    }

    public void SetDamageCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateBaseFlameDamagePerPulse = f;
    }
    public void SetFireRateCalculation(Func<float, float> f)
    {
        // flamethrower doesn't have fire rate
    }

    public void SetCritChangeCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateCritChance = f;
        critChance = CalculateCritChance(critChance);
    }

    public void RecalctCritChance()
    {
        critChance = CalculateCritChance(critChance);
    }


    public void ActivateStim()
    {
        if (stimActive || stimCoolingDown) return;

        stimActive = true;
        stimTimer = stimDuration;
        stimCoolingDown = false;

        baseFlameDamagePerPulse = flameDamagePerPulse;
        baseFlamePulseInterval = flamePulseInterval;
        baseCritChance = critChance;
        baseCritMultiplier = critMultiplier;

        flameDamagePerPulse *= stimMultiplier;
        flamePulseInterval /= stimMultiplier;
        critChance *= Mathf.Clamp01(critChance * stimMultiplier);
        critMultiplier *= stimMultiplier;

        stimModeVFX.Play();

        isCoolingDown = false;
        if (activeFlame != null)
        {
            activeFlame.gameObject.SetActive(true);
            if (!activeFlame.IsActive)
                activeFlame.StartFlame(CalculateBaseFlameDamagePerPulse);
        }

        StartCoroutine(StimLoop());
    }

    private IEnumerator StimLoop()
    {
        while (stimTimer > 0f)
        {
            stimTimer -= Time.deltaTime;
            UpdateSweep();
            yield return null;
        }

        EndStim();
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