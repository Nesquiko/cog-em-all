using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerSelectionManager : MonoBehaviour
{
    public static TowerSelectionManager Instance {  get; private set; }

    [Header("Settings")]
    [SerializeField] private LayerMask towerMask = ~0;
    [SerializeField] private float rayDistance = 1000f;

    private Camera mainCamera;
    private TowerSelectable currentSelection;
    private TowerSelectable currentHover;

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
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = mainCamera.ScreenPointToRay(mouse.position.ReadValue());
        TowerSelectable hitTower = null;

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, towerMask))
        {
            hitTower = hit.collider.GetComponentInParent<TowerSelectable>();
        }

        if (hitTower != currentHover)
        {
            if (currentHover && !currentHover.IsSelected)
                currentHover.OnHoverExit();
            if (hitTower && !hitTower.IsSelected)
                hitTower.OnHoverEnter();
            currentHover = hitTower;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (hitTower)
                SelectTower(hitTower);
            else
                DeselectCurrent();
        }
    }

    private void SelectTower(TowerSelectable ts)
    {
        if (currentSelection == ts) return;

        DeselectCurrent();

        currentSelection = ts;
        currentSelection.Select();
    }

    private void DeselectCurrent()
    {
        if (currentSelection)
        {
            currentSelection.Deselect();
            currentSelection = null;
        }
    }
}