using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    [SerializeField] private float splashRadius = 10f;
    [SerializeField] private float lifetime = 5f;

    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private SphereCollider sphereCollider;

    [Header("VFX")]
    [SerializeField] private ParticleSystem shellExplosionVFX;
    [SerializeField] private float shellVFXScaleFactor = 3f;

    private Vector3 start;
    private Vector3 target;
    private float damage;
    private bool crit;
    private float arcHeight;
    private float travelDuration;
    private float elapsed;
    private bool launched;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }

    private void Awake()
    {
        shellExplosionVFX.transform.localScale = new(
            splashRadius / shellVFXScaleFactor,
            splashRadius / shellVFXScaleFactor,
            splashRadius / shellVFXScaleFactor
        );
    }

    public void Launch(Vector3 targetPos, float dmg, bool isCritical, float arc)
    {
        start = transform.position;
        target = targetPos + Vector3.up * 0.5f;
        arcHeight = arc;
        launched = true;
        damage = dmg;
        crit = isCritical;

        travelDuration = Mathf.Clamp(1.5f, 0.8f, 2.5f);

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
            StartCoroutine(Explode());
        }
    }

    private IEnumerator Explode()
    {
        launched = false;

        meshRenderer.enabled = false;
        sphereCollider.enabled = false;

        shellExplosionVFX.transform.parent = null;
        shellExplosionVFX.Play();

        Collider[] hits = Physics.OverlapSphere(transform.position, splashRadius);
        HashSet<Enemy> damaged = new();

        foreach (Collider c in hits)
        {
            if (c.TryGetComponent<Enemy>(out var e) && damaged.Add(e))
            {
                e.TakeDamage(damage, crit, withEffect: EnemyStatusEffect.Slow);
            }
        }

        float vfxLife = shellExplosionVFX.main.duration + shellExplosionVFX.main.startLifetime.constantMax;
        Destroy(shellExplosionVFX.gameObject, vfxLife);
        Destroy(gameObject);
        yield break;
    }
}
