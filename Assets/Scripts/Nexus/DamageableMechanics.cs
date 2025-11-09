using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount);
    float HealthPointsNormalized();
    bool IsDestroyed();
    Transform Transform();
}
