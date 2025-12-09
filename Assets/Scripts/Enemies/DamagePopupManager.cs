using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class DamagePopupManager : MonoBehaviour
{
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private int prewarmCount = 50;
    [SerializeField] private int distanceUpdateEveryFrames = 5;
    [SerializeField] private float popupHeightOffset = 10f;

    [SerializeField] private PauseManager pauseManager;

    private ObjectPool<DamagePopup> pool;
    private readonly List<DamagePopup> activePopups = new();
    private Camera mainCamera;
    private int frameCounter;

    private void Awake()
    {
        mainCamera = Camera.main;

        pool = new(
            CreatePopup,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPoolObject,
            collectionCheck: false,
            defaultCapacity: prewarmCount
        );

        for (int i = 0; i < prewarmCount; i++)
        {
            var p = pool.Get();
            pool.Release(p);
        }
    }

    private DamagePopup CreatePopup()
    {
        var p = Instantiate(damagePopupPrefab, transform);
        p.Initialize();
        return p;
    }

    private void OnGetFromPool(DamagePopup p)
    {
        p.gameObject.SetActive(true);
    }

    private void OnReleaseToPool(DamagePopup p)
    {
        p.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(DamagePopup p)
    {
        Destroy(p.gameObject);
    }

    public void ShowPopup(Vector3 worldPosition, float damage, bool isCritical = false)
    {
        if (PlayerPrefs.GetInt("ShowDamageDealt") == 0) return;
        worldPosition += Vector3.up * popupHeightOffset;
        var popup = pool.Get();
        popup.Activate(worldPosition, damage, isCritical);
        activePopups.Add(popup);
    }

    private void Update()
    {
        if (pauseManager.Paused) return;

        frameCounter++;
        float deltaTime = Time.deltaTime;

        bool shouldRecalcDistance = frameCounter % distanceUpdateEveryFrames == 0;

        for (int i = activePopups.Count - 1; i >= 0; i--)
        {
            var popup = activePopups[i];
            float distance = 0f;

            if (shouldRecalcDistance)
            {
                Vector3 pos = popup.transform.position;
                distance = (mainCamera.transform.position - pos).magnitude;
            }

            bool alive = popup.Tick(deltaTime, distance);
            if (!alive)
            {
                activePopups.RemoveAt(i);
                pool.Release(popup);
            }
        }
    }
}
