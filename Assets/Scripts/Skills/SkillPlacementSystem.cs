using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] private GameObject[] factionSpecificSkillPrefabs;
    [SerializeField] private SkillButton[] skillButtons;
    [SerializeField] private GameObject airshipDropPoint;

    [Header("Visuals")]
    [SerializeField] private Material ghostValidMaterial;
    [SerializeField] private Material ghostInvalidMaterial;

    private GameObject skillPrefab;
    private GameObject ghostInstance;
    private Camera mainCamera;
    private bool isPlacing;
    private bool canPlace;

    private readonly Dictionary<int, GameObject> hotkeyToPrefab = new();
    private readonly Dictionary<int, SkillButton> hotkeyToButton = new();

    private int currentHotkeyIndex = -1;
    private SkillActivationMode currentMode;

    public event Action<ISkill> OnUseSkill;

    public bool IsPlacing => isPlacing;

    private Faction currentFaction;
    private HashSet<FactionSpecificSkill> activeFactionSpecificSkills;

    private void Awake()
    {
        mainCamera = Camera.main;

        currentFaction = Faction.OverpressureCollective;  // TODO: luky -> tu mi musi prist aktualna fakcia
        activeFactionSpecificSkills = new()  // TODO: luky -> tu mi musia prist zo skill tree skilly, ktore mam povolit
        {
            FactionSpecificSkill.AirshipAirstrike,
            FactionSpecificSkill.AirshipFreezeZone,
            FactionSpecificSkill.AirshipDisableZone,
            FactionSpecificSkill.MarkEnemy,
            FactionSpecificSkill.SuddenDeath,
        };

        SetupFactionSpecificSkills();
    }

    private void SetupFactionSpecificSkills()
    {
        hotkeyToPrefab[5] = skillPrefabs[0];
        hotkeyToPrefab[6] = skillPrefabs[1];
        hotkeyToPrefab[7] = skillPrefabs[2];
        for (int i = 0; i < 3; i++)
            hotkeyToButton[i + 5] = skillButtons[i];

        switch (currentFaction)
        {
            case Faction.TheBrassArmy:
                if (activeFactionSpecificSkills.Contains(FactionSpecificSkill.AirshipAirstrike))
                    AssignAirshipSkill(factionSpecificSkillPrefabs[0], AirshipSkillType.Airstrike);
                break;

            case Faction.TheValveboundSeraphs:
                if (activeFactionSpecificSkills.Contains(FactionSpecificSkill.AirshipFreezeZone))
                    AssignAirshipSkill(factionSpecificSkillPrefabs[1], AirshipSkillType.FreezeZone);
                if (activeFactionSpecificSkills.Contains(FactionSpecificSkill.MarkEnemy))
                    AssignSecondarySkill(factionSpecificSkillPrefabs[3]);
                break;

            case Faction.OverpressureCollective:
                if (activeFactionSpecificSkills.Contains(FactionSpecificSkill.AirshipDisableZone))
                    AssignAirshipSkill(factionSpecificSkillPrefabs[2], AirshipSkillType.DisableZone);
                if (activeFactionSpecificSkills.Contains(FactionSpecificSkill.SuddenDeath))
                    AssignSecondarySkill(factionSpecificSkillPrefabs[4]);
                break;
        }
    }

    private void AssignAirshipSkill(GameObject prefab, AirshipSkillType type)
    {
        if (prefab == null) return;
        hotkeyToPrefab[8] = prefab;
        var button = skillButtons[3];
        hotkeyToButton[8] = button;
    }

    private void AssignSecondarySkill(GameObject prefab)
    {
        if (prefab == null) return;
        hotkeyToPrefab[9] = prefab;
        var button = skillButtons[4];
        hotkeyToButton[9] = button;
    }

    private void Update()
    {
        if (pauseManager.Paused) return;

        int hotkeyPressed = GetPressedSkillHotkey();

        if (!isPlacing)
        {
            if (hotkeyPressed != -1 && hotkeyToPrefab.TryGetValue(hotkeyPressed, out var prefab))
            {
                BeginPlacement(prefab, hotkeyPressed);
            }
            if (hotkeyPressed != -1 && !hotkeyToPrefab.ContainsKey(hotkeyPressed))
            {
                Debug.LogWarning($"No prefab assigned to hotkey {hotkeyPressed}.");
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
            else if (hotkeyToPrefab.TryGetValue(hotkeyPressed, out var prefab))
            {
                BeginPlacement(prefab, hotkeyPressed);
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

        if (hotkeyIndex != -1 && hotkeyToPrefab.TryGetValue(hotkeyIndex, out var realPrefab))
            skillPrefab = realPrefab;
        else
            skillPrefab = prefab;

        currentHotkeyIndex = hotkeyIndex;

        if (!skillPrefab.TryGetComponent<ISkill>(out var skill)) return;

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
                if (!staticSkill.TryGetComponent<ISkillPlaceable>(out var placeable)) break;
                placeable.Initialize();
                var circle = Instantiate(buildProgressPrefab, position, Quaternion.identity);
                var progress = circle.GetComponent<BuildProgress>();
                progress.Initialize(staticSkill, disableObjectBehaviors: false);
                break;

            case SkillActivationMode.Airship:
                if (!skillPrefab.TryGetComponent<AirshipSkill>(out var airshipSkill)) break;
                Vector3 startPosition = airshipDropPoint.transform.position;
                airshipSkill.Initialize(
                    startPos: startPosition,
                    targetPos: position
                );
                break;
        }

        OnUseSkill?.Invoke(skill);
        CancelPlacement();
        StartCoroutine(EnableSelectionNextFrame());
    }

    private void CreateGhost(GameObject prefab)
    {
        ghostInstance = Instantiate(prefab);
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

        if (!skillPrefab.TryGetComponent<MarkEnemy>(out var markEnemy)) return;
        markEnemy.BeginAim();

        /*
        var skill = skillPrefab.GetComponent<MarkEnemy>();
        skill.TryActivate();
        OnUseSkill?.Invoke(skill);*/
        Debug.Log("Deploying a raycast skill");
    }

    private void TriggerInstantSkill()
    {
        /*
        // TODO: sudden death
        var skill = skillPrefab.GetComponent<ISkill>();
        OnUseSkill?.Invoke(skill);*/
        Debug.Log("Deploying an instant skill");
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
        if (Keyboard.current.digit5Key.wasPressedThisFrame && CanUseHotkey(5)) return 5;
        if (Keyboard.current.digit6Key.wasPressedThisFrame && CanUseHotkey(6)) return 6;
        if (Keyboard.current.digit7Key.wasPressedThisFrame && CanUseHotkey(7)) return 7;
        if (Keyboard.current.digit8Key.wasPressedThisFrame && CanUseHotkey(8)) return 8;
        if (Keyboard.current.digit9Key.wasPressedThisFrame && CanUseHotkey(9)) return 9;
        return -1;
    }

    private bool CanUseHotkey(int index)
    {
        return hotkeyToButton.TryGetValue(index, out var button) && button.CanPlaceSkill;
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
