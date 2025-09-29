using UnityEngine;
using Unity.Cinemachine;

public class CameraInputSystem : MonoBehaviour
{

    private const float MOVE_SPEED = 75f;
    private const float ROTATE_SPEED = 75f;
    private const float MOUSE_EDGE_MOVE_BUFFER = 20;
    private const float MOUSE_DRAG_MOVE_COEF = .5f;
    private const float ZOOM_COEF = 4f;
    private const float ZOOM_SPEED = 35f;
    private const float ZOOM_MIN = 5f;
    private const float ZOOM_MAX = 200f;

    private const float MAP_MIN_X = 500f;
    private const float MAP_MAX_X = 2500f;
    private const float MAP_MIN_Z = 500f;
    private const float MAP_MAX_Z = 2500f;

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

        // Mouse edge moving
        // var mousePosition = cameraInputActions.Camera.MousePosition.ReadValue<Vector2>();
        // if (mousePosition.x < MOUSE_EDGE_MOVE_BUFFER)
        // {
        //     inputMoveVec3.x = -1;
        // }
        // else if (mousePosition.x > Screen.width - MOUSE_EDGE_MOVE_BUFFER)
        // {
        //     inputMoveVec3.x = 1;
        // }

        // if (mousePosition.y < MOUSE_EDGE_MOVE_BUFFER)
        // {
        //     inputMoveVec3.z = -1;
        // }
        // else if (mousePosition.y > Screen.height - MOUSE_EDGE_MOVE_BUFFER)
        // {
        //     inputMoveVec3.z = 1;
        // }

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
}
