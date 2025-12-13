using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private IEnemy enemy;
    [SerializeField] private Transform fill;
    [SerializeField] private Camera cam;

    private Vector3 initialScale;

    public bool ActiveSelf => gameObject != null && gameObject.activeSelf;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (fill != null) initialScale = fill.localScale;

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (enemy == null) return;

        if (cam != null)
        {
            transform.LookAt(transform.position + cam.transform.forward);
        }

        float hpFraction = Mathf.Clamp01(enemy.HealthPointsNormalized);
        fill.localScale = new Vector3(
            initialScale.x,
            initialScale.y * hpFraction,
            initialScale.z
        );
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
}
