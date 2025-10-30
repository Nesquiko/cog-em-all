using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class MortarTower : MonoBehaviour, ITower, ITowerSelectable
{
    [Header("Stats")]
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
    [SerializeField] private GameObject towerOverlay;
    [SerializeField] private CursorSettings cursorSettings;

    [Header("Recoil")]
    [SerializeField] private float recoilDistance = 0.5f;
    [SerializeField] private float recoilSpeed = 20f;
    [SerializeField] private float recoilReturnSpeed = 5f;

    private readonly Dictionary<int, Enemy> enemiesInRange = new();
    private readonly HashSet<int> tooClose = new();
    private Enemy target;
    private float fireCooldown;

    private Vector3 cannonPivotDefaultPosition;
    private Coroutine recoilRoutine;

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

    void Start()
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

    void Update()
    {
        if (target == null || !IsEnemyValid(target.transform.position))
        {
            target = GetValidTarget();
            if (target == null) return;
        }

        RotateTowardTarget(target.transform);

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f && IsAimedAtTarget(target.transform))
        {
            Shoot(target);
            fireCooldown = 1f / fireRate;
        }
    }

    public void RegisterInRange(Enemy e)
    {
        int id = e.gameObject.GetInstanceID();
        if (enemiesInRange.ContainsKey(id)) return;
        enemiesInRange.Add(id, e);
        e.OnDeath += HandleEnemyDeath;
    }

    public void UnregisterOutOfRange(Enemy e)
    {
        int id = e.gameObject.GetInstanceID();
        enemiesInRange.Remove(id);
        tooClose.Remove(id);
        e.OnDeath -= HandleEnemyDeath;
    }

    public void RegisterTooClose(Enemy e)
    {
        tooClose.Add(e.gameObject.GetInstanceID());
    }

    public void UnregisterTooClose(Enemy e)
    {
        tooClose.Remove(e.gameObject.GetInstanceID());
    }

    private void HandleEnemyDeath(Enemy deadEnemy)
    {
        target = TowerMechanics.HandleEnemyRemoval(deadEnemy, enemiesInRange, target);
    }

    private void Shoot(Enemy enemy)
    {
        GameObject shellGO = Instantiate(shellPrefab, firePoint.position, firePoint.rotation);
        if (recoilRoutine != null) StopCoroutine(recoilRoutine);
        recoilRoutine = StartCoroutine(RecoilKick());
        
        if (shellGO.TryGetComponent<Shell>(out var shell))
        {
            bool isCritical = Random.value < critChance;
            float dmg = shell.BaseDamage;
            if (isCritical) dmg *= critMultiplier;

            shell.Launch(enemy.transform.position, dmg, isCritical, launchSpeed, arcHeight);
        }
    }

    private bool IsEnemyValid(Vector3 enemyPosition)
    {
        float distance = Vector3.Distance(transform.position, enemyPosition);
        return distance >= minRange && distance <= maxRange;
    }

    private Enemy GetValidTarget()
    {
        Enemy best = null;
        float bestDistance = Mathf.Infinity;

        foreach (var (id, enemy) in enemiesInRange)
        {
            if (tooClose.Contains(id)) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
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

    private void OnDestroy()
    {
        TowerMechanics.UnsubscribeAll(enemiesInRange, HandleEnemyDeath);
    }

    public void Select()
    {
        outerRangeIndicator.SetActive(true);
        innerRangeIndicator.SetActive(true);
        towerOverlay.SetActive(true);
        TowerMechanics.ApplyHighlight(highlightRenderers, TowerMechanics.SelectedColor);
    }

    public void Deselect()
    {
        outerRangeIndicator.SetActive(false);
        innerRangeIndicator.SetActive(false);
        towerOverlay.SetActive(false);
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
        TowerMechanics.ClearHighlight(highlightRenderers);
    }

    public TowerTypes TowerType() => TowerTypes.Mortar;
}
