using System.Collections;
using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    [SerializeField] private float recoilDistance = 0.15f;
    [SerializeField] private float returnSpeed = 15f;
    [SerializeField] private float kickUp = 1.5f;

    private Vector3 defaultLocalPosition;
    private Quaternion defaultLocalRotation;
    private Coroutine recoilRoutine;

    private void Awake()
    {
        defaultLocalPosition = transform.localPosition;
        defaultLocalRotation = transform.localRotation;
    }

    public void PlayRecoil()
    {
        if (recoilRoutine != null) StopCoroutine(recoilRoutine);
        recoilRoutine = StartCoroutine(Recoil());
    }

    private IEnumerator Recoil()
    {
        Vector3 backPosition = defaultLocalPosition - Vector3.forward * recoilDistance;
        Quaternion upRotation = defaultLocalRotation * Quaternion.Euler(-kickUp, 0f, 0f);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            transform.SetLocalPositionAndRotation(Vector3.Lerp(defaultLocalPosition, backPosition, t), Quaternion.Slerp(defaultLocalRotation, upRotation, t));
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            transform.SetLocalPositionAndRotation(Vector3.Lerp(backPosition, defaultLocalPosition, t), Quaternion.Slerp(upRotation, defaultLocalRotation, t));
            yield return null;
        }

        transform.SetLocalPositionAndRotation(defaultLocalPosition, defaultLocalRotation);
    }
}
