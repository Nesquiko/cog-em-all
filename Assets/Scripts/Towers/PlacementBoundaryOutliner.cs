using UnityEngine;

public class PlacementBoundaryOutliner : MonoBehaviour
{
    [SerializeField] private TowerPlacementSettings placementSettings;
    [SerializeField] private GameObject barrierPrefab;
    [SerializeField] private GameObject cornerPrefab;
    [SerializeField] private float spacing = 20f;
    [SerializeField] private float barrierOffsetY = 0f;

    private void Start()
    {
        DrawOutline();
    }

    private void DrawOutline()
    {
        float minX = placementSettings.MinX;
        float maxX = placementSettings.MaxX;
        float minZ = placementSettings.MinZ;
        float maxZ = placementSettings.MaxZ;

        for (float x = minX; x <= maxX; x += spacing)
            PlaceBarrier(new(x, barrierOffsetY, maxZ), Quaternion.identity);
        

        for (float x = minX; x <= maxX; x += spacing)
            PlaceBarrier(new(x, barrierOffsetY, minZ), Quaternion.Euler(0f, 180f, 0f));

        for (float z = minZ; z <= maxZ; z += spacing)
            PlaceBarrier(new(minX, barrierOffsetY, z), Quaternion.Euler(0f, -90f, 0f));

        for (float z = minZ; z <= maxZ; z += spacing)
            PlaceBarrier(new(maxX, barrierOffsetY, z), Quaternion.Euler(0f, 90f, 0f));

        PlaceCorner(new(minX, barrierOffsetY, minZ), Quaternion.Euler(0f, 180f, 0));
        PlaceCorner(new(minX, barrierOffsetY, maxZ), Quaternion.Euler(0f, -90f, 0f));
        PlaceCorner(new(maxX, barrierOffsetY, maxZ), Quaternion.identity);
        PlaceCorner(new(maxX, barrierOffsetY, minZ), Quaternion.Euler(0f, 90f, 0f));
    }

    private void PlaceBarrier(Vector3 position, Quaternion rotation)
    {
        Instantiate(barrierPrefab, position, rotation, transform);
    }

    private void PlaceCorner(Vector3 position, Quaternion rotation)
    {
        Instantiate(cornerPrefab, position, rotation, transform);
    }
}
