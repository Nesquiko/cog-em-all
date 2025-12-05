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
    [SerializeField] private SkillButton[] skillButtons;

    [Header("Visuals")]
    [SerializeField] private Material ghostValidMaterial;
    [SerializeField] private Material ghostInvalidMaterial;

    private GameObject skillPrefab;
    private GameObject ghostInstance;
    private Camera mainCamera;
    private bool isPlacing;
    private bool canPlace;

    private int currentHotkeyIndex = -1;
    private SkillActivationMode currentMode;

    public event Action<ISkill> OnUseSkill;

    public bool IsPlacing => isPlacing;

    private void Awake() => mainCamera = Camera.main;

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
                BeginPlacement(skillPrefabs[hotkeyPressed], hotkeyPressed);
                return;
            }
        }

        if (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }

        HandlePlacementUpdate();
    }

    private void HandlePlacementUpdate()
    {
        if (currentMode == SkillActivationMode.Raycast)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                TryActivateRaycastSkill();
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
        currentHotkeyIndex = hotkeyIndex;

        var skill = skillPrefab.GetComponent<ISkill>();
        if (skill == null) return;

        currentMode = skill.ActivationMode();

        HUDPanelUI.ShowPlacementInfo(skill.SkillType());

        switch (currentMode)
        {
            case SkillActivationMode.Placement:
            case SkillActivationMode.Airship:
                CreateGhost(skillPrefab);
                isPlacing = true;
                break;

            case SkillActivationMode.Raycast:
                isPlacing = true;
                break;

            case SkillActivationMode.Instant:
                TriggerInstantSkill();
                break;
        }
    }

    private void PlaceSkill(Vector3 position, Quaternion rotation)
    {
        if (!isPlacing || skillPrefab == null) return;

        if (!skillPrefab.TryGetComponent<ISkill>(out var skill)) return;

        switch (skill.ActivationMode())
        {
            case SkillActivationMode.Placement:
                var staticSkill = Instantiate(skillPrefab, position, rotation);
                if (staticSkill.TryGetComponent<ISkillPlaceable>(out var placeable))
                    placeable.Initialize();
                var circle = Instantiate(buildProgressPrefab, position, Quaternion.identity);
                var progress = circle.GetComponent<BuildProgress>();
                progress.Initialize(staticSkill, disableObjectBehaviors: false);
                break;

            case SkillActivationMode.Airship:
                /*var airSkill = skillPrefab.GetComponent<AirshipBase>();
                var airSkillGO = Instantiate(skillPrefab, position, rotation);
                airSkillGO.GetComponent<AirshipBase>().Trigger(position);*/
                Debug.Log("Deploying an airship skill");
                break;
        }

        OnUseSkill?.Invoke(skill);
        CancelPlacement();
        StartCoroutine(EnableSelectionNextFrame());
    }

    private void CreateGhost(GameObject prefab)
    {
        ghostInstance = Instantiate(skillPrefab);
        int ghostLayer = LayerMask.NameToLayer("PlacementGhost");
        ghostInstance.layer = ghostLayer;
        foreach (Transform t in ghostInstance.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = ghostLayer;
        SetGhostMode(ghostInstance, true);
        ApplyGhostMaterial(ghostValidMaterial);
    }

    private void TryActivateRaycastSkill()
    {
        CancelPlacement();
        /*
        var skill = skillPrefab.GetComponent<MarkEnemy>();
        skill.TryActivate();
        OnUseSkill?.Invoke(skill);*/
        Debug.Log("Deploying a raycast skill");
    }

    private void TriggerInstantSkill()
    {
        Debug.Log("Deploying an instant skill");
        /*
        // TODO: sudden death
        var skill = skillPrefab.GetComponent<ISkill>();
        OnUseSkill?.Invoke(skill);*/
    }

    public void CancelPlacement()
    {
        isPlacing = false;
        currentHotkeyIndex = -1;

        if (ghostInstance != null) Destroy(ghostInstance);
        ghostInstance = null;
        skillPrefab = null;
        HUDPanelUI.HidePlacementInfo();
    }

    private IEnumerator EnableSelectionNextFrame()
    {
        yield return null;
        towerSelectionManager.EnableSelection();
    }

    private int GetPressedSkillHotkey()
    {
        if (Keyboard.current.digit5Key.wasPressedThisFrame && skillButtons[0].CanPlaceSkill) return 0;
        if (Keyboard.current.digit6Key.wasPressedThisFrame && skillButtons[1].CanPlaceSkill) return 1;
        if (Keyboard.current.digit7Key.wasPressedThisFrame && skillButtons[2].CanPlaceSkill) return 2;
        if (Keyboard.current.digit8Key.wasPressedThisFrame && skillButtons[3].CanPlaceSkill) return 3;
        if (Keyboard.current.digit9Key.wasPressedThisFrame && skillButtons[4].CanPlaceSkill) return 4;
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
