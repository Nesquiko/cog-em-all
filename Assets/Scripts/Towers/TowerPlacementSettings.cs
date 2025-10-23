using UnityEngine;

[CreateAssetMenu(
    fileName = "TowerPlacementSettings",
    menuName = "Scriptable Objects/Tower Placement Settings"
)]
public class TowerPlacementSettings : ScriptableObject
{
    [Header("Placeable Region Bounds")]
    [SerializeField] private Bounds placeableRegion = new(
        new(100f, 0, 100f),
        new(400f, 0, 400f)
    );

    [Header("Placement Settings")]
    [Tooltip("Radius to check for blocking overlaps.")]
    [SerializeField] private float placementRadius = 1.0f;

    [Tooltip("Layers that block tower placement.")]
    [SerializeField] private LayerMask[] blockingMasks;

    public bool IsValidPlacement(Vector3 point)
    {
        bool overlaps = false;

        foreach (var blockingMask in blockingMasks)
        {
            overlaps = Physics.CheckSphere(
                point,
                placementRadius,
                blockingMask,
                QueryTriggerInteraction.Ignore
            );
        }

        bool outsideRegion = !placeableRegion.Contains(point);

        return !overlaps && !outsideRegion;
    }
}
