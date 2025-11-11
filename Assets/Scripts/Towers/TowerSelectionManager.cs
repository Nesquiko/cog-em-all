using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TowerSelectionManager : MonoBehaviour
{
    [SerializeField] private TowerControlManager towerControlManager;
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;

    [SerializeField] private LayerMask towerMask;
    [SerializeField] private TowerInfo towerInfoPanel;
    
    private Camera mainCamera;
    private ITowerSelectable currentSelected;
    private ITowerSelectable currentHovered;

    private bool disabled;
    public bool Disabled => disabled;

    public void DisableSelection()
    {
        ClearHover();
        DeselectCurrent();
        disabled = true;
    }

    public void EnableSelection() => disabled = false;

    public ITowerSelectable CurrentSelected() => currentSelected;

    private void Awake()
    {
        mainCamera = Camera.main;
        disabled = false;
    }

    private void Update()
    {
        if (disabled) return;

        if (towerControlManager.InControl) return;
        if (towerPlacementSystem.IsPlacing) return;

        if (Mouse.current == null) return;

        HandleHover();
        HandleClick();

        UpdateTowerInfoPanel();
    }

    private void HandleHover()
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, towerMask, QueryTriggerInteraction.Ignore))
        {
            ITowerSelectable hovered = hit.collider.GetComponentInParent<ITowerSelectable>();
            
            if (hovered != currentHovered)
            {
                currentHovered?.OnHoverExit();
                currentHovered = hovered;
                currentHovered?.OnHoverEnter();
            }
        }
        else
        {
            if (currentHovered != null)
            {
                currentHovered.OnHoverExit();
                currentHovered = null;
            }
        }
    }

    private void HandleClick()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            DeselectCurrent();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentHovered != null)
            {
                if (currentHovered == currentSelected)
                {
                    DeselectCurrent();
                }
                else
                {
                    SelectTower(currentHovered);
                }
            }
            else
            {
                DeselectCurrent();
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            DeselectCurrent();
        }
    }

    private void SelectTower(ITowerSelectable newTower)
    {
        if (currentSelected == newTower) return;

        currentSelected?.Deselect();

        currentSelected = newTower;
        currentSelected.Select();
    }

    public void DeselectCurrent()
    {
        if (currentSelected == null) return;

        currentSelected.Deselect();
        currentSelected = null;

        UpdateTowerInfoPanel();
    }

    public void ClearHover()
    {
        if (currentHovered != null && currentHovered != currentSelected)
        {
            currentHovered.OnHoverExit();
            currentHovered = null;
        }

        UpdateTowerInfoPanel();
    }

    private void UpdateTowerInfoPanel()
    {
        ITowerSelectable displayTower = currentHovered ?? currentSelected;

        if (displayTower != null)
        {
            if (!towerInfoPanel.gameObject.activeSelf)
            {
                towerInfoPanel.Show(displayTower.TowerType());
            }
            else
            {
                towerInfoPanel.UpdateTowerInfo(displayTower.TowerType());
            }
        }
        else
        {
            if (towerInfoPanel.gameObject.activeSelf)
            {
                towerInfoPanel.Hide();
            }
        }
    }
}