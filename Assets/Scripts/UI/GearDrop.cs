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
    private Vector3 startPosition;

    public bool Done
    {
        get;
        private set;
    }

    public void Activate(Vector3 worldPosition)
    {
        transform.position = worldPosition;
        transform.localScale = Vector3.one;
        visual.localRotation = Random.rotation;
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
            velocity += Physics.gravity * t * 0.5f;
            transform.position += velocity * t;
            visual.Rotate(Vector3.up, rotateSpeed * t, Space.Self);

            if (elapsed >= idleTime)
            {
                flying = true;
                startPosition = transform.position;
            }
            return;
        }

        transform.position = Vector3.Lerp(transform.position, targetWorld, t * flySpeed);
        transform.Rotate(Vector3.up, rotateSpeed * t, Space.Self);

        if ((transform.position - targetWorld).sqrMagnitude < 0.05f)
            Done = true;
    }
}
