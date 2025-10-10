using UnityEngine;
using Unity.Cinemachine;

public class CameraInputSystem : MonoBehaviour
{

    private const float MOVE_SPEED = 75f;
    private const float ROTATE_SPEED = 75f;
    private const float MOUSE_DRAG_MOVE_COEF = .5f;
    private const float ZOOM_COEF = 4f;
    private const float ZOOM_SPEED = 35f;
    private const float ZOOM_MIN = 5f;
    private const float ZOOM_MAX = 200f;

    private const float MAP_MIN_X = 100f;
    private const float MAP_MAX_X = 400f;
    private const float MAP_MIN_Z = 100f;
    private const float MAP_MAX_Z = 400f;

    private CameraInputActions cameraInputActions;

    [SerializeField] private CinemachineCamera cinemachineCamera;

    private void Awake()
    {
        cameraInputActions = new CameraInputActions();
        cameraInputActions.Camera.Enable();
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
            inputMoveVec3.x = -(mouseDelta.x * MOUSE_DRAG_MOVE_COEF);
            inputMoveVec3.z = -(mouseDelta.y * MOUSE_DRAG_MOVE_COEF);
        }


        var move = transform.forward * inputMoveVec3.z + transform.right * inputMoveVec3.x;
        var newPos = transform.position + move * MOVE_SPEED * Time.deltaTime;
        newPos.x = Mathf.Clamp(newPos.x, MAP_MIN_X, MAP_MAX_X);
        newPos.z = Mathf.Clamp(newPos.z, MAP_MIN_Z, MAP_MAX_Z);
        transform.position = newPos;

        // Keyboar rotation
        var rotate = cameraInputActions.Camera.Rotate.ReadValue<float>();
        transform.eulerAngles += new Vector3(0, rotate * ROTATE_SPEED * Time.deltaTime, 0);

        // Zoom
        var zoom = cameraInputActions.Camera.Zoom.ReadValue<float>();
        if (zoom != 0)
        {
            var startZoom = cinemachineCamera.Lens.OrthographicSize;
            var endZoom = startZoom + (zoom * ZOOM_COEF);
            var lerped = Mathf.Lerp(startZoom, endZoom, Time.deltaTime * ZOOM_SPEED);
            cinemachineCamera.Lens.OrthographicSize = Mathf.Clamp(lerped, ZOOM_MIN, ZOOM_MAX);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        float y = transform.position.y;

        Vector3 p1 = new Vector3(MAP_MIN_X, y, MAP_MIN_Z);
        Vector3 p2 = new Vector3(MAP_MAX_X, y, MAP_MIN_Z);
        Vector3 p3 = new Vector3(MAP_MAX_X, y, MAP_MAX_Z);
        Vector3 p4 = new Vector3(MAP_MIN_X, y, MAP_MAX_Z);

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
