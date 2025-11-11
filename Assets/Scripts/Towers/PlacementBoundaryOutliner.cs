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

        for (float x = minX + spacing; x < maxX; x += spacing)
            PlaceBarrier(new Vector3(x, barrierOffsetY, maxZ), Quaternion.identity);

        for (float x = minX + spacing; x < maxX; x += spacing)
            PlaceBarrier(new Vector3(x, barrierOffsetY, minZ), Quaternion.Euler(0f, 180f, 0f));

        for (float z = minZ + spacing; z < maxZ; z += spacing)
            PlaceBarrier(new Vector3(minX, barrierOffsetY, z), Quaternion.Euler(0f, -90f, 0f));

        for (float z = minZ + spacing; z < maxZ; z += spacing)
            PlaceBarrier(new Vector3(maxX, barrierOffsetY, z), Quaternion.Euler(0f, 90f, 0f));

        PlaceCorner(new(minX, barrierOffsetY, minZ), Quaternion.identity);
        PlaceCorner(new(minX, barrierOffsetY, maxZ), Quaternion.Euler(0f, 90f, 0f));
        PlaceCorner(new(maxX, barrierOffsetY, maxZ), Quaternion.Euler(0f, 180f, 0f));
        PlaceCorner(new(maxX, barrierOffsetY, minZ), Quaternion.Euler(0f, -90f, 0f));
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
