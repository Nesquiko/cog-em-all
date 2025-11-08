using UnityEngine;

public class BuildProgress : MonoBehaviour
{
    [SerializeField] private float buildTime = 1f;

    private GameObject buildObject;
    private float timer;
    private Vector3 startScale;
    private bool done;

    private bool initialized = false;

    private bool disableBehaviors;

    public void Initialize(GameObject targetObject, bool disableObjectBehaviors)
    {
        buildObject = targetObject;
        disableBehaviors = disableObjectBehaviors;
        initialized = true;
    }

    private void Start()
    {
        startScale = transform.localScale;
        transform.localScale = Vector3.zero;
        if (disableBehaviors)
            DisableObjectBehaviours();
    }

    private void Update()
    {
        if (done) return;

        if (initialized && buildObject == null)
        {
            Destroy(gameObject);
            return;
        }

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / buildTime);

        transform.localScale = Vector3.Lerp(Vector3.zero, startScale, progress);

        if (progress >= 1f)
        {
            if (disableBehaviors)
                EnableObjectBehaviours();
            done = true;
            Destroy(gameObject);
        }
    }

    private void DisableObjectBehaviours()
    {
        foreach (var component in buildObject.GetComponentsInChildren<MonoBehaviour>())
        {
            component.enabled = false;
        }
    }

    private void EnableObjectBehaviours()
    {
        foreach (var component in buildObject.GetComponentsInChildren<MonoBehaviour>())
        {
            component.enabled = true;
        }
    }
}
