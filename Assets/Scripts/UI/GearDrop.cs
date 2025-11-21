using UnityEngine;

public class GearDrop : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform visual;
    [SerializeField] private float rotateSpeed = 250f;
    [SerializeField] private float idleTime = 0.8f;
    [SerializeField] private float flySpeed = 8f;

    private float elapsed;
    private bool flying;
    private Vector3 velocity;
    private float currentYRotation;
    private Quaternion baseRotation;

    public bool Done { get; private set; }

    public void Activate(Vector3 worldPosition)
    {
        transform.position = worldPosition;
        transform.localScale = Vector3.one;
        visual.localRotation = baseRotation;
        currentYRotation = 0f;

        Done = false;
        elapsed = 0f;
        flying = false;

        velocity = Random.insideUnitSphere * 1.5f + Vector3.up * 2.5f;
    }

    public void Tick(float t, Vector3 targetWorld)
    {
        if (Done) return;
        elapsed += t;

        if (!flying)
        {
            velocity += 0.5f * t * Physics.gravity;
            transform.position += velocity * t;

            currentYRotation += rotateSpeed * t;
            visual.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);

            if (elapsed >= idleTime)
            {
                flying = true;
            }
            return;
        }

        transform.position = Vector3.Lerp(transform.position, targetWorld, t * flySpeed);
        visual.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);

        if ((transform.position - targetWorld).sqrMagnitude < 0.05f)
            Done = true;
    }
}
