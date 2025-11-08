using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Transform fill;
    [SerializeField] private Camera cam;

    private IDamageable damageable;
    private Vector3 initialScale;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (fill != null) initialScale = fill.localScale;
        damageable ??= GetComponentInParent<IDamageable>();

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (damageable == null) return;

        if (cam != null)
        {
            transform.LookAt(transform.position + cam.transform.forward);
        }

        float hpFraction = Mathf.Clamp01(damageable.HealthPointsNormalized);
        fill.localScale = new Vector3(
            initialScale.x,
            initialScale.y * hpFraction,
            initialScale.z
        );
    }
}
