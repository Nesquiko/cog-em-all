using UnityEngine;

public class NexusHealthBar : MonoBehaviour
{
    [SerializeField] private Transform fill;
    [SerializeField] private Camera cam;

    private Nexus nexus;
    private Vector3 initialScale;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (fill != null) initialScale = fill.localScale;
        if (nexus == null) nexus = GetComponentInParent<Nexus>();

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (nexus == null) return;

        if (cam != null)
        {
            transform.LookAt(transform.position + cam.transform.forward);
        }

        float hpFraction = Mathf.Clamp01(nexus.HealthPointsNormalized);
        fill.localScale = new Vector3(
            initialScale.x,
            initialScale.y * hpFraction,
            initialScale.z
        );
    }
}
