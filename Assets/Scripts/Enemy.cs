using UnityEngine;
using UnityEngine.Splines;

public class Enemy : MonoBehaviour
{
    [SerializeField] private SplineContainer path;
    [SerializeField] private float speed = 100f;
    [SerializeField] private float healthPoints = 100f;

    private float t = 0f;

    public void SetSpline(SplineContainer pathContainer, float startT = 0f)
    {
        path = pathContainer;
        t = Mathf.Clamp01(startT);

        if (path != null)
            transform.position = path.EvaluatePosition(0, t);
    }

    public void TakeDamage(float damage)
    {
        healthPoints -= damage;
        if (healthPoints <= 0f)
        {
            Destroy(gameObject);
            // TODO: you killed a guy
        }
    }

    void Update()
    {
        if (path == null) return;

        float length = path.CalculateLength();
        if (length <= 0.001f) return;

        t += (speed / length) * Time.deltaTime;
        if (t > 1f) t -= 1f;

        Vector3 position = path.EvaluatePosition(0, t);
        Vector3 tangent = path.EvaluateTangent(0, t);

        transform.position = position;
        if (tangent != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(tangent);
        }
    }
}
