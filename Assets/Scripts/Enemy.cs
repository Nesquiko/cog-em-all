using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Enemy : MonoBehaviour
{
    [SerializeField] private SplineContainer path;
    [SerializeField] private float speed = 100f;
    [SerializeField] private float maxHealthPoints = 100f;
    private float healthPoints;
    [SerializeField] private GameObject healthBarGO;

    public float HealthPointsNormalized => healthPoints / maxHealthPoints;

    private float t = 0f;

    public void SetSpline(SplineContainer pathContainer, float startT = 0f)
    {
        path = pathContainer;
        t = Mathf.Clamp01(startT);

        Assert.IsNotNull(path);
        transform.position = path.EvaluatePosition(0, t);
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
            Destroy(gameObject);
            // TODO: you killed a guy
        }
    }

    public bool IsFullHealth => Mathf.Approximately(healthPoints, maxHealthPoints);

    void Awake()
    {
        healthPoints = maxHealthPoints;
    }

    private void Update()
    {
        Assert.IsNotNull(path);

        float length = path.CalculateLength();
        if (length <= 0.001f) return;

        t += speed / length * Time.deltaTime;
        if (t > 1f) t -= 1f;

        Vector3 position = path.EvaluatePosition(0, t);
        Vector3 tangent = path.EvaluateTangent(0, t);

        transform.position = new Vector3(position.x, position.y, position.z);
        if (tangent != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(tangent);
        }
    }
}
