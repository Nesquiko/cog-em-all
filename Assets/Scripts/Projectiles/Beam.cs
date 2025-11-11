using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Beam : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 200f;
    [SerializeField] private float chainRadius = 10f;
    [SerializeField] private int maxChains = 3;
    [SerializeField] private float stayTimeOnHit = 0.05f;

    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LayerMask enemyMask;

    private Transform firePoint;
    private Transform initialTarget;
    private float damage;
    private bool crit;

    public void Initialize(Transform from, Transform to, float dmg, bool isCritical)
    {
        Assert.IsNotNull(from);
        Assert.IsNotNull(to);
        firePoint = from;
        initialTarget = to;
        damage = dmg;
        crit = isCritical;
    }

    private void Start()
    {
        if (initialTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(ChainRoutine());
    }

    private IEnumerator ChainRoutine()
    {
        List<IEnemy> chainTargets = BuildChain(initialTarget, maxChains - 1)
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

                t += Time.deltaTime * (speed / distance);
                Vector3 point = Vector3.Lerp(currentPos, targetPos, t);

                lineRenderer.SetPosition(0, currentPos);
                lineRenderer.SetPosition(1, point);

                yield return null;
            }
            if (nextEnemy != null)
            {
                nextEnemy.TakeDamage(damage, crit);
            }

            yield return new WaitForSeconds(stayTimeOnHit);

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
        Collider[] hits = Physics.OverlapSphere(origin, chainRadius, enemyMask, QueryTriggerInteraction.Ignore);

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
}