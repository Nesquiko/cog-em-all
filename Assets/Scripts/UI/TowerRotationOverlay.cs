using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TowerRotationOverlay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HoldButton rotateLeftButton;
    [SerializeField] private HoldButton rotateRightButton;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 90f;

    private Camera mainCamera;
    private RectTransform rectTransform;
    private Transform target;
    private bool active;

    private TowerSelectionManager towerSelectionManager;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
        towerSelectionManager = FindFirstObjectByType<TowerSelectionManager>();

        rotateLeftButton.OnHold += RotateLeft;
        rotateRightButton.OnHold += RotateRight;
    }

    private void Start()
    {
        Hide();
    }

    private void LateUpdate()
    {
        if (!active || target == null)
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

        //if (Keyboard.current.rKey.isPressed) Rotate(-1);
        //if (Keyboard.current.tKey.isPressed) Rotate(1);
    }

    private void Rotate(int direction)
    {
        if (!target) return;
        target.Rotate(Vector3.up, direction * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void RotateLeft() => Rotate(-1);

    private void RotateRight() => Rotate(1);

    private void OnCancelRotation()
    {
        active = false;

        if (target.TryGetComponent<FlamethrowerTower>(out var tower))
            tower.EndManualRotation();

        towerSelectionManager.EnableSelection();
        gameObject.SetActive(false);
    }

    public void Show()
    {
        active = true;
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        active = false;
        gameObject.SetActive(false);
    }
}
