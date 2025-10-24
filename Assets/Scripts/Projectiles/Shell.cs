using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    [SerializeField] private float splashRadius = 5f;
    [SerializeField] private float damage = 120f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject explosionEffect;

    private Vector3 start;
    private Vector3 target;
    private float speed;
    private float arcHeight;
    private float travelDuration;
    private float elapsed;
    private bool launched;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }

    public void Launch(Vector3 targetPos, float launchSpeed, float arc)
    {
        start = transform.position;
        target = targetPos + Vector3.up * 0.5f;
        speed = launchSpeed;
        arcHeight = arc;
        launched = true;

        float distance = Vector3.Distance(start, target);
        travelDuration = Mathf.Clamp(1.5f, 0.8f, 2.5f);
        speed = distance / travelDuration;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!launched) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / travelDuration);

        Vector3 horizontal = Vector3.Lerp(start, target, t);
        float height = Mathf.Sin(t * Mathf.PI) * arcHeight;
        transform.position = new Vector3(horizontal.x, horizontal.y + height, horizontal.z);
    
        if (t >= 1f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Collider[] hits = Physics.OverlapSphere(transform.position, splashRadius);
        HashSet<Enemy> damaged = new();

        foreach (Collider c in hits)
        {
            if (c.TryGetComponent<Enemy>(out var e) && !damaged.Contains(e))
            {
                e.TakeDamage(damage);
                damaged.Add(e);
            }
        }

        Destroy(gameObject);
    }
}
