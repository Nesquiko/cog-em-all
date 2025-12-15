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
    Damage = 0,
    FireRate = 1,
    Range = 2,
    CritChange = 3,
    CritDamage = 4,
    FireTime = 5,
    ChainLength = 6,
    DotDuration = 7,
    MaxAppliedStacks = 8,
}

public interface ITower
{
    int InstanceID();
    TowerTypes TowerType();
    int CurrentLevel();
    int MaxAllowedLevel();
    void ApplyUpgrade(TowerDataBase data);
    void SetDamageCalculation(Func<float, float> f);
    void SetFireRateCalculation(Func<float, float> f);
    void ActivateGainRangeOnHill();
    void SetCritChangeCalculation(Func<float, float> f);
    void RecalctCritChance();

    event Action<TowerTypes, float> OnDamageDealt;
    event Action<TowerTypes> OnEnemyKilled;
    event Action<TowerTypes> OnUpgrade;
    float Range();
    void SetRange(float range);
}

public interface IAppliesDOT
{
    void SetDotEnabled(bool enabled);
    void SetDotDuration(float burnDuration);
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

public interface ITowerRotateable : ITower
{
    void ShowTowerRotationOverlay();
}

public interface ITowerStimulable : ITower
{
    bool StimActive();
    bool StimCoolingDown();
    void ActivateStim();
    bool CanActivateStim();
}

public static class TowerMechanics
{
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    private static Color defaultEmissionColor = Color.black;

    public static readonly Color HoverColor = new(0.6f, 0.7f, 1.0f);
    public static readonly Color SelectedColor = new(1.0f, 0.85f, 0.3f);
    private const float HighlightIntensity = 3.0f;

    public static IEnemy GetClosestEnemy(Vector3 towerPosition, IDictionary<int, IEnemy> enemies)
    {
        IEnemy closest = null;
        float minDist = float.MaxValue;

        foreach (var e in enemies.Values)
        {
            if (IsEnemyNull(e)) continue;
            float dist = (e.Transform.position - towerPosition).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = e;
            }
        }

        return closest;
    }

    public static IEnemy GetClosestMarkedEnemy(Vector3 towerPosition, Dictionary<int, IEnemy> enemies)
    {
        IEnemy best = null;
        float bestDistSq = float.MaxValue;
        foreach (var e in enemies.Values)
        {
            if (IsEnemyNull(e) || !e.Marked) continue;
            float d = (e.Transform.position - towerPosition).sqrMagnitude;
            if (d < bestDistSq)
            {
                bestDistSq = d;
                best = e;
            }
        }
        return best;
    }

    public static IEnemy SelectTargetWithMarkPriority(
        Vector3 towerPosition,
        Dictionary<int, IEnemy> enemiesInRange,
        IEnemy current,
        float range
    )
    {
        if (current != null && current.Marked && IsEnemyInRange(towerPosition, current, range))
            return current;

        var marked = GetClosestMarkedEnemy(towerPosition, enemiesInRange);
        if (marked != null && IsEnemyInRange(towerPosition, marked, range))
            return marked;

        var fallback = GetClosestEnemy(towerPosition, enemiesInRange);
        if (fallback != null && IsEnemyInRange(towerPosition, fallback, range))
            return fallback;

        return null;
    }

    public static bool IsEnemyInRange(Vector3 towerPosition, IEnemy enemy, float range)
    {
        if (enemy == null) return false;
        float distance = Vector3.Distance(towerPosition, enemy.Transform.position);
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

    public static void HandleTriggerEnter(Collider other, IDictionary<int, IEnemy> enemies, Action<IEnemy> deathAction)
    {
        if (!other.TryGetComponent<IEnemy>(out var enemy)) return;

        int id = enemy.GetInstanceID();
        if (enemies.ContainsKey(id)) return;

        enemies.Add(id, enemy);
        enemy.OnDeath += deathAction;
    }

    public static bool HandleTriggerExit(Collider other, IDictionary<int, IEnemy> enemies, Action<IEnemy> deathAction, IEnemy currentTarget, out IEnemy newTarget)
    {
        newTarget = currentTarget;

        if (!other.TryGetComponent<IEnemy>(out var enemy)) return false;

        int id = enemy.GetInstanceID();
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

    public static IEnemy HandleEnemyRemoval(IEnemy deadEnemy, IDictionary<int, IEnemy> enemies, IEnemy currentTarget)
    {
        if (deadEnemy == null) return currentTarget;

        int id = deadEnemy.GetInstanceID();
        if (enemies.ContainsKey(id))
        {
            enemies.Remove(id);
        }

        if (currentTarget == deadEnemy) currentTarget = null;

        return currentTarget;
    }

    public static void UnsubscribeAll(IDictionary<int, IEnemy> enemies, Action<IEnemy> deathAction)
    {
        if (enemies == null || enemies.Count == 0) return;

        foreach (var e in enemies.Values)
        {
            if (e != null) e.OnDeath -= deathAction;
        }
    }

#if UNITY_EDITOR
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
#endif

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

    public static int GetMaxAllowedLevel(TowerTypes type)
    {
        OperationDataDontDestroy operationData = OperationDataDontDestroy.GetOrReadDev();
        Dictionary<TowerTypes, int> unlockedTowerLevels = ModifiersCalculator.UnlockedTowerLevels(operationData.Modifiers);
        return unlockedTowerLevels.GetValueOrDefault(type, 1);
    }

    public static bool IsEnemyNull(IEnemy e) => e.Equals(null) || e.Transform.Equals(null);
}
