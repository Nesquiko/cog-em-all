using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour, IDamageSource
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private SphereCollider sphereCollider;

    [Header("VFX")]
    [SerializeField] private ParticleSystem shellExplosionVFX;
    [SerializeField] private float shellVFXScaleFactor = 3f;

    private MortarTower owner;
    private Vector3 start;
    private Vector3 target;
    private float damage;
    private bool crit;
    private float arcHeight;
    private float travelDuration;
    private float elapsed;
    private bool launched;

    public DamageSourceType Type() => DamageSourceType.Shell;

    public event Action<float> OnDamageDealt;
    public event Action OnEnemyKilled;

    public void Initialize(MortarTower ownerTower)
    {
        owner = ownerTower;
        shellExplosionVFX.transform.localScale = new(
            owner.ShellSplashRadius / shellVFXScaleFactor,
            owner.ShellSplashRadius / shellVFXScaleFactor,
            owner.ShellSplashRadius / shellVFXScaleFactor
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

        Destroy(gameObject, owner.ShellLifetime);
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

        CinemachineShake.Instance.Shake(ShakeIntensity.Low, ShakeLength.Medium);

        Collider[] hits = Physics.OverlapSphere(transform.position, owner.ShellSplashRadius);
        HashSet<IEnemy> damaged = new();

        foreach (Collider c in hits)
        {
            if (c.TryGetComponent<IEnemy>(out var e) && damaged.Add(e))
            {
                EnemyStatusEffect effect = null;
                if (owner.SlowOnHitActive) effect = EnemyStatusEffect.Slow;
                else if (owner.BleedOnHitActive) effect = EnemyStatusEffect.Bleed(owner.BleedDuration);

                OnDamageDealt?.Invoke(damage);
                if (e.HealthPoints < damage) OnEnemyKilled?.Invoke();
                e.TakeDamage(damage, Type(), crit, effect: effect);
            }
        }

        float vfxLife = shellExplosionVFX.main.duration + shellExplosionVFX.main.startLifetime.constantMax;
        Destroy(shellExplosionVFX.gameObject, vfxLife);
        Destroy(gameObject);

        SoundManagersDontDestroy.GerOrCreate()?.SoundFX.PlaySoundFXClip(SoundFXType.Explosion, transform);

        yield break;
    }
}
