using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerControlManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerControlUI;
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject towerInfoPanel;
    [SerializeField] private TowerSelectionManager towerSelectionManager;
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private float transitionTime = 2.0f;

    private Camera mainCamera;
    private CinemachineBrain brain;
    private ITowerControllable currentTower;
    private Transform controlPoint;

    private CanvasGroup playerControlCanvasGroup;

    private Vector3 previousCameraPosition;
    private Quaternion previousCameraRotation;

    private bool isReturning;
    private bool inControl;

    public bool InControl => inControl;

    private void Awake()
    {
        mainCamera = Camera.main;
        brain = mainCamera.GetComponent<CinemachineBrain>();
        playerControlUI.SetActive(false);
        isReturning = false;
        playerControlCanvasGroup = playerControlUI.GetComponent<CanvasGroup>();
        playerControlCanvasGroup.alpha = 0f;
    }

    public void TakeControl(ITowerControllable tower)
    {
        towerSelectionManager.DeselectCurrent();
        towerSelectionManager.DisableSelection();

        if (inControl) return;

        currentTower = tower;
        controlPoint = tower.GetControlPoint();

        previousCameraPosition = mainCamera.transform.position;
        previousCameraRotation = mainCamera.transform.rotation;

        HUD.SetActive(false);
        menuPanel.SetActive(false);
        towerInfoPanel.SetActive(false);
        playerControlUI.SetActive(false);

        tower.OnPlayerTakeControl(true);
        controlPoint = tower.GetControlPoint();

        StartCoroutine(MoveCameraToControlPoint());
    }

    public void ReleaseControl()
    {
        if (!inControl || isReturning) return;
        isReturning = true;

        currentTower.OnPlayerTakeControl(false);
        inControl = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        HUD.SetActive(true);
        menuPanel.SetActive(true);
        towerSelectionManager.DeselectCurrent();

        mainCamera.transform.SetParent(null);
        StartCoroutine(ReturnCamera());
    }

    private void Update()
    {
        if (pauseManager.Paused) return;

        if (!inControl) return;

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            ReleaseControl();
            return;
        }

        Vector2 delta = Mouse.current.delta.ReadValue();
        currentTower?.HandlePlayerAim(delta);

        if (Mouse.current.leftButton.isPressed)
            currentTower?.HandlePlayerFire();
    }

    private IEnumerator MoveCameraToControlPoint()
    {
        mainCamera.transform.SetParent(null, true);
        playerControlUI.SetActive(true);

        brain.enabled = false;

        mainCamera.transform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
        controlPoint.GetPositionAndRotation(out Vector3 targetPosition, out Quaternion targetRotation);

        float duration = transitionTime;
        float t = 0f;
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = curve.Evaluate(t);

            mainCamera.transform.SetPositionAndRotation(Vector3.Lerp(startPosition, targetPosition, eased), Quaternion.Slerp(startRotation, targetRotation, eased));

            playerControlCanvasGroup.alpha = eased;

            yield return null;
        }

        mainCamera.transform.SetPositionAndRotation(targetPosition, targetRotation);
        mainCamera.transform.SetParent(controlPoint, worldPositionStays: true);

        yield return null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerControlCanvasGroup.blocksRaycasts = true;
        inControl = true;
    }

    private IEnumerator ReturnCamera()
    {
        float duration = transitionTime;
        float t = 0f;

        mainCamera.transform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
        Vector3 endPosition = previousCameraPosition;
        Quaternion endRotation = previousCameraRotation;

        brain.enabled = false;

        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = curve.Evaluate(t);

            mainCamera.transform.SetPositionAndRotation(Vector3.Lerp(startPosition, endPosition, eased), Quaternion.Slerp(startRotation, endRotation, eased));

            playerControlCanvasGroup.alpha = 1 - eased;

            yield return null;
        }

        towerSelectionManager.EnableSelection();

        mainCamera.transform.SetPositionAndRotation(endPosition, endRotation);
        playerControlUI.SetActive(false);
        brain.enabled = true;
        currentTower = null;
        isReturning = false;
    }
}
