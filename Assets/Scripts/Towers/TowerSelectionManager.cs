using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TowerSelectionManager : MonoBehaviour
{
    public static TowerSelectionManager Instance {  get; private set; }

    [SerializeField] private LayerMask towerMask;
    [SerializeField] private TowerInfo towerInfoPanel;
    
    private Camera mainCamera;
    private ITowerSelectable currentSelected;
    private ITowerSelectable currentHovered;

    private bool disabled;
    public bool Disabled => disabled;

    public void DisableSelection() => disabled = true;

    public void EnableSelection() => disabled = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        mainCamera = Camera.main;
        disabled = false;
    }

    private void Update()
    {
        if (disabled) return;

        if (TowerControlManager.Instance.InControl) return;
        if (TowerPlacementSystem.Instance.IsPlacing) return;

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
    }

    public void ClearHover()
    {
        if (currentHovered != null && currentHovered != currentSelected)
        {
            currentHovered.OnHoverExit();
            currentHovered = null;
        }
    }

    private void UpdateTowerInfoPanel()
    {
        ITowerSelectable displayTower = currentHovered ?? currentSelected;

        if (displayTower != null)
        {
            if (!towerInfoPanel.gameObject.activeSelf)
            {
                towerInfoPanel.Show(displayTower);
            }
            else
            {
                towerInfoPanel.UpdateTowerInfo(displayTower);
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