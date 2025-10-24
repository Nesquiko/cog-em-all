using System;
using UnityEngine;

public class Nexus : MonoBehaviour
{
    [SerializeField] private float maxHealthPoints = 1_000_000f;
    [SerializeField] private GameObject healthBarGO;

    private float healthPoints;
    public float HealthPointsNormalized => healthPoints / maxHealthPoints;

    public event Action<Nexus> OnDestroyed;

    private void Awake()
    {
        healthPoints = maxHealthPoints;
    }

    public void TakeDamage(float damage)
    {
        healthPoints -= damage;
        if (healthBarGO != null)
        {
            healthBarGO.SetActive(true);
        }

        if (healthPoints <= 0f)
        {
            healthPoints = 0;
            OnDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
    }

    public bool IsFullHealth => Mathf.Approximately(healthPoints, maxHealthPoints);
}
