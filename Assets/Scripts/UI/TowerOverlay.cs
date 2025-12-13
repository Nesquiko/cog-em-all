using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
            stimModeCanvasGroup = stimModeButton.GetComponent<CanvasGroup>();
            stimModeScaleOnHover = stimModeButton.GetComponent<ScaleOnHover>();
            stimModeCursorPointer = stimModeButton.GetComponent<CursorPointer>();
            stimModeTooltipOnButton = stimModeButton.GetComponent<TooltipOnButton>();
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

    public void Start()
    {
        AdjustOverlayButtons();
    }

    public void AdjustOverlayButtons()
    {
        // Upgradeable
        if (upgradeButton != null)
        {
            bool upgradeEnabled = towerGO.TryGetComponent<ITower>(out var tower) && towerDataCatalog.CanUpgrade(tower.TowerType(), tower.CurrentLevel(), tower.MaxAllowedLevel());
            upgradeButton.GetComponent<Button>().interactable = upgradeEnabled;
            upgradeCanvasGroup.alpha = upgradeEnabled ? 1f : 0.5f;
            upgradeCanvasGroup.interactable = upgradeEnabled;
            upgradeScaleOnHover.enabled = upgradeEnabled;
            upgradeCursorPointer.enabled = upgradeEnabled;
            upgradeTooltipOnButton.enabled = upgradeEnabled;
        }

        // Stimulable
        if (stimModeButton != null)
        {
            bool stimModeEnabled = towerGO.TryGetComponent<ITowerStimulable>(out var stimTower) && stimTower.CanActivateStim();
            stimModeButton.GetComponent<Button>().interactable = stimModeEnabled;
            stimModeCanvasGroup.alpha = stimModeEnabled ? 1f : 0.5f;
            stimModeCanvasGroup.interactable = stimModeEnabled;
            stimModeScaleOnHover.enabled = stimModeEnabled;
            stimModeCursorPointer.enabled = stimModeEnabled;
            stimModeTooltipOnButton.enabled = stimModeEnabled;
        }

        // Controllable
        if (takeControlButton != null)
        {
            bool takeControlEnabled = towerGO.TryGetComponent<ITowerControllable>(out _);
            takeControlButton.GetComponent<Button>().interactable = takeControlEnabled;
            takeControlCanvasGroup.alpha = takeControlEnabled ? 1f : 0.5f;
            takeControlCanvasGroup.interactable = takeControlEnabled;
            takeControlScaleOnHover.enabled = takeControlEnabled;
            takeControlCursorPointer.enabled = takeControlEnabled;
            takeControlTooltipOnButton.enabled = takeControlEnabled;
        }

        // Rotateable
        if (rotateButton != null)
        {
            bool rotateEnabled = towerGO.TryGetComponent<ITowerRotateable>(out _);
            rotateButton.GetComponent<Button>().interactable = rotateEnabled;
            rotateCanvasGroup.alpha = rotateEnabled ? 1f : 0.5f;
            rotateCanvasGroup.interactable = rotateEnabled;
            rotateScaleOnHover.enabled = rotateEnabled;
            rotateCursorPointer.enabled = rotateEnabled;
            rotateTooltipOnButton.enabled = rotateEnabled;
        }
    }

    private void LateUpdate()
    {
        if (tower == null) return;
        Vector3 targetPosition = towerGO.transform.position;
        targetPosition.y += 7f;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);

        if (screenPosition.z < 0) return;

        rectTransform.position = screenPosition;
    }

    public void OnStimModeClicked()
    {
        if (stimModeButton == null || !towerGO.TryGetComponent<ITowerStimulable>(out var tower) || !tower.CanActivateStim()) return;
        tower.ActivateStim();
        StartCoroutine(HandleStimButtonCooldown(tower));
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
        AdjustOverlayButtons();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        StartCoroutine(RefreshAfterFrame());
    }

    private IEnumerator RefreshAfterFrame()
    {
        yield return null;
        AdjustOverlayButtons();
    }

    private IEnumerator HandleStimButtonCooldown(ITowerStimulable stimTower)
    {
        SetStimButtonInteractable(false);

        while (stimTower.StimActive() || stimTower.StimCoolingDown())
            yield return null;

        SetStimButtonInteractable(true);
    }

    private void SetStimButtonInteractable(bool enable)
    {
        if (stimModeButton == null) return;
        stimModeButton.GetComponent<Button>().interactable = enable;
        stimModeCanvasGroup.alpha = enable ? 1f : 0.5f;
        stimModeCanvasGroup.interactable = enable;
        stimModeScaleOnHover.enabled = enable;
        stimModeCursorPointer.enabled = enable;
        stimModeTooltipOnButton.enabled = enable;
    }
}
