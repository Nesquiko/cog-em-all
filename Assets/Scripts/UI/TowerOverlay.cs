using UnityEngine;

public class TowerOverlay : MonoBehaviour
{
    [SerializeField] private GameObject upgradeButton;
    [SerializeField] private GameObject takeControlButton;
    [SerializeField] private GameObject rotateButton;

    private CanvasGroup upgradeCanvasGroup;
    private ScaleOnHover upgradeScaleOnHover;
    private CursorPointer upgradeCursorPointer;
    private TooltipOnButton upgradeTooltipOnButton;

    private CanvasGroup takeControlCanvasGroup;
    private ScaleOnHover takeControlScaleOnHover;
    private CursorPointer takeControlCursorPointer;
    private TooltipOnButton takeControlTooltipOnButton;

    private CanvasGroup rotateCanvasGroup;
    private ScaleOnHover rotateScaleOnHover;
    private CursorPointer rotateCursorPointer;
    private TooltipOnButton rotateTooltipOnButton;

    private Camera mainCamera;
    private RectTransform rectTransform;
    private GameObject towerGO;
    private ITower tower;

    private TowerControlManager towerControlManager;
    private TowerSellManager towerSellManager;
    private TowerUpgradeManager towerUpgradeManager;

    public void Initialize(GameObject t)
    {
        towerGO = t;
        tower = towerGO.GetComponent<ITower>();
    }

    public void Start()
    {
        AdjustOverlayButtons();
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
        towerControlManager = FindFirstObjectByType<TowerControlManager>();
        towerSellManager = FindFirstObjectByType<TowerSellManager>();
        towerUpgradeManager = FindFirstObjectByType<TowerUpgradeManager>();

        upgradeCanvasGroup = upgradeButton.GetComponent<CanvasGroup>();
        upgradeScaleOnHover = upgradeButton.GetComponent<ScaleOnHover>();
        upgradeCursorPointer = upgradeButton.GetComponent<CursorPointer>();
        upgradeTooltipOnButton = upgradeButton.GetComponent<TooltipOnButton>();

        takeControlCanvasGroup = takeControlButton.GetComponent<CanvasGroup>();
        takeControlScaleOnHover = takeControlButton.GetComponent<ScaleOnHover>();
        takeControlCursorPointer = takeControlButton.GetComponent<CursorPointer>();
        takeControlTooltipOnButton = takeControlButton.GetComponent<TooltipOnButton>();

        rotateCanvasGroup = rotateButton.GetComponent<CanvasGroup>();
        rotateScaleOnHover = rotateButton.GetComponent<ScaleOnHover>();
        rotateCursorPointer = rotateButton.GetComponent<CursorPointer>();
        rotateTooltipOnButton = rotateButton.GetComponent<TooltipOnButton>();
    }

    private void AdjustOverlayButtons()
    {
        if (!towerGO.TryGetComponent<ITowerUpgradeable>(out var towerUpgradeable) || !towerUpgradeable.CanUpgrade())
        {
            upgradeCanvasGroup.alpha = 0.5f;
            upgradeScaleOnHover.enabled = false;
            upgradeCursorPointer.enabled = false;
            upgradeTooltipOnButton.enabled = false;
        }

        if (!towerGO.TryGetComponent<ITowerControllable>(out _))
        {
            takeControlCanvasGroup.alpha = 0.5f;
            takeControlScaleOnHover.enabled = false;
            takeControlCursorPointer.enabled = false;
            takeControlTooltipOnButton.enabled = false;
        }

        if (!towerGO.TryGetComponent<ITowerRotateable>(out _))
        {
            rotateCanvasGroup.alpha = 0.5f;
            rotateScaleOnHover.enabled = false;
            rotateCursorPointer.enabled = false;
            rotateTooltipOnButton.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (tower == null) return;
        Vector3 targetPosition = towerGO.transform.position;
        targetPosition.y += 7f;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);
    
        if (screenPosition.z < 0)
        {
            return;
        }

        rectTransform.position = screenPosition;
    }

    public void OnTakeControlClicked()
    {
        if (!towerGO.TryGetComponent<ITowerControllable>(out var tower)) return;
        towerControlManager.TakeControl(tower);
    }

    public void OnUpgradeTowerClicked()
    {
        if (!towerGO.TryGetComponent<ITowerUpgradeable>(out var tower)) return;
        towerUpgradeManager.RequestUpgrade(tower);

        AdjustOverlayButtons();
    }

    public void OnSellTowerClicked()
    {
        if (!towerGO.TryGetComponent<ITowerSellable>(out var tower)) return;
        towerSellManager.RequestSell(tower);
    }

    public void OnRotateTowerClicked()
    {
        if (!towerGO.TryGetComponent<ITowerRotateable>(out var tower)) return;
        tower.ShowTowerRotationOverlay();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
