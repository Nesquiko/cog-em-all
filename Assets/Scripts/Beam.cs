using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
    [SerializeField] private float damage = 150f;
    [SerializeField] private float chainRadius = 100f;
    [SerializeField] private int maxChains = 3;
    [SerializeField] private float speed = 700f;
    [SerializeField] private float stayTimeOnHit = 0.01f;
    [SerializeField] private LineRenderer lineRenderer;

    private Transform firePoint;
    private Transform initialTarget;

    public void Initialize(Transform from, Transform to)
    {
        firePoint = from;
        initialTarget = to;
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
        List<Enemy> chainTargets = BuildChain(initialTarget, maxChains - 1);

        Vector3 currentPos = firePoint.position;
        lineRenderer.positionCount = 2;

        AnimationCurve widthCurve = new();
        widthCurve.AddKey(0f, 3f);
        widthCurve.AddKey(1f, 3f);
        lineRenderer.widthCurve = widthCurve;

        for (int i = 0; i < chainTargets.Count; i++)
        {
            Enemy nextEnemy = chainTargets[i];
            if (nextEnemy == null)
            {
                continue;
            }

            Vector3 targetPos = nextEnemy.transform.position;

            float t = 0f;
            float distance = Vector3.Distance(currentPos, targetPos);

            while (t < 1f)
            {
                t += Time.deltaTime * (speed / distance);
                Vector3 point = Vector3.Lerp(currentPos, targetPos, t);

                lineRenderer.SetPosition(0, currentPos);
                lineRenderer.SetPosition(1, point);

                yield return null;
            }

            nextEnemy.TakeDamage(damage);

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

    private Enemy FindClosestEnemy(Vector3 pos, List<Enemy> exclude)
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy closest = null;
        float minDist = Mathf.Infinity;

        foreach (Enemy e in allEnemies)
        {
            if (exclude.Contains(e)) continue;
            float dist = Vector3.Distance(pos, e.transform.position);
            if (dist < minDist && dist <= chainRadius)
            {
                minDist = dist;
                closest = e;
            }
        }
        return closest;
    }
}