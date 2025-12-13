using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CapsuleCollider))]
public class GatlingTower : MonoBehaviour, ITower, ITowerSelectable, ITowerSellable, ITowerControllable, ITowerStimulable
{
    [Header("Stats")]
    [SerializeField] private float bulletDamage = 50f;
    [SerializeField] private float bulletSpeed = 100f;
    [SerializeField] private float bulletLifetime = 3f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float range = 30f;
    [SerializeField, Range(0f, 1f)] private float critChance = 0.15f;
    [SerializeField] private float critMultiplier = 2.0f;

    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform gatlingHead;
    [SerializeField] private GameObject gatlingSeat;
    [SerializeField] private Transform gatlingGunL;
    [SerializeField] private Transform gatlingGunR;
    [SerializeField] private Transform gatlingFirePointL;
    [SerializeField] private Transform gatlingFirePointR;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private DecalProjector rangeProjector;
    [SerializeField] private Renderer[] highlightRenderers;

    [Header("UI References")]
    [SerializeField] private TowerOverlayCatalog towerOverlayCatalog;
    [SerializeField] private CursorSettings cursorSettings;

    [Header("Tower Control Mode")]
    [SerializeField] private Transform controlPoint;
    [SerializeField] private float sensitivity = 0.75f;

    [Header("Infinite Range")]
    [SerializeField] private bool infiniteRangeActive = true;
    [SerializeField] private float manualBulletRange = 1000f;

    [Header("Upgrades")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxAllowedLevel = 1;
    [SerializeField] private TowerDataCatalog towerDataCatalog;
    [SerializeField] private GameObject level2;
    [SerializeField] private GameObject level3;
    [SerializeField] private GameObject level3Gear1;
    [SerializeField] private GameObject level3Gear2;
    [SerializeField] private float gearSpinSpeed = 180f;

    [Header("Slow on Hit")]
    [SerializeField] private bool slowOnHitActive = false;

    [Header("Armor rending")]
    [SerializeField] private bool armorRendingActive = false;
    [SerializeField] private int maxArmourRendingStacks = EnemyStatusEffect.ArmorShredDefaultMaxStacks;
    public int MaxArmorRendingStacks => maxArmourRendingStacks;

    [Header("Range on Hill")]
    [SerializeField] private bool hillRangeSkillActive = false;
    [SerializeField] private float heightRangeMultiplier = 0.05f;
    [SerializeField] private float baselineHeight = 0f;

    [Header("Stim Mode")]
    [SerializeField] private float stimMultiplier = 2f;
    [SerializeField] private float stimDuration = 5f;
    [SerializeField] private float stimCooldown = 5f;

    [Header("Progressive Increase")]
    [SerializeField] private bool progressiveIncreaseActive = true;
    [SerializeField] private float stimMultiplierMultiplier = 1.5f;

    [Header("Recoil")]
    [SerializeField, Min(1)] private int barrelsPerGun = 8;
    [SerializeField] private float recoilDistance = 0.2f;
    [SerializeField] private float recoilSpeed = 20f;
    [SerializeField] private float recoilReturnSpeed = 5f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem muzzleFlashL;
    [SerializeField] private ParticleSystem muzzleFlashR;
    [SerializeField] private ParticleSystem upgradeVFX;
    [SerializeField] private ParticleSystem stimModeVFX;
    [SerializeField] private ParticleSystem stimCooldownLeftVFX;
    [SerializeField] private ParticleSystem stimCooldownRightVFX;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();
    private IEnemy target;
    private float fireCooldown = 0f;

    private Vector3 gunPositionL;
    private Vector3 gunPositionR;
    private Coroutine recoilRoutineL;
    private Coroutine recoilRoutineR;
    private float barrelAngleStep;
    private float spinDuration;
    private float spinElapsedL;
    private float spinElapsedR;
    private float startAngleL;
    private float startAngleR;
    private float targetAngleL;
    private float targetAngleR;
    private bool spinningL;
    private bool spinningR;

    private bool stimActive = false;
    private bool stimCoolingDown = false;
    private float stimTimer;
    private float stimCooldownTimer;
    public bool StimActive() => stimActive;
    public bool StimCoolingDown() => stimCoolingDown;
    public bool CanActivateStim() => !stimActive && !stimCoolingDown;

    private float baseBulletDamage;
    private float baseCritChance;
    private float baseCritMultiplier;
    private float baseFireRate;
    private float baseRange;

    private bool shootFromLeftFirePoint = true;

    private bool underPlayerControl;
    private float yaw;
    private float pitch;

    private GameObject towerOverlayGO;
    private TowerOverlay towerOverlay;

    private TowerSelectionManager towerSelectionManager;

    private Func<float, float> CalculateBaseBulletDamage;
    private Func<float, float> CalculateFireRate;
    private Func<float, float> CalculateCritChance;

    public float BulletLifetime => bulletLifetime;
    public float BulletSpeed => bulletSpeed;
    public bool InfiniteRange => underPlayerControl && infiniteRangeActive;
    public float ManualBulletRange => manualBulletRange;
    public float TowerRange => EffectiveRange(range);
    public bool SlowOnHitActive => slowOnHitActive;
    public bool ArmorRendingActive => armorRendingActive;

    public TowerTypes TowerType() => TowerTypes.Gatling;

    public int CurrentLevel() => currentLevel;
    public int MaxAllowedLevel() => maxAllowedLevel;
    public bool CanUpgrade() => towerDataCatalog.CanUpgrade(TowerType(), CurrentLevel(), MaxAllowedLevel());

    public Transform GetControlPoint() => controlPoint;

    private OperationDataDontDestroy operationData;

    private void OnDrawGizmosSelected()
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

        barrelAngleStep = 360f / barrelsPerGun;
        spinDuration = 2f / fireRate;
    }

    private void Start()
    {
        Assert.IsNotNull(capsuleCollider);
        capsuleCollider.radius = EffectiveRange(range);

        Assert.IsNotNull(rangeProjector);
        ShowRange(false);
        SetRangeProjector(EffectiveRange(range));

        if (gatlingGunL != null) gunPositionL = gatlingGunL.localPosition;
        if (gatlingGunR != null) gunPositionR = gatlingGunR.localPosition;
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

        TowerMechanics.RotateTowardTarget(gatlingHead, target.Transform);

        UpdateGunSpin();
    }

    private void HandleStimUpdate()
    {
        if (stimActive)
        {
            if (progressiveIncreaseActive)
            {
                float elapsed = stimDuration - stimTimer;
                float progress = Mathf.Clamp01(elapsed / stimDuration);
                progress = Mathf.SmoothStep(0f, 1f, progress);

                float dynamicMultiplier = Mathf.Lerp(stimMultiplier, stimMultiplier * stimMultiplierMultiplier, progress);

                bulletDamage = baseBulletDamage * dynamicMultiplier;
                critChance = Mathf.Clamp01(baseCritChance * dynamicMultiplier);
                critMultiplier = baseCritMultiplier * dynamicMultiplier;
                fireRate = CalculateFireRate(baseFireRate) * dynamicMultiplier;
                range = baseRange * dynamicMultiplier;

                capsuleCollider.radius = EffectiveRange(range);
                SetRangeProjector(EffectiveRange(range));

                var emission = stimModeVFX.emission;
                emission.rateOverTime = Mathf.Lerp(10, 150, progress);
            }
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
                stimCooldownLeftVFX.Stop(withChildren: true);
                stimCooldownRightVFX.Stop(withChildren: true);
            }
        }
    }

    private void EndStim()
    {
        stimActive = false;
        stimCoolingDown = true;
        stimCooldownTimer = stimCooldown;

        bulletDamage = baseBulletDamage;
        critChance = baseCritChance;
        critMultiplier = baseCritMultiplier;
        fireRate = CalculateFireRate(baseFireRate);
        range = baseRange;

        capsuleCollider.radius = EffectiveRange(range);
        SetRangeProjector(EffectiveRange(range));

        stimModeVFX.Stop(withChildren: true);
        stimCooldownLeftVFX.Play();
        stimCooldownRightVFX.Play();
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
        Transform firePoint = shootFromLeftFirePoint ? gatlingFirePointL : gatlingFirePointR;
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        bool isCritical = UnityEngine.Random.value < critChance;
        float dmg = CalculateBaseBulletDamage?.Invoke(bulletDamage) ?? bulletDamage;
        if (isCritical) dmg *= critMultiplier;

        bullet.Initialize(this, enemy.Transform, dmg, isCritical);

        HandleRecoil();

        shootFromLeftFirePoint = !shootFromLeftFirePoint;
    }

    private void HandleRecoil()
    {
        Transform gun = shootFromLeftFirePoint ? gatlingGunL : gatlingGunR;

        if (gun == null) return;

        if (shootFromLeftFirePoint)
        {
            if (recoilRoutineL != null) StopCoroutine(recoilRoutineL);
            recoilRoutineL = StartCoroutine(RecoilKick(gun, gunPositionL));
            muzzleFlashL.Play();
            StartGunSpinLeft();
        }
        else
        {
            if (recoilRoutineR != null) StopCoroutine(recoilRoutineR);
            recoilRoutineR = StartCoroutine(RecoilKick(gun, gunPositionR));
            muzzleFlashR.Play();
            StartGunSpinRight();
        }
    }

    private void StartGunSpinLeft()
    {
        startAngleL = gatlingGunL.localEulerAngles.z;
        targetAngleL = startAngleL + barrelAngleStep;
        spinElapsedL = 0f;
        spinningL = true;
    }

    private void StartGunSpinRight()
    {
        startAngleR = gatlingGunR.localEulerAngles.z;
        targetAngleR = startAngleR - barrelAngleStep;
        spinElapsedR = 0f;
        spinningR = true;
    }

    private IEnumerator RecoilKick(Transform gun, Vector3 defaultLocalPosition)
    {
        Vector3 start = gun.localPosition;
        Vector3 back = defaultLocalPosition - Vector3.forward * recoilDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            gun.localPosition = Vector3.Lerp(start, back, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilReturnSpeed;
            gun.localPosition = Vector3.Lerp(back, defaultLocalPosition, t);
            yield return null;
        }

        gun.localPosition = defaultLocalPosition;
    }

    private void UpdateGunSpin()
    {
        if (spinningL)
        {
            spinElapsedL += Time.deltaTime;
            float t = Mathf.Clamp01(spinElapsedL / spinDuration);
            float angle = Mathf.LerpAngle(startAngleL, targetAngleL, t);
            gatlingGunL.localRotation = Quaternion.Euler(0f, 0f, angle);
            if (t >= 1f) spinningL = false;
        }

        if (spinningR)
        {
            spinElapsedR += Time.deltaTime;
            float t = Mathf.Clamp01(spinElapsedR / spinDuration);
            float angle = Mathf.LerpAngle(startAngleR, targetAngleR, t);
            gatlingGunR.localRotation = Quaternion.Euler(0f, 0f, angle);
            if (t >= 1f) spinningR = false;
        }
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
            if (gatlingSeat != null) StartCoroutine(FadeGatlingSeat(fadeOut: true, delayBefore: 1f));

            target = null;

            Vector3 euler = gatlingHead.rotation.eulerAngles;
            yaw = euler.y;
            pitch = euler.x;

            if (controlPoint != null)
                controlPoint.rotation = gatlingHead.rotation;
        }
        else
        {
            if (gatlingSeat != null) StartCoroutine(FadeGatlingSeat(fadeOut: false, delayBefore: 0f));
        }
    }

    private IEnumerator FadeGatlingSeat(bool fadeOut, float delayBefore = 0f)
    {
        if (delayBefore > 0f)
            yield return new WaitForSeconds(delayBefore);

        float startAlpha = fadeOut ? 1f : 0f;
        float endAlpha = fadeOut ? 0f : 1f;
        float duration = fadeOut ? 0.8f : 0.6f;

        var renderer = gatlingSeat.GetComponent<Renderer>();
        Color c = renderer.material.color;
        c.a = startAlpha;
        renderer.material.color = c;

        if (!fadeOut)
            gatlingSeat.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            c.a = alpha;
            renderer.material.color = c;
            yield return null;
        }

        c.a = endAlpha;
        renderer.material.color = c;

        if (fadeOut)
            gatlingSeat.SetActive(false);
    }

    public void HandlePlayerAim(Vector2 mouseDelta)
    {
        yaw += mouseDelta.x * sensitivity;
        pitch -= mouseDelta.y * sensitivity;

        pitch = Mathf.Clamp(pitch, -80f, 80f);

        Quaternion lookRot = Quaternion.Euler(pitch, yaw, 0f);

        gatlingHead.rotation = lookRot;

        if (controlPoint != null && controlPoint.parent != gatlingHead)
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
        var gun = shootFromLeftFirePoint ? gatlingGunL : gatlingGunR;
        var firePoint = shootFromLeftFirePoint ? gatlingFirePointL : gatlingFirePointR;

        Vector3 aimPoint = firePoint.position + firePoint.forward * 100f;
        GameObject fakeTarget = new("GatlingManualAimTarget");
        fakeTarget.transform.position = aimPoint;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(firePoint.forward, Vector3.up));
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        bool isCritical = UnityEngine.Random.value < critChance;
        float baseBulletDmg = CalculateBaseBulletDamage?.Invoke(bulletDamage) ?? bulletDamage;
        float dmg = baseBulletDmg * (isCritical ? critMultiplier : 1f);
        bullet.Initialize(this, fakeTarget.transform, dmg, isCritical);

        if (controlPoint.TryGetComponent<CameraRecoil>(out var recoil)) recoil.PlayRecoil();

        if (gun == null) return;

        if (shootFromLeftFirePoint)
        {
            if (recoilRoutineL != null) StopCoroutine(recoilRoutineL);
            recoilRoutineL = StartCoroutine(RecoilKick(gun, gunPositionL));
            muzzleFlashL.Play();
        }
        else
        {
            if (recoilRoutineR != null) StopCoroutine(recoilRoutineR);
            recoilRoutineR = StartCoroutine(RecoilKick(gun, gunPositionR));
            muzzleFlashR.Play();
        }

        shootFromLeftFirePoint = !shootFromLeftFirePoint;

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
        if (baseData is not GatlingTowerData data) return;

        upgradeVFX.Play();

        towerSelectionManager.DeselectCurrent();

        currentLevel = data.Level;
        if (currentLevel == 2) level2.SetActive(true);
        if (currentLevel == 3)
        {
            level3.SetActive(true);
            StartCoroutine(SpinLevel3Gear(level3Gear1.transform));
            StartCoroutine(SpinLevel3Gear(level3Gear2.transform));
        }

        bulletDamage = data.bulletDamage;
        bulletSpeed = data.bulletSpeed;
        bulletLifetime = data.bulletLifetime;
        fireRate = CalculateFireRate(data.fireRate);
        range = data.range;
        critChance = CalculateCritChance(data.critChance);
        critMultiplier = data.critMultiplier;

        capsuleCollider.radius = EffectiveRange(range);
        SetRangeProjector(EffectiveRange(range));
    }

    private IEnumerator SpinLevel3Gear(Transform gear)
    {
        while (true)
        {
            gear.Rotate(Vector3.right, gearSpinSpeed * Time.deltaTime, Space.Self);
            yield return null;
        }
    }

    public void SetDamageCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateBaseBulletDamage = f;
    }

    public void SetFireRateCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateFireRate = f;
        fireRate = CalculateFireRate(fireRate);
    }

    public void SetDotDuration(float duration)
    {
        // gatling doesn't have dot
    }

    public void RecalctCritChance()
    {
        critChance = CalculateCritChance(critChance);
    }

    public void SetCritChangeCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateCritChance = f;
        critChance = CalculateCritChance(critChance);
    }

    public void ActivateStim()
    {
        if (stimActive || stimCoolingDown) return;

        stimActive = true;
        stimTimer = stimDuration;
        stimCoolingDown = false;

        baseBulletDamage = bulletDamage;
        baseCritChance = critChance;
        baseCritMultiplier = critMultiplier;
        baseFireRate = fireRate;
        baseRange = range;

        bulletDamage *= stimMultiplier;
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

    public void SetMaxRendingStacks(int max)
    {
        maxArmourRendingStacks = max;
    }

    public void ActivateGainRangeOnHill()
    {
        hillRangeSkillActive = true;
    }
}
