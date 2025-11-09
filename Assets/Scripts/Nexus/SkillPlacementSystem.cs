using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class SkillPlacementSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SplineContainer road;
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private GameObject buildProgressPrefab;
    [SerializeField] private HUDPanelUI HUDPanelUI;
    [SerializeField] private TowerSelectionManager towerSelectionManager;
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;
    
    [Header("Skills")]
    [SerializeField] private GameObject[] skillPrefabs;

    [Header("Visuals")]
    [SerializeField] private Material ghostValidMaterial;
    [SerializeField] private Material ghostInvalidMaterial;

    private GameObject skillPrefab;
    private GameObject ghostInstance;
    private Camera mainCamera;
    private bool isPlacing;
    private bool canPlace;

    private int currentHotkeyIndex = -1;

    public event Action<ISkill> OnUseSkill;

    public bool IsPlacing => isPlacing;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (pauseManager.Paused) return;

        int hotkeyPressed = GetPressedSkillHotkey();

        if (!isPlacing)
        {
            if (hotkeyPressed != -1)
            {
                BeginPlacement(skillPrefabs[hotkeyPressed], hotkeyPressed);
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
                BeginPlacement(skillPrefabs[hotkeyPressed], hotkeyPressed);  // will be skillPrefabs
                return;
            }
        }

        if (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 mousePoint = Vector3.zero;
        Plane groundPlane = new(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float enter))
            mousePoint = ray.GetPoint(enter);

        (Vector3 position, Vector3 tangent) = GetClosestPointOnRoad(mousePoint);
        Quaternion rotation = Quaternion.LookRotation(tangent, Vector3.up);

        if (skillPrefab.TryGetComponent<ISkillPlaceable>(out var skillInfo))
            rotation *= skillInfo.PlacementRotationOffset();

        if (ghostInstance != null)
        {
            ghostInstance.transform.SetPositionAndRotation(
                position,
                rotation
            );
            canPlace = true;
            ApplyGhostMaterial(ghostValidMaterial);
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            ApplyGhostMaterial(ghostInvalidMaterial);
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && canPlace)
        {
            PlaceSkill(position, rotation);
        }
    }

    private (Vector3 position, Vector3 tangent) GetClosestPointOnRoad(Vector3 samplePoint)
    {
        if (road == null)
            return (samplePoint, Vector3.forward);

        float bestT = 0f;
        float bestDistance = float.MaxValue;

        int samples = 1000;
        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            Vector3 p = road.EvaluatePosition(t);
            float d = (samplePoint - p).sqrMagnitude;
            if (d < bestDistance)
            {
                bestDistance = d;
                bestT = t;
            }
        }

        Vector3 closestPosition = road.EvaluatePosition(bestT);
        Vector3 tangent = ((Vector3)road.EvaluateTangent(bestT)).normalized;
        return (closestPosition, tangent);
    }
    
    public void BeginPlacement(GameObject prefab, int hotkeyIndex = -1)
    {
        towerPlacementSystem.CancelPlacement();
        CancelPlacement();

        towerSelectionManager.DisableSelection();

        skillPrefab = prefab;
        isPlacing = true;
        currentHotkeyIndex = hotkeyIndex;

        ghostInstance = Instantiate(skillPrefab);
        SkillTypes skillType = ghostInstance.GetComponent<ISkill>().SkillType();

        HUDPanelUI.ShowPlacementInfo(skillType);

        int ghostLayer = LayerMask.NameToLayer("PlacementGhost");
        ghostInstance.layer = ghostLayer;
        foreach (Transform t in ghostInstance.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = ghostLayer;
        SetGhostMode(ghostInstance, true);
        ApplyGhostMaterial(ghostValidMaterial);
    }

    private void PlaceSkill(Vector3 position, Quaternion rotation)
    {
        if (skillPrefab == null || !isPlacing) return;

        GameObject skillGO = Instantiate(skillPrefab, position, rotation);
        if (skillGO.TryGetComponent<ISkillPlaceable>(out var skill))
            skill.Initialize();

        OnUseSkill?.Invoke(skill);

        var circle = Instantiate(buildProgressPrefab, position, Quaternion.identity);
        var progress = circle.GetComponent<BuildProgress>();
        progress.Initialize(skillGO, disableObjectBehaviors: false);

        CancelPlacement();

        StartCoroutine(EnableSelectionNextFrame());
    }

    private IEnumerator EnableSelectionNextFrame()
    {
        yield return null;
        towerSelectionManager.EnableSelection();
    }

    public void CancelPlacement()
    {
        isPlacing = false;
        skillPrefab = null;
        currentHotkeyIndex = -1;

        if (ghostInstance != null) Destroy(ghostInstance);
        ghostInstance = null;

        HUDPanelUI.HidePlacementInfo();
    }

    private int GetPressedSkillHotkey()
    {
        if (Keyboard.current.digit5Key.wasPressedThisFrame) return 0;
        if (Keyboard.current.digit6Key.wasPressedThisFrame) return 1;
        if (Keyboard.current.digit7Key.wasPressedThisFrame) return 2;
        return -1;
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
