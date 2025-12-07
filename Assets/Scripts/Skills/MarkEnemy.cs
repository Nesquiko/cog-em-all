using UnityEngine;
using UnityEngine.InputSystem;

public class MarkEnemy : MonoBehaviour, ISkill
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float maxDistance = 500f;

    [Header("Visual")]
    [SerializeField] private GameObject linePrefab;
    //[SerializeField] private GameObject reticlePrefab;

    [Header("Marker")]
    //[SerializeField] private GameObject markerPrefab;
    [SerializeField] private float markedDuration = 10f;

    private Camera mainCamera;
    private GameObject lineInstance;
    private LineRenderer lineRenderer;
    private GameObject reticleInstance;
    private bool aiming;

    private void Awake() => mainCamera = Camera.main;

    public SkillTypes SkillType() => SkillTypes.MarkEnemy;
    public SkillActivationMode ActivationMode() => SkillActivationMode.Raycast;
    public float GetCooldown() => 60f;

    public void BeginAim()
    {
        lineInstance = Instantiate(linePrefab);
        lineRenderer = lineInstance.GetComponent<LineRenderer>();

        //reticleInstance = Instantiate(reticlePrefab);
        //reticleInstance.SetActive(false);

        aiming = true;
    }

    public void StopAim()
    {
        if (lineInstance) Destroy(lineInstance);
        if (reticleInstance) Destroy(reticleInstance);
        aiming = false;
    }

    private void Update()
    {
        if (!aiming) return;
        DrawLaser();
    }

    private void DrawLaser()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 origin = mainCamera.ScreenToWorldPoint(
            new Vector3(Screen.width / 2f, 0f, 0.2f)
        ); 
        if (Physics.Raycast(ray, out var hit, maxDistance, enemyMask | groundMask))
        {
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, hit.point);

            //reticleInstance.transform.position = hit.point + Vector3.up * 0.1f;
            //reticleInstance.SetActive(true);

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if ((enemyMask.value & (1 << hit.collider.gameObject.layer)) != 0)
                {
                    if (hit.collider.TryGetComponent<IEnemy>(out var enemy))
                    {
                        Mark(enemy);
                        StopAim();
                    }
                }
                else
                {
                    StopAim();
                }
            }
        }
        else
        {
            Vector3 end = ray.origin + ray.direction * maxDistance;
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, end);
            reticleInstance.SetActive(false);
        }
    }

    public void Mark(IEnemy enemy)
    {
        // TODO set enemy as marked
        Debug.Log($"Marked enemy: {enemy.Type}");
    }
}
