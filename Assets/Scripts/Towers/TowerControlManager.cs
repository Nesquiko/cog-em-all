using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerControlManager : MonoBehaviour
{
    public static TowerControlManager Instance { get; private set; }

    private Camera mainCamera;
    private CinemachineBrain brain;
    private ITowerControllable currentTower;
    private Transform controlPoint;
    private Vector3 previousCameraPosition;
    private Quaternion previousCameraRotation;
    private bool inControl;

    public bool InControl => inControl;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        mainCamera = Camera.main;
        brain = mainCamera.GetComponent<CinemachineBrain>();
    }

    public void TakeControl(ITowerControllable tower)
    {
        if (inControl) return;

        inControl = true;
        currentTower = tower;
        controlPoint = tower.GetControlPoint();

        previousCameraPosition = mainCamera.transform.position;
        previousCameraRotation = mainCamera.transform.rotation;

        brain.enabled = false;

        mainCamera.transform.SetParent(controlPoint, worldPositionStays: false);
        mainCamera.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        
        tower.OnPlayerTakeControl(true);
    }

    public void ReleaseControl()
    {
        if (!inControl) return;

        currentTower.OnPlayerTakeControl(false);

        mainCamera.transform.SetParent(null);
        mainCamera.transform.SetPositionAndRotation(
            previousCameraPosition,
            previousCameraRotation
        );

        brain.enabled = true;

        inControl = false;
        currentTower = null;
    }

    private void Update()
    {
        if (!inControl) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ReleaseControl();
            return;
        }

        Vector2 delta = Mouse.current.delta.ReadValue();
        currentTower?.HandlePlayerAim(delta);

        if (Mouse.current.leftButton.isPressed)
            currentTower?.HandlePlayerFire();
    }
}
