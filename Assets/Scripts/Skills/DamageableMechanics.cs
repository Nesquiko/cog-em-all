using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, IEnemy attacker);
    float HealthPointsNormalized();
    bool IsDestroyed();
    Transform Transform();
}
