using UnityEngine;

public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        if (Camera.main)
            transform.forward = Camera.main.transform.forward;
    }
}
