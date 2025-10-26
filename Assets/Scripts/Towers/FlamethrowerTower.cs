using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class FlamethrowerTower : MonoBehaviour, ITowerSelectable
{
    [Header("Stats")]
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
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private TowerOverlay towerOverlayPrefab;
    [SerializeField] private CursorSettings cursorSettings;

    private readonly Dictionary<int, Enemy> enemiesInRange = new();
    private Enemy target;

    private bool isCoolingDown = false;
    private Flame activeFlame;

    public float CritChance => critChance;
    public float CritMultiplier => critMultiplier;

    private TowerOverlay activeTowerOverlay;

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

    private void Start()
    {
        Assert.IsNotNull(flameCollider);
        flameCollider.transform.localScale = new(range, range, range);

        Assert.IsNotNull(rangeIndicator);
        rangeIndicator.SetActive(false);
        rangeIndicator.transform.localScale = new(range, rangeIndicator.transform.localScale.y, range);

        if (flamePrefab != null)
        {
            Vector3 flamePosition = new(firePoint.position.x, 0f, firePoint.position.z);
            GameObject flame = Instantiate(flamePrefab, flamePosition, firePoint.rotation);
            activeFlame = flame.GetComponent<Flame>();
            activeFlame.SetOwner(this);
            activeFlame.SetRange(range);
            flame.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isCoolingDown && enemiesInRange.Count > 0 && activeFlame != null)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (isCoolingDown || activeFlame == null) return;

        activeFlame.gameObject.SetActive(true);
        activeFlame.SetOwner(this);
        activeFlame.SetRange(range);
        activeFlame.StartFlame();

        StartCoroutine(CooldownRoutine(activeFlame.FireDuration));
    }

    public List<Enemy> GetCurrentEnemiesInRange()
    {
        List<Enemy> currentEnemies = new();
        foreach (Enemy enemy in enemiesInRange.Values)
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
        e.OnDeath -= HandleEnemyDeath;
    }

    private void HandleEnemyDeath(Enemy deadEnemy)
    {
        target = TowerMechanics.HandleEnemyRemoval(deadEnemy, enemiesInRange, target);
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

    public TowerTypes TowerType() => TowerTypes.Flamethrower;
}
