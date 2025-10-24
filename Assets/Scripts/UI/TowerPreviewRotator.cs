using UnityEngine;

public class TowerPreviewRotator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rotationTarget;

    [Header("Rotation Settings")]
    [SerializeField] private float idleSpeed = 15f;
    [SerializeField] private bool reverse = false;

    private void Awake()
    {
        if (!rotationTarget)
            rotationTarget = transform;
    }

    private void Update()
    {
        float direction = reverse ? -1f : 1f;
        rotationTarget.Rotate(Vector3.up, direction * idleSpeed * Time.deltaTime, Space.World);
    }
}