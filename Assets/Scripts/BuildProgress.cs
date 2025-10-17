using UnityEngine;

public class BuildProgress : MonoBehaviour
{
    [SerializeField] private float buildTime = 1f;

    private GameObject towerObject;
    private float timer;
    private Vector3 startScale;
    private bool done;

    public void Initialize(GameObject targetTower)
    {
        towerObject = targetTower;
    }

    private void Start()
    {
        startScale = transform.localScale;
        transform.localScale = Vector3.zero;
        DisableTowerBehaviours();
    }

    void Update()
    {
        if (done) return;

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / buildTime);

        transform.localScale = Vector3.Lerp(Vector3.zero, startScale, progress);

        if (progress >= 1f)
        {
            EnableTowerBehaviours();
            done = true;
            Destroy(gameObject);
        }
    }

    private void DisableTowerBehaviours()
    {
        foreach (var component in towerObject.GetComponentsInChildren<MonoBehaviour>())
        {
            component.enabled = false;
        }
    }

    private void EnableTowerBehaviours()
    {
        foreach (var component in towerObject.GetComponentsInChildren<MonoBehaviour>())
        {
            component.enabled = true;
        }
    }
}
