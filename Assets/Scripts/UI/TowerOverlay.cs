using UnityEngine;
using UnityEngine.UI;

public class TowerOverlay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button takeControlButton;
    [SerializeField] private Button upgradeTowerButton;
    [SerializeField] private Button sellTowerButton;
    [SerializeField] private Button rotateTowerButton;

    private Camera mainCamera;
    private RectTransform rectTransform;
    private Transform target;

    private TowerControlManager towerControlManager;
    private TowerSellManager towerSellManager;

    public void SetTarget(Transform t)
    {
        target = t;
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
        towerControlManager = FindFirstObjectByType<TowerControlManager>();
        towerSellManager = FindFirstObjectByType<TowerSellManager>();

        takeControlButton.onClick.AddListener(OnTakeControlClicked);
        //upgradeTowerButton.onClick.AddListener(OnUpgradeTowerClicked);
        sellTowerButton.onClick.AddListener(OnSellTowerClicked);
        rotateTowerButton.onClick.AddListener(OnRotateTowerClicked);
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }
        Vector3 targetPosition = target.position;
        targetPosition.y += 7f;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);
    
        if (screenPosition.z < 0)
        {
            return;
        }

        rectTransform.position = screenPosition;
    }

    private void OnTakeControlClicked()
    {
        if (target.TryGetComponent<ITowerControllable>(out var tower))
            towerControlManager.TakeControl(tower);
    }

    /*private void OnUpgradeTowerClicked()
    {

    }*/

    private void OnSellTowerClicked()
    {
        if (target.TryGetComponent<ITowerSellable>(out var tower))
        {
            tower.SellAndDestroy();
        }
    }

    private void OnRotateTowerClicked()
    {
        if (target.TryGetComponent<FlamethrowerTower>(out var tower))
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
