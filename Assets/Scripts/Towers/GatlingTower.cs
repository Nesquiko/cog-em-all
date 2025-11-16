using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CapsuleCollider))]
public class GatlingTower : MonoBehaviour, ITower, ITowerSelectable, ITowerSellable, ITowerControllable
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
    [SerializeField] private GameObject rangeIndicator;
    [SerializeField] private Renderer[] highlightRenderers;

    [Header("UI References")]
    [SerializeField] private GameObject towerOverlayPrefab;
    [SerializeField] private CursorSettings cursorSettings;

    [Header("Tower Control Mode")]
    [SerializeField] private Transform controlPoint;
    [SerializeField] private float sensitivity = 0.75f;

    [Header("Upgrades")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private TowerDataCatalog towerDataCatalog;

    [Header("Recoil")]
    [SerializeField] private float recoilDistance = 0.2f;
    [SerializeField] private float recoilSpeed = 20f;
    [SerializeField] private float recoilReturnSpeed = 5f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem muzzleFlashL;
    [SerializeField] private ParticleSystem muzzleFlashR;
    [SerializeField] private ParticleSystem upgradeVFX;

    private readonly Dictionary<int, IEnemy> enemiesInRange = new();
    private IEnemy target;
    private float fireCooldown = 0f;

    private Vector3 gunPositionL;
    private Vector3 gunPositionR;
    private Coroutine recoilRoutineL;
    private Coroutine recoilRoutineR;

    private bool shootFromLeftFirePoint = true;

    private bool underPlayerControl;
    private float yaw;
    private float pitch;

    private GameObject towerOverlayGO;
    private TowerOverlay towerOverlay;

    private TowerSelectionManager towerSelectionManager;

    private Func<float, float> CalculateBaseBulletDamage;

    public float BulletLifetime => bulletLifetime;
    public float BulletSpeed => bulletSpeed;

    public TowerTypes TowerType() => TowerTypes.Gatling;

    public int CurrentLevel() => currentLevel;

    public bool CanUpgrade() => towerDataCatalog.CanUpgrade(TowerType(), CurrentLevel());

    public Transform GetControlPoint() => controlPoint;

    private void OnDrawGizmosSelected()
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
    }

    private void Start()
    {
        Assert.IsNotNull(capsuleCollider);
        capsuleCollider.radius = range;

        Assert.IsNotNull(rangeIndicator);
        rangeIndicator.SetActive(false);
        rangeIndicator.transform.localScale = new(range * 2, rangeIndicator.transform.localScale.y, range * 2);

        if (gatlingGunL != null) gunPositionL = gatlingGunL.localPosition;
        if (gatlingGunR != null) gunPositionR = gatlingGunR.localPosition;
    }

    private void Update()
    {
        if (underPlayerControl) return;

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

        TowerMechanics.RotateTowardTarget(gatlingHead, target.Transform, 10f);
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
        }
        else
        {
            if (recoilRoutineR != null) StopCoroutine(recoilRoutineR);
            recoilRoutineR = StartCoroutine(RecoilKick(gun, gunPositionR));
            muzzleFlashR.Play();
        }
    }

    private IEnumerator RecoilKick(Transform gun, Vector3 defaultLocalPosition)
    {
        Vector3 start = gun.localPosition;
        Vector3 back = defaultLocalPosition + gun.up * recoilDistance;

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

    public void OnPlayerTakeControl(bool active)
    {
        underPlayerControl = active;
        fireCooldown = 0f;

        if (active)
        {
            StartCoroutine(FadeGatlingSeat(fadeOut: true, delayBefore: 1f));

            target = null;

            Vector3 euler = gatlingHead.rotation.eulerAngles;
            yaw = euler.y;
            pitch = euler.x;

            if (controlPoint != null)
                controlPoint.rotation = gatlingHead.rotation;
        }
        else
        {
            StartCoroutine(FadeGatlingSeat(fadeOut: false, delayBefore: 0f));
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
        var firepoint = shootFromLeftFirePoint ? gatlingFirePointL : gatlingFirePointR;

        Vector3 aimPoint = firepoint.position + firepoint.forward * 100f;
        GameObject fakeTarget = new();
        fakeTarget.transform.position = aimPoint;

        GameObject bulletGO = Instantiate(bulletPrefab, firepoint.position, Quaternion.LookRotation(firepoint.forward, Vector3.up));
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

        bulletDamage = data.bulletDamage;
        bulletSpeed = data.bulletSpeed;
        bulletLifetime = data.bulletLifetime;
        fireRate = data.fireRate;
        range = data.range;
        critChance = data.critChance;
        critMultiplier = data.critMultiplier;

        capsuleCollider.radius = data.range;
        rangeIndicator.transform.localScale = new(range * 2, rangeIndicator.transform.localScale.y, range * 2);
    }

    public void SetDamageCalculation(Func<float, float> f)
    {
        Assert.IsNotNull(f);
        CalculateBaseBulletDamage = f;
    }

    private void OnDestroy()
    {
        TowerMechanics.UnsubscribeAll(enemiesInRange, HandleEnemyDeath);
        enemiesInRange.Clear();
    }
}
