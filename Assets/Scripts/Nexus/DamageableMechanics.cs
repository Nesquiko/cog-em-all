using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount);
    float HealthPointsNormalized {  get; }
    bool IsDestroyed {  get; }
    Transform transform { get; }
}
