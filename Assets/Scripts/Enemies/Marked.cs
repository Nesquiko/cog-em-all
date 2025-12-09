using UnityEngine;

[ExecuteAlways]
public class Marked : MonoBehaviour
{
    private Camera mainCamera;

    private void LateUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 toCamera = mainCamera.transform.position - transform.position;
        toCamera.y = 0f;
        if (toCamera.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
    }
}
