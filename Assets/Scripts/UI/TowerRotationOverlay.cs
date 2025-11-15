using UnityEngine;

public class TowerRotationOverlay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HoldButton rotateLeftButton;
    [SerializeField] private HoldButton rotateRightButton;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 90f;

    private Camera mainCamera;
    private RectTransform rectTransform;
    private GameObject towerGO;
    private bool active;

    public void Initialize(GameObject t)
    {
        towerGO = t;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;

        rotateLeftButton.OnHold += RotateLeft;
        rotateRightButton.OnHold += RotateRight;
    }

    private void LateUpdate()
    {
        if (!active || towerGO == null)
        {
            return;
        }
        Vector3 targetPosition = towerGO.transform.position;
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
        if (!towerGO) return;
        towerGO.transform.Rotate(Vector3.up, direction * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void RotateLeft() => Rotate(-1);

    private void RotateRight() => Rotate(1);

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
