using UnityEngine;

public class TowerOverlay : MonoBehaviour
{
    private Camera mainCamera;
    private RectTransform rectTransform;
    private Transform target;

    public void SetTarget(Transform t)
    {
        target = t;
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }
        Vector3 targetPosition = target.position;
        targetPosition.y += 7f;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);
    
        if (screenPosition.z < 0)
        {
            gameObject.SetActive(false);
            return;
        }

        rectTransform.position = screenPosition;
    }

    public void OnTakeControlClicked()
    {
        if (target.TryGetComponent<ITowerControllable>(out var tower))
            TowerControlManager.Instance.TakeControl(tower);
    }
}
