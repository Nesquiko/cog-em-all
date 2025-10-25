using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerPlacementSystem : MonoBehaviour
{
    public static TowerPlacementSystem Instance { get; private set; }

    [Header("References")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask blockingMask;
    [SerializeField] private LayerMask roadMask;
    [SerializeField] private GameObject buildProgressPrefab;

    [Header("Visuals")]
    [SerializeField] private Material ghostValidMaterial;
    [SerializeField] private Material ghostInvalidMaterial;

    [SerializeField] private TowerPlacementSettings placementSettings;

    private GameObject towerPrefab;
    private GameObject ghostInstance;
    private Camera mainCamera;
    private bool isPlacing;
    private bool canPlace;

    public bool IsPlacing => isPlacing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!isPlacing) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        int ghostLayer = LayerMask.NameToLayer("PlacementGhost");
        int effectiveMask = groundMask & ~(1 << ghostLayer);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, effectiveMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 point = hit.point;

            if (ghostInstance != null)
            {
                ghostInstance.transform.position = point + Vector3.up * 0.01f;
                canPlace = placementSettings.IsValidPlacement(point);
                ApplyGhostMaterial(canPlace ? ghostValidMaterial : ghostInvalidMaterial);
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (Mouse.current.leftButton.wasPressedThisFrame && canPlace)
            {
                PlaceTower(point);
            }
        }
        else
        {
            ApplyGhostMaterial(ghostInvalidMaterial);
            canPlace = false;
        }
    }

    public void BeginPlacement(GameObject prefab)
    {
        TowerSelectionManager.Instance.ClearHover();

        CancelPlacement();

        towerPrefab = prefab;
        isPlacing = true;
        ghostInstance = Instantiate(prefab);
        int ghostLayer = LayerMask.NameToLayer("PlacementGhost");
        ghostInstance.layer = ghostLayer;
        foreach (Transform t in ghostInstance.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = ghostLayer;
        SetGhostMode(ghostInstance, true);
        ApplyGhostMaterial(ghostValidMaterial);
    }

    private void PlaceTower(Vector3 position)
    {
        if (towerPrefab == null || !isPlacing) return;

        GameObject towerGO = Instantiate(towerPrefab, position, Quaternion.identity);

        if (buildProgressPrefab != null)
        {
            var circle = Instantiate(buildProgressPrefab, position, Quaternion.identity);
            var progress = circle.GetComponent<BuildProgress>();
            progress.Initialize(towerGO);
        }

        CancelPlacement();
    }

    private void CancelPlacement()
    {
        isPlacing = false;
        towerPrefab = null;
        if (ghostInstance != null) Destroy(ghostInstance);
        ghostInstance = null;
    }

    public void TryPlaceAtMouse()
    {
        if (!isPlacing || towerPrefab == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask) && placementSettings.IsValidPlacement(hit.point))
        {
            PlaceTower(hit.point);
        }
        else
        {
            CancelPlacement();
        }
    }

    private void SetGhostMode(GameObject obj, bool enable)
    {
        foreach (var c in obj.GetComponentsInChildren<MonoBehaviour>())
            c.enabled = !enable;

        foreach (var col in obj.GetComponentsInChildren<Collider>())
            col.enabled = !enable;

        foreach (var r in obj.GetComponentsInChildren<Renderer>())
            r.sharedMaterial = ghostValidMaterial;
    }

    private void ApplyGhostMaterial(Material material)
    {
        if (ghostInstance == null || material == null) return;
        foreach (var renderer in ghostInstance.GetComponentsInChildren<Renderer>())
        {
            renderer.sharedMaterial = material;
        }
    }
}