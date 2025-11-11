using System;
using System.Collections;
using UnityEngine;

public class Wall : MonoBehaviour, ISkillPlaceable, IDamageable
{
    [SerializeField] private SkillTypes skillType = SkillTypes.Wall;
    [SerializeField] private Quaternion placementRotationOffset = Quaternion.Euler(0f, 0f, 0f);
    public SkillTypes SkillType() => skillType;
    public Quaternion PlacementRotationOffset() => placementRotationOffset;
    public Transform Transform() => transform;

    [SerializeField] private float maxHealthPoints = 500f;
    [SerializeField] private GameObject wallHealthBar;
    [SerializeField] private GameObject wallModel;
    [SerializeField] private GameObject minimapIndicator;

    [SerializeField] private Vector3 wallHealthBarScale;
    [SerializeField] private Vector3 minimapIndicatorScale;

    [Header("VFX")]
    [SerializeField] private ParticleSystem wallExplosion;

    private bool isDying;

    private float healthPoints;
    public float HealthPointsNormalized() => healthPoints / maxHealthPoints;

    public bool IsDestroyed() => isDying || healthPoints <= 0f;

    public event Action<Wall> OnDestroyed;
    public event Action<Wall> OnHealthChanged;

    private void Awake()
    {
        healthPoints = maxHealthPoints;
        OnHealthChanged?.Invoke(this);
    }

    public void Initialize()
    {
        wallHealthBar.transform.localScale = wallHealthBarScale;
        minimapIndicator.transform.localScale = minimapIndicatorScale;
    }

    public void TakeDamage(float damage)
    {
        if (isDying) return;

        healthPoints -= damage;
        if (wallHealthBar != null)
        {
            wallHealthBar.SetActive(true);
        }

        OnHealthChanged?.Invoke(this);

        if (healthPoints <= 0f)
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        if (isDying) yield break;
        isDying = true;

        healthPoints = 0;
        OnDestroyed?.Invoke(this);

        // wallExplosion.Play(withChildren: true);  // walls can explode after destruction (skill tree item?)

        yield return new WaitForSeconds(0.1f);

        Destroy(wallModel);
        Destroy(wallHealthBar);

        yield return new WaitForSeconds(2.1f);

        Destroy(gameObject);
    }

    public bool IsFullHealth => Mathf.Approximately(healthPoints, maxHealthPoints);
}
