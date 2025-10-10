using System;
using System.Collections.Generic;
using UnityEngine;

public static class TowerMechanics
{
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
}
