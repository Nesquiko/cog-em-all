using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Beam : MonoBehaviour, IDamageSource
{
    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject executedPopupPrefab;

    private TeslaTower owner;
    private Transform firePoint;
    private Transform initialTarget;
    private float damage;
    private bool crit;

    public DamageSourceType Type() => DamageSourceType.Beam;

    public void Initialize(TeslaTower ownerTower, Transform from, Transform to, float dmg, bool isCritical)
    {
        Assert.IsNotNull(from);
        Assert.IsNotNull(to);
        owner = ownerTower;
        firePoint = from;
        initialTarget = to;
        damage = dmg;
        crit = isCritical;
    }

    private void Start()
    {
        if (initialTarget == null)
        {
            StartCoroutine(StraightBeamRoutine());
            return;
        }

        if (!initialTarget.TryGetComponent<IEnemy>(out _))
        {
            StartCoroutine(StraightBeamRoutine());
            return;
        }

        StartCoroutine(ChainRoutine());
    }

    private IEnumerator StraightBeamRoutine()
    {
        lineRenderer.positionCount = 2;
        Vector3 start = firePoint != null ? firePoint.position : transform.position;
        Vector3 end = start + (firePoint != null ? firePoint.forward : transform.forward) * 100f;

        DamageEnemiesAlongLine(start, end);

        float t = 0f;
        while (t < 0.05f)
        {
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void DamageEnemiesAlongLine(Vector3 start, Vector3 end)
    {
        Vector3 dir = end - start;
        float dist = dir.magnitude;
        dir.Normalize();

        if (Physics.Raycast(start, dir, out RaycastHit hit, dist, enemyMask))
        {
            IEnemy enemy = hit.collider.GetComponentInParent<IEnemy>();
            if (enemy != null)
            {
                DealDamageOrExecute(enemy, damage);
                StartCoroutine(ChainFromEnemy(enemy));
            }
        }
    }

    private IEnumerator ChainFromEnemy(IEnemy first)
    {
        List<IEnemy> chainTargets = BuildChain(first.Transform, owner.BeamMaxChains - 1);
        Vector3 currentPos = first.Transform.position;

        foreach (var enemy in chainTargets)
        {
            if (enemy == null) continue;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, currentPos);
            lineRenderer.SetPosition(1, enemy.Transform.position);

            DealDamageOrExecute(enemy, damage * 0.5f);
            currentPos = enemy.Transform.position;

            yield return new WaitForSeconds(owner.BeamStayTimeOnHit);
        }

        Destroy(gameObject);
    }

    private IEnumerator ChainRoutine()
    {
        if (!initialTarget.TryGetComponent<IEnemy>(out var firstEnemy))
        {
            yield return StraightBeamRoutine();
            yield break;
        }

        List<IEnemy> chainTargets = BuildChain(initialTarget, owner.BeamMaxChains - 1)
            .Where(e => e != null)
            .ToList();

        if (chainTargets.Count == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        Vector3 currentPos = firePoint.position;
        lineRenderer.positionCount = 2;

        AnimationCurve widthCurve = new();
        widthCurve.AddKey(0f, 1.5f);
        widthCurve.AddKey(1f, 1.5f);
        lineRenderer.widthCurve = widthCurve;

        for (int i = 0; i < chainTargets.Count; i++)
        {
            var nextEnemy = chainTargets[i];
            if (nextEnemy.Equals(null) || nextEnemy.Transform.Equals(null)) continue;

            var targetPos = nextEnemy.Transform.position;

            float t = 0f;
            float distance = Vector3.Distance(currentPos, targetPos);

            while (t < 1f)
            {
                if (nextEnemy == null)
                {
                    Destroy(gameObject);
                    yield break;
                }

                t += Time.deltaTime * (owner.BeamSpeed / distance);
                Vector3 point = Vector3.Lerp(currentPos, targetPos, t);

                lineRenderer.SetPosition(0, currentPos);
                lineRenderer.SetPosition(1, point);

                yield return null;
            }
            DealDamageOrExecute(nextEnemy, damage);

            yield return new WaitForSeconds(owner.BeamStayTimeOnHit);

            currentPos = targetPos;
        }

        yield return new WaitForSeconds(0.05f);
        Destroy(gameObject);
    }

    private List<IEnemy> BuildChain(Transform startTransform, int maxAdditional)
    {
        List<IEnemy> chain = new();
        if (startTransform.TryGetComponent<IEnemy>(out var first))
        {
            chain.Add(first);
        }
        else return chain;

        var current = first;

        for (int i = 0; i < maxAdditional; i++)
        {
            var next = FindClosestEnemy(current.Transform.position, chain);
            if (next == null) break;

            chain.Add(next);
            current = next;
        }

        return chain;
    }

    private IEnemy FindClosestEnemy(Vector3 origin, List<IEnemy> exclude)
    {
        Collider[] hits = Physics.OverlapSphere(origin, owner.BeamChainRadius, enemyMask, QueryTriggerInteraction.Ignore);

        IEnemy closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;

            if (!hit.TryGetComponent<IEnemy>(out var e)) continue;
            if (exclude.Contains(e)) continue;

            float distance = Vector3.Distance(origin, e.Transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = e;
            }
        }

        return closest;
    }

    private void DealDamageOrExecute(IEnemy enemy, float dmg)
    {
        if (enemy == null) return;

        if (owner.ExecuteActive && enemy.HealthPointsNormalized <= owner.ExecuteThreshold)
        {
            enemy.TakeDamage(enemy.HealthPoints + 1f, Type(), true, effect: owner.DisableBuffsOnHitActive ? EnemyStatusEffect.DisableBuffs : null);
            return;
        }

        enemy.TakeDamage(dmg, Type(), crit);
    }
}