using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GearDropManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GearDrop gearDropPrefab;
    [SerializeField] private RectTransform gearIconUI;
    [SerializeField] private Canvas canvasUI;
    [SerializeField] private float targetDepth = 10f;

    [Header("Behaviour")]
    [SerializeField] private int prewarmCount = 50;
    [SerializeField] private float targetRecalcInterval = 0.2f;
    [SerializeField] private float collectionDelay = 0.1f;

    private ObjectPool<GearDrop> pool;
    private readonly List<GearDrop> active = new();

    private Camera mainCamera;
    private Vector3 cachedWorldTarget;
    private float recalcTimer;

    private void Awake()
    {
        mainCamera = Camera.main;

        pool = new ObjectPool<GearDrop>(
            Create,
            OnGet,
            OnRelease,
            OnDestroyGearDrop,
            collectionCheck: false,
            defaultCapacity: prewarmCount
        );

        for (int i = 0; i < prewarmCount; i++)
            pool.Release(pool.Get());
    }

    private GearDrop Create()
    {
        var gearDrop = Instantiate(gearDropPrefab, transform);
        gearDrop.gameObject.SetActive(false);
        return gearDrop;
    }

    private void OnGet(GearDrop gearDrop) => gearDrop.gameObject.SetActive(true);
    private void OnRelease(GearDrop gearDrop) => gearDrop.gameObject.SetActive(false);
    private void OnDestroyGearDrop(GearDrop gearDrop) => Destroy(gearDrop.gameObject);

    private void Update()
    {
        recalcTimer -= Time.deltaTime;
        if (recalcTimer <= 0f)
        {
            recalcTimer = targetRecalcInterval;
            cachedWorldTarget = ComputeWorldTarget();
        }

        float t = Time.deltaTime;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var gearDrop = active[i];
            gearDrop.Tick(t, cachedWorldTarget);
            if (gearDrop.Done)
            {
                active.RemoveAt(i);
                HandleCollected(gearDrop);
            }
        }
    }

    private Vector3 ComputeWorldTarget()
    {
        Vector3 screen = gearIconUI.position;
        Vector3 world = mainCamera.ScreenToWorldPoint(new(screen.x, screen.y, targetDepth));
        return world;
    }

    public void SpawnGears(Vector3 worldPosition, int gears)
    {
        int count = (int) gears / 10;

        count = Mathf.Max(1, count);
        for (int i = 0; i < count; i++)
        {
            var gearDrop = pool.Get();
            active.Add(gearDrop);
            Vector3 offset = Random.insideUnitSphere * 0.5f;
            offset.y = Mathf.Abs(offset.y) * 1.5f;
            gearDrop.Activate(worldPosition + offset);
        }
    }

    private void HandleCollected(GearDrop gearDrop)
    {
        StartCoroutine(DelayedRelease(gearDrop));
    }

    private IEnumerator DelayedRelease(GearDrop gearDrop)
    {
        yield return new WaitForSeconds(collectionDelay);
        pool.Release(gearDrop);
        // update currency here
    }
}
