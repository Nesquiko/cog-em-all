using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum TowerTypes
{
    Gatling = 0,
    Tesla = 1,
    Mortar = 2,
    Flamethrower = 3,
};

public enum TowerAttribute
{
    Damage,
    FireRate,
    Range,
}

public interface ITower
{
    TowerTypes TowerType();

    void SetDamageCalculation(Func<float, float> f);
}

public interface ITowerSelectable : ITower
{
    void Select();
    void Deselect();
    void OnHoverEnter();
    void OnHoverExit();
}

public interface ITowerControllable : ITower
{
    Transform GetControlPoint();
    void OnPlayerTakeControl(bool active);
    void HandlePlayerAim(Vector2 mouseDelta);
    void HandlePlayerFire();
}

public interface ITowerSellable : ITower
{
    void SellAndDestroy();
}

public interface ITowerUpgradeable : ITower
{
    int CurrentLevel();
    bool CanUpgrade();
    void ApplyUpgrade(TowerUpgradeData data);
}

public interface ITowerRotateable : ITower
{
    void ShowTowerRotationOverlay();
}

public static class TowerMechanics
{
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    private static Color defaultEmissionColor = Color.black;

    public static readonly Color HoverColor = new(0.6f, 0.7f, 1.0f);
    public static readonly Color SelectedColor = new(1.0f, 0.85f, 0.3f);
    private const float HighlightIntensity = 3.0f;

    public static Enemy GetClosestEnemy(Vector3 towerPosition, IDictionary<int, Enemy> enemies)
    {
        Enemy closest = null;
        float minDist = Mathf.Infinity;

        foreach (Enemy e in enemies.Values)
        {
            if (e == null) continue;

            float dist = Vector3.Distance(towerPosition, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = e;
            }
        }

        return closest;
    }

    public static bool IsEnemyInRange(Vector3 towerPosition, Enemy enemy, float range)
    {
        if (enemy == null) return false;
        float distance = Vector3.Distance(towerPosition, enemy.transform.position);
        return distance <= range;
    }

    public static void RotateTowardTarget(Transform towerHead, Transform target, float rotationSpeed = 5f)
    {
        if (towerHead == null || target == null) return;

        Vector3 direction = target.position - towerHead.position;

        if (direction == Vector3.zero) return;

        Quaternion desiredRotation = Quaternion.LookRotation(direction);
        towerHead.rotation = Quaternion.Lerp(
            towerHead.rotation,
            desiredRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public static void HandleTriggerEnter(Collider other, IDictionary<int, Enemy> enemies, Action<Enemy> deathAction)
    {
        if (!other.TryGetComponent<Enemy>(out var enemy)) return;

        int id = enemy.gameObject.GetInstanceID();
        if (enemies.ContainsKey(id)) return;

        enemies.Add(id, enemy);
        enemy.OnDeath += deathAction;
    }

    public static bool HandleTriggerExit(Collider other, IDictionary<int, Enemy> enemies, Action<Enemy> deathAction, Enemy currentTarget, out Enemy newTarget)
    {
        newTarget = currentTarget;

        if (!other.TryGetComponent<Enemy>(out var enemy)) return false;

        int id = enemy.gameObject.GetInstanceID();
        if (enemies.ContainsKey(id))
        {
            enemies.Remove(id);
            enemy.OnDeath -= deathAction;
        }

        if (currentTarget == enemy)
        {
            newTarget = null;
            return true;
        }

        return false;
    }

    public static Enemy HandleEnemyRemoval(Enemy deadEnemy, IDictionary<int, Enemy> enemies, Enemy currentTarget)
    {
        if (deadEnemy == null) return currentTarget;

        int id = deadEnemy.gameObject.GetInstanceID();
        if (enemies.ContainsKey(id))
        {
            enemies.Remove(id);
        }

        if (currentTarget == deadEnemy) currentTarget = null;

        return currentTarget;
    }

    public static void UnsubscribeAll(IDictionary<int, Enemy> enemies, Action<Enemy> deathAction)
    {
        if (enemies == null || enemies.Count == 0) return;

        foreach (var e in enemies.Values)
        {
            if (e != null) e.OnDeath -= deathAction;
        }
    }

    public static void DrawRangeGizmos(Vector3 position, Color? color, params float[] radii)
    {
        if (radii == null) return;

        Handles.color = color ?? Color.cyan;
        Vector3 center = new(position.x, 0f, position.z);
        foreach (float r in radii)
        {
            if (r <= 0f) continue;
            Handles.DrawWireDisc(center, Vector3.up, r);
        }
    }

    public static void DrawRangeGizmos(Vector3 position, IEnumerable<(float radius, Color? color)> ranges)
    {
        if (ranges == null) return;

        foreach (var (radius, color) in ranges)
        {
            if (radius <= 0f) continue;

            Handles.color = color ?? Color.cyan;
            Vector3 center = new(position.x, 0f, position.z);
            Handles.DrawWireDisc(center, Vector3.up, radius);
        }
    }

    public static void ApplyHighlight(Renderer[] renderers, Color color)
    {
        if (renderers == null || renderers.Length == 0) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;

            Material material = r.material;
            if (material.HasProperty(EmissionColorID))
            {
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                material.SetColor(EmissionColorID, color * HighlightIntensity);
            }
        }
    }

    public static void ClearHighlight(Renderer[] renderers)
    {
        if (renderers == null || renderers.Length == 0) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;

            Material material = r.material;
            if (material.HasProperty(EmissionColorID))
            {
                material.SetColor(EmissionColorID, defaultEmissionColor);
            }
        }
    }
}
