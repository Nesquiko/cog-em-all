using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Beam : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float baseDamage = 30f;
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
    public float BaseDamage => baseDamage;

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
        if (lineRenderer == null || firePoint == null || initialTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(ChainRoutine());
    }

    private IEnumerator ChainRoutine()
    {
        List<Enemy> chainTargets = BuildChain(initialTarget, maxChains - 1)
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
            Enemy nextEnemy = chainTargets[i];
            if (nextEnemy == null) continue;

            if (nextEnemy.Equals(null) || nextEnemy.gameObject == null) break;

            Vector3 targetPos = nextEnemy.transform.position;

            float t = 0f;
            float distance = Vector3.Distance(currentPos, targetPos);

            while (t < 1f)
            {
                if (nextEnemy == null || nextEnemy.gameObject == null)
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

    private List<Enemy> BuildChain(Transform startTransform, int maxAdditional)
    {
        List<Enemy> chain = new();
        if (startTransform.TryGetComponent<Enemy>(out var first))
        {
            chain.Add(first);
        }
        else return chain;

        Enemy current = first;

        for (int i = 0; i < maxAdditional; i++)
        {
            Enemy next = FindClosestEnemy(current.transform.position, chain);
            if (next == null) break;

            chain.Add(next);
            current = next;
        }

        return chain;
    }

    private Enemy FindClosestEnemy(Vector3 origin, List<Enemy> exclude)
    {
        Collider[] hits = Physics.OverlapSphere(origin, chainRadius, enemyMask, QueryTriggerInteraction.Ignore);

        Enemy closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;

            if (!hit.TryGetComponent<Enemy>(out var e)) continue;
            if (exclude.Contains(e)) continue;

            float distance = Vector3.Distance(origin, e.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = e;
            }
        }

        return closest;
    }
}