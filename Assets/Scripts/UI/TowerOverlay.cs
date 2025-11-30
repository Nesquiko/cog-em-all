using UnityEngine;

public class TowerOverlay : MonoBehaviour
{
    [SerializeField] private GameObject takeControlButton;
    [SerializeField] private GameObject stimModeButton;
    [SerializeField] private GameObject upgradeButton;
    [SerializeField] private GameObject sellButton;
    [SerializeField] private GameObject rotateButton;

    [SerializeField] private TowerDataCatalog towerDataCatalog;

    private CanvasGroup upgradeCanvasGroup;
    private ScaleOnHover upgradeScaleOnHover;
    private CursorPointer upgradeCursorPointer;
    private TooltipOnButton upgradeTooltipOnButton;

    private CanvasGroup takeControlCanvasGroup;
    private ScaleOnHover takeControlScaleOnHover;
    private CursorPointer takeControlCursorPointer;
    private TooltipOnButton takeControlTooltipOnButton;

    private CanvasGroup stimModeCanvasGroup;
    private ScaleOnHover stimModeScaleOnHover;
    private CursorPointer stimModeCursorPointer;
    private TooltipOnButton stimModeTooltipOnButton;

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

        if (upgradeButton != null)
        {
            upgradeCanvasGroup = upgradeButton.GetComponent<CanvasGroup>();
            upgradeScaleOnHover = upgradeButton.GetComponent<ScaleOnHover>();
            upgradeCursorPointer = upgradeButton.GetComponent<CursorPointer>();
            upgradeTooltipOnButton = upgradeButton.GetComponent<TooltipOnButton>();
        }

        if (stimModeButton != null)
        {
            stimModeCanvasGroup = takeControlButton.GetComponent<CanvasGroup>();
            stimModeScaleOnHover = takeControlButton.GetComponent<ScaleOnHover>();
            stimModeCursorPointer = takeControlButton.GetComponent<CursorPointer>();
            stimModeTooltipOnButton = takeControlButton.GetComponent<TooltipOnButton>();
        }

        if (takeControlButton != null)
        {
            takeControlCanvasGroup = takeControlButton.GetComponent<CanvasGroup>();
            takeControlScaleOnHover = takeControlButton.GetComponent<ScaleOnHover>();
            takeControlCursorPointer = takeControlButton.GetComponent<CursorPointer>();
            takeControlTooltipOnButton = takeControlButton.GetComponent<TooltipOnButton>();
        }

        if (rotateButton != null)
        {
            rotateCanvasGroup = rotateButton.GetComponent<CanvasGroup>();
            rotateScaleOnHover = rotateButton.GetComponent<ScaleOnHover>();
            rotateCursorPointer = rotateButton.GetComponent<CursorPointer>();
            rotateTooltipOnButton = rotateButton.GetComponent<TooltipOnButton>();
        }
    }

    private void AdjustOverlayButtons()
    {
        // Upgradeable
        if (upgradeButton != null && (!towerGO.TryGetComponent<ITower>(out var tower) || !towerDataCatalog.CanUpgrade(tower.TowerType(), tower.CurrentLevel())))
        {
            upgradeCanvasGroup.alpha = 0.5f;
            upgradeScaleOnHover.enabled = false;
            upgradeCursorPointer.enabled = false;
            upgradeTooltipOnButton.enabled = false;
        }

        // Stimulable
        if (stimModeButton != null && !towerGO.TryGetComponent<ITowerStimulable>(out _))
        {
            stimModeCanvasGroup.alpha = 0.5f;
            stimModeScaleOnHover.enabled = false;
            stimModeCursorPointer.enabled = false;
            stimModeTooltipOnButton.enabled = false;
        }

        // Controllable
        if (takeControlButton != null && !towerGO.TryGetComponent<ITowerControllable>(out _))
        {
            takeControlCanvasGroup.alpha = 0.5f;
            takeControlScaleOnHover.enabled = false;
            takeControlCursorPointer.enabled = false;
            takeControlTooltipOnButton.enabled = false;
        }

        // Rotateable
        if (rotateButton != null && !towerGO.TryGetComponent<ITowerRotateable>(out _))
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

    public void OnStimModeClicked()
    {
        if (stimModeButton == null || !towerGO.TryGetComponent<ITowerStimulable>(out var tower)) return;
        //towerStimManager.ActivateStimMode(tower);
    }

    public void OnTakeControlClicked()
    {
        if (takeControlButton == null || !towerGO.TryGetComponent<ITowerControllable>(out var tower)) return;
        towerControlManager.TakeControl(tower);
    }

    public void OnUpgradeTowerClicked()
    {
        if (upgradeButton == null || !towerGO.TryGetComponent<ITower>(out var tower)) return;
        towerDataCatalog.RequestUpgrade(tower);

        AdjustOverlayButtons();
    }

    public void OnSellTowerClicked()
    {
        if (sellButton == null || !towerGO.TryGetComponent<ITowerSellable>(out var tower)) return;
        towerSellManager.RequestSell(tower);

        AdjustOverlayButtons();
    }

    public void OnRotateTowerClicked()
    {
        if (rotateButton == null || !towerGO.TryGetComponent<ITowerRotateable>(out var tower)) return;
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
