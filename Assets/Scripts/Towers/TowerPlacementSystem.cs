using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerPlacementSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask blockingMask;
    [SerializeField] private LayerMask roadMask;
    [SerializeField] private LayerMask ghostLayer;
    [SerializeField] private GameObject buildProgressPrefab;
    [SerializeField] private HUDPanelUI HUDPanelUI;
    [SerializeField] private TowerSelectionManager towerSelectionManager;
    [SerializeField] private SkillPlacementSystem skillPlacementSystem;
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private GameObject[] towerPrefabs;
    [SerializeField] private GameObject[] towerGhostPrefabs;
    [SerializeField] private TowerButton[] towerButtons;

    [Header("Visuals")]
    [SerializeField] private Material ghostValidMaterial;
    [SerializeField] private Material ghostInvalidMaterial;

    [Header("Place Animation")]
    [SerializeField] private HammerStrikeController hammerStrikeController;
    [SerializeField] private float towerRiseHeight = 10f;
    [SerializeField] private float towerRiseDuration = 0.25f;

    [SerializeField] private TowerPlacementSettings placementSettings;

    private GameObject towerPrefab;
    private GameObject ghostPrefab;
    private GameObject ghostInstance;
    private Camera mainCamera;
    private bool isPlacing;
    private bool canPlace;

    private int currentHotkeyIndex = -1;

    public event Action<ITower> OnPlace;

    public bool IsPlacing => isPlacing;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (pauseManager.Paused) return;

        int hotkeyPressed = GetPressedTowerHotkey();

        if (!isPlacing)
        {
            if (hotkeyPressed != -1)
            {
                BeginPlacement(towerPrefabs[hotkeyPressed], hotkeyPressed);
            }
            return;
        }

        if (hotkeyPressed != -1)
        {
            if (hotkeyPressed == currentHotkeyIndex)
            {
                CancelPlacement();
                return;
            }
            else
            {
                BeginPlacement(towerPrefabs[hotkeyPressed], hotkeyPressed);
                return;
            }
        }

        if (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask & ~(1 << ghostLayer), QueryTriggerInteraction.Ignore))
        {
            Vector3 point = hit.point;

            if (ghostInstance != null)
            {
                ghostInstance.transform.position = point + Vector3.up * 0.01f;
                canPlace = placementSettings.IsValidPlacement(point);
                ApplyGhostMaterial(canPlace ? ghostValidMaterial : ghostInvalidMaterial);
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                ApplyGhostMaterial(ghostInvalidMaterial);
                return;
            }

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

    public void BeginPlacement(GameObject prefab, int hotkeyIndex = -1)
    {
        ghostPrefab = towerGhostPrefabs[hotkeyIndex];

        skillPlacementSystem.CancelPlacement();
        CancelPlacement(enableSelectionAfter: false);

        towerSelectionManager.DisableSelection();

        towerPrefab = prefab;
        isPlacing = true;
        currentHotkeyIndex = hotkeyIndex;

        ghostInstance = Instantiate(ghostPrefab);
        TowerTypes towerType = prefab.GetComponent<ITower>().TowerType();
        HUDPanelUI.ShowPlacementInfo(towerType);

        ApplyGhostMaterial(ghostValidMaterial);
    }

    private void PlaceTower(Vector3 position)
    {
        if (!isPlacing) return;

        StartCoroutine(PlaceTowerAfterImpact(position));

        CancelPlacement(resetTowerPrefab: false);
    }

    private IEnumerator PlaceTowerAfterImpact(Vector3 position)
    {
        bool spawned = false;
        GameObject towerGO = null;

        void OnImpact(Vector3 impactPosition)
        {
            if (spawned) return;
            spawned = true;

            Vector3 spawnPos = position - Vector3.up * towerRiseHeight;
            towerGO = Instantiate(towerPrefab, spawnPos, Quaternion.identity);

            towerGO.SetActive(false);
        }

        hammerStrikeController.Impact += OnImpact;
        hammerStrikeController.Strike(position);

        while (!spawned)
            yield return null;

        hammerStrikeController.Impact -= OnImpact;

        towerGO.SetActive(true);

        yield return StartCoroutine(RiseTower(towerGO, position));

        ITower tower = towerGO.GetComponent<ITower>();
        OnPlace?.Invoke(tower);

        towerPrefab = null;
    }

    private IEnumerator RiseTower(GameObject tower, Vector3 targetPos)
    {
        Vector3 startPos = tower.transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / towerRiseDuration;
            float e = Mathf.SmoothStep(0f, 1f, t);

            tower.transform.position = Vector3.Lerp(startPos, targetPos, e);

            yield return null;
        }

        tower.transform.position = targetPos;
    }

    public void CancelPlacement(bool enableSelectionAfter = true, bool resetTowerPrefab = true)
    {
        isPlacing = false;
        if (resetTowerPrefab) towerPrefab = null;
        currentHotkeyIndex = -1;

        if (ghostInstance != null) Destroy(ghostInstance);
        ghostInstance = null;

        HUDPanelUI.HidePlacementInfo();

        if (enableSelectionAfter)
            StartCoroutine(ReenableSelectionNextFrame());
    }

    private IEnumerator ReenableSelectionNextFrame()
    {
        yield return null;
        towerSelectionManager.EnableSelection();
    }

    private int GetPressedTowerHotkey()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame && towerButtons[0].IsEnabled) return 0;
        if (Keyboard.current.digit2Key.wasPressedThisFrame && towerButtons[1].IsEnabled) return 1;
        if (Keyboard.current.digit3Key.wasPressedThisFrame && towerButtons[2].IsEnabled) return 2;
        if (Keyboard.current.digit4Key.wasPressedThisFrame && towerButtons[3].IsEnabled) return 3;
        return -1;
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