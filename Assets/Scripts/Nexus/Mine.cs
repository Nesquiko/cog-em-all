using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour, ISkillPlaceable
{
    [SerializeField] private SkillTypes skillType = SkillTypes.Mine;
    [SerializeField] private Quaternion placementRotationOffset = Quaternion.Euler(0f, 0f, 0f);
    public SkillTypes SkillType() => skillType;
    public float GetCooldown() => 5f;
    public Quaternion PlacementRotationOffset() => placementRotationOffset;

    [Header("Settings")]
    [SerializeField] private float armDelay = 1.0f;
    [SerializeField] private float triggerDelay = 1.0f;
    [SerializeField] private float explosionRadius = 15.0f;
    [SerializeField] private float explosionDamage = 200.0f;
    [SerializeField] private LayerMask enemyMask;

    [SerializeField] private GameObject mineModel;
    [SerializeField] private MeshRenderer[] mineScrews;
    [SerializeField] private Material screwArmedMaterial;
    [SerializeField] private GameObject minimapIndicator;
    [SerializeField] private Vector3 minimapIndicatorScale;

    [Header("VFX")]
    [SerializeField] private ParticleSystem mineExplosion;

    private bool armed;
    private bool triggered;

    public void Initialize()
    {
        minimapIndicator.transform.localScale = minimapIndicatorScale;

        StartCoroutine(ArmAfterDelay());
    }

    private IEnumerator ArmAfterDelay()
    {
        yield return new WaitForSeconds(armDelay);
        armed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!armed || triggered) return;
        if (!other.TryGetComponent<IEnemy>(out var enemy)) return;

        triggered = true;
        StartCoroutine(TriggerExplosion());
    }

    private IEnumerator TriggerExplosion()
    {
        int screwCount = mineScrews.Length;
        if (screwCount == 0)
        {
            yield return new WaitForSeconds(triggerDelay);
            StartCoroutine(Explode());
            yield break;
        }

        float interval = triggerDelay / screwCount;

        for (int i = 0; i < screwCount; i++)
        {
            var screw = mineScrews[i];
            if (screw != null)
                screw.material = screwArmedMaterial;

            yield return new WaitForSeconds(interval);
        }

        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        mineExplosion.Play();

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, enemyMask, QueryTriggerInteraction.Collide);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IEnemy>(out var e))
                e.TakeDamage(explosionDamage);
        }

        yield return new WaitForSecondsRealtime(0.1f);

        Destroy(mineModel);

        yield return new WaitForSecondsRealtime(2.1f);

        Destroy(gameObject);
    }
}
