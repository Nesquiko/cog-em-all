using UnityEngine;

[CreateAssetMenu(
    fileName = "TowerPlacementSettings",
    menuName = "Scriptable Objects/Tower Placement Settings"
)]
public class TowerPlacementSettings : ScriptableObject
{
    [Header("Placeable Region (XZ limits)")]
    [Tooltip("Minimum X world coordinate allowed for tower placement.")]
    [SerializeField] private float minX = 100f;

    [Tooltip("Maximum X world coordinate allowed for tower placement.")]
    [SerializeField] private float maxX = 400f;

    [Tooltip("Minimum Z world coordinate allowed for tower placement.")]
    [SerializeField] private float minZ = 100f;

    [Tooltip("Maximum Z world coordinate allowed for tower placement.")]
    [SerializeField] private float maxZ = 400f;

    [Header("Placement Settings")]
    [Tooltip("Radius to check for blocking overlaps.")]
    [SerializeField] private float placementRadius = 1.0f;

    [Tooltip("Layers that block tower placement.")]
    [SerializeField] private LayerMask[] blockingMasks;

    public bool IsValidPlacement(Vector3 point)
    {
        if (!IsInPlaceableRegion(point))
            return false;

        foreach (var blockingMask in blockingMasks)
        {
            if (Physics.CheckSphere(
                    point,
                    placementRadius,
                    blockingMask,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsInPlaceableRegion(Vector3 point)
    {
        return point.x >= minX && point.x <= maxX &&
               point.z >= minZ && point.z <= maxZ;
    }
}