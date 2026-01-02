using System;
using System.Collections;
using UnityEngine;

public class HammerStrikeController : MonoBehaviour
{
    [SerializeField] private GameObject hammerPrefab;
    [SerializeField] private float strikeDuration = 0.35f;
    [SerializeField] private ParticleSystem impactVFXPrefab;

    private Camera mainCamera;
    private bool impactTriggered;

    public event Action<Vector3> Impact;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Strike(Vector3 target)
    {
        impactTriggered = false;

        float radius = GetHammerRadius(hammerPrefab);

        Vector3 camBack = Vector3.ProjectOnPlane(-mainCamera.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;

        Vector3 backPos = target - camBack * radius;
        Vector3 leftPos = target - camRight * radius;
        Vector3 rightPos = target + camRight * radius;

        StartCoroutine(
            CoroutineGroup(
                StrikeHammer(backPos, GetFacingRotation(backPos, target), target),
                StrikeHammer(leftPos, GetFacingRotation(leftPos, target), target),
                StrikeHammer(rightPos, GetFacingRotation(rightPos, target), target)
            )
        );
    }

    private IEnumerator StrikeHammer(
        Vector3 pivotPosition,
        Quaternion pivotRotation,
        Vector3 target
    )
    {
        GameObject hammerPivot = Instantiate(hammerPrefab, pivotPosition, pivotRotation);

        Vector3 position = hammerPivot.transform.position;
        position.y = target.y;
        hammerPivot.transform.position = position;

        Transform pivot = hammerPivot.transform;
        Quaternion baseRotation = pivot.localRotation;

        var renderers = hammerPivot.GetComponentsInChildren<Renderer>();

        float startAngle = 0f;
        float hitAngle = -90f;
        float kickbackAngle = hitAngle + 15f;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / strikeDuration;
            float e = Mathf.SmoothStep(0f, 1f, t);

            float angle = Mathf.Lerp(startAngle, hitAngle, e);
            pivot.localRotation = baseRotation * Quaternion.Euler(0f, 0f, angle);

            if (!impactTriggered && e >= 0.94f)
            {
                impactTriggered = true;
                OnImpact(target);
            }

            yield return null;
        }

        pivot.localRotation = baseRotation * Quaternion.Euler(0f, 0f, hitAngle);

        t = 0f;
        float kickbackDuration = strikeDuration * 0.35f;

        while (t < 1f)
        {
            t += Time.deltaTime / kickbackDuration;
            float e = Mathf.SmoothStep(0f, 1f, t);

            float angle = Mathf.Lerp(hitAngle, kickbackAngle, e);
            pivot.localRotation = baseRotation * Quaternion.Euler(0f, 0f, angle);

            float alpha = Mathf.Lerp(1f, 0f, e);
            SetAlpha(renderers, alpha);

            yield return null;
        }

        Destroy(hammerPivot);
    }

    private float GetHammerRadius(GameObject hammerPrefab)
    {
        Transform impactPoint = hammerPrefab.transform.GetChild(0);
        return impactPoint.localPosition.y * hammerPrefab.transform.localScale.y;
    }

    private IEnumerator CoroutineGroup(params IEnumerator[] routines)
    {
        int running = routines.Length;

        foreach (var routine in routines)
            StartCoroutine(Run(routine));

        while (running > 0)
            yield return null;

        IEnumerator Run(IEnumerator routine)
        {
            yield return StartCoroutine(routine);
            running--;
        }
    }

    private void SetAlpha(Renderer[] renderers, float alpha)
    {
        foreach (var r in renderers)
        {
            foreach (var mat in r.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }
        }
    }

    private Quaternion GetFacingRotation(Vector3 pivotPosition, Vector3 target)
    {
        Vector3 dir = target - pivotPosition;
        dir.y = 0f;

        return Quaternion.LookRotation(dir) * Quaternion.Euler(0f, -90f, 0f);
    }

    private void OnImpact(Vector3 target)
    {
        CinemachineShake.Instance.Shake(
            ShakeIntensity.Medium, 
            ShakeLength.Medium
        );
        Instantiate(
            impactVFXPrefab, 
            target + Vector3.up * 5f, 
            Quaternion.identity
        );

        Impact?.Invoke(target);
    }
}
