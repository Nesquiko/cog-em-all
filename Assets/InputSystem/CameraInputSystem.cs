using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Assertions;

public class CameraInputSystem : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 75f;
    [SerializeField] private float rotateSpeed = 75f;
    [SerializeField] private float mouseDragMoveCoef = .5f;

    [Header("Zoom")]
    [SerializeField, Range(0f, 1f)] private float zoomT = 0f;
    [SerializeField] private float zoomSpeed = 1.5f;
    [SerializeField] private Vector3 offsetFar = new(0f, 65f, -170f);
    [SerializeField] private Vector3 offsetNear = new(0f, 5f, -10f);
    [SerializeField] private float offsetSmoothTime = 0.15f;
    private Vector3 currentOffset;
    private Vector3 offsetVelocity;

    [Header("Map bounds")]
    [SerializeField] private float mapMinX = 100f;
    [SerializeField] private float mapMaxX = 400f;
    [SerializeField] private float mapMinZ = 100f;
    [SerializeField] private float mapMaxZ = 400f;


    private CameraInputActions cameraInputActions;

    [SerializeField] private CinemachineCamera cinemachineCamera;
    private CinemachineFollow cinemachineFollow;

    private void Awake()
    {
        cameraInputActions = new CameraInputActions();
        cameraInputActions.Camera.Enable();
    }

    void Start()
    {
        cinemachineFollow = cinemachineCamera.GetComponent<CinemachineFollow>();
        Assert.IsNotNull(cinemachineFollow);
        currentOffset = EvaluateOffset(zoomT);
        cinemachineFollow.FollowOffset = currentOffset;
    }

    private void FixedUpdate()
    {
        // Keyboard moving
        var input = cameraInputActions.Camera.Movement.ReadValue<Vector2>();
        var inputMoveVec3 = new Vector3(input.x, 0, input.y);

        // Mouse drag moving
        if (cameraInputActions.Camera.MouseShouldDrag.IsPressed())
        {
            var mouseDelta = cameraInputActions.Camera.MouseDragMovement.ReadValue<Vector2>();
            inputMoveVec3.x = -(mouseDelta.x * mouseDragMoveCoef);
            inputMoveVec3.z = -(mouseDelta.y * mouseDragMoveCoef);
        }

        var move = transform.forward * inputMoveVec3.z + transform.right * inputMoveVec3.x;
        var newPos = transform.position + move * moveSpeed * Time.deltaTime;
        newPos.x = Mathf.Clamp(newPos.x, mapMinX, mapMaxX);
        newPos.z = Mathf.Clamp(newPos.z, mapMinZ, mapMaxZ);
        transform.position = newPos;

        // Keyboard rotation
        var rotate = cameraInputActions.Camera.Rotate.ReadValue<float>();
        transform.eulerAngles += new Vector3(0, rotate * rotateSpeed * Time.deltaTime, 0);

        // Zoom
        var zoomInput = cameraInputActions.Camera.Zoom.ReadValue<float>();
        if (Mathf.Abs(zoomInput) > Mathf.Epsilon)
        {
            zoomT = Mathf.Clamp01(zoomT - zoomInput * zoomSpeed * Time.deltaTime);
        }

        var targetOffset = EvaluateOffset(zoomT);
        currentOffset = Vector3.SmoothDamp(currentOffset, targetOffset, ref offsetVelocity, offsetSmoothTime);
        cinemachineFollow.FollowOffset = currentOffset;
    }

    private Vector3 EvaluateOffset(float t)
    {
        return Vector3.LerpUnclamped(offsetFar, offsetNear, t);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        float y = transform.position.y;

        Vector3 p1 = new Vector3(mapMinX, y, mapMinZ);
        Vector3 p2 = new Vector3(mapMaxX, y, mapMinZ);
        Vector3 p3 = new Vector3(mapMaxX, y, mapMaxZ);
        Vector3 p4 = new Vector3(mapMinX, y, mapMaxZ);

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);

        float cubeSize = 2f;
        Gizmos.DrawWireCube(p1, new Vector3(cubeSize, 0.1f, cubeSize));
        Gizmos.DrawWireCube(p2, new Vector3(cubeSize, 0.1f, cubeSize));
        Gizmos.DrawWireCube(p3, new Vector3(cubeSize, 0.1f, cubeSize));
        Gizmos.DrawWireCube(p4, new Vector3(cubeSize, 0.1f, cubeSize));
    }
}