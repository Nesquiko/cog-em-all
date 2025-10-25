using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CapsuleCollider))]
public class TeslaTower : MonoBehaviour, ITowerSelectable
{
    [Header("Stats")]
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
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private TowerOverlay towerOverlayPrefab;
    [SerializeField] private CursorSettings cursorSettings;

    private readonly Dictionary<int, Enemy> enemiesInRange = new();
    private Enemy target;
    private float fireCooldown = 0f;

    private TowerOverlay activeTowerOverlay;

    void OnDrawGizmosSelected()
    {
        TowerMechanics.DrawRangeGizmos(transform.position, Color.cyan, range);
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

        if (beamGO.TryGetComponent<Beam>(out var beam))
        {
            bool isCritical = Random.value < critChance;
            float damage = beam.BaseDamage;
            if (isCritical) damage *= critMultiplier;
            beam.Initialize(firePoint, enemy.transform, damage, isCritical);
        }
    }

    private void OnDestroy()
    {
        TowerMechanics.UnsubscribeAll(enemiesInRange, HandleEnemyDeath);
    }

    public void Select()
    {
        rangeIndicator.SetActive(true);
        TowerMechanics.ApplyHighlight(highlightRenderers, TowerMechanics.SelectedColor);

        if (activeTowerOverlay == null)
        {
            activeTowerOverlay = Instantiate(towerOverlayPrefab, uiCanvas.transform);
            activeTowerOverlay.SetTarget(transform);
        }
    }

    public void Deselect()
    {
        rangeIndicator.SetActive(false);
        TowerMechanics.ClearHighlight(highlightRenderers);

        if (activeTowerOverlay != null)
        {
            Destroy(activeTowerOverlay.gameObject);
            activeTowerOverlay = null;
        }
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
}
