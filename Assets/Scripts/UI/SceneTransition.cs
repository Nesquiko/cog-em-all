using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform doorTop;
    [SerializeField] private RectTransform doorBottom;
    [SerializeField] private RectTransform doorKnob;

    [Header("Animation")]
    [SerializeField] private float closeDuration = 0.7f;
    [SerializeField] private float openDuration = 0.6f;
    [SerializeField] private AnimationCurve doorEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float knobRotation = 180f;
    [SerializeField] private AnimationCurve knobEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float knobOvershoot = 12f;
    [SerializeField] private float knobSettleDuration = 0.12f;
    [SerializeField] private AnimationCurve knobSettleEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Door Shake")]
    [SerializeField] private float shakeAmplitude = 6f;
    [SerializeField] private float shakeDuration = 0.08f;
    [SerializeField] private AnimationCurve shakeDamping = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private CanvasGroup canvasGroup;
    private Quaternion knobStartRotation;

    private Vector2 topClosedPosition;
    private Vector2 bottomClosedPosition;
    private Vector2 topOpenPosition;
    private Vector2 bottomOpenPosition;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        knobStartRotation = doorKnob.localRotation;

        float screenHeight = ((RectTransform)transform).rect.height;

        topClosedPosition = doorTop.anchoredPosition;
        bottomClosedPosition = doorBottom.anchoredPosition;

        topOpenPosition = topClosedPosition + Vector2.up * screenHeight;
        bottomOpenPosition = bottomClosedPosition + Vector2.down * screenHeight;

        doorTop.anchoredPosition = topOpenPosition;
        doorBottom.anchoredPosition = bottomOpenPosition;
    }

    public void SetCanvasGroup(CanvasGroup cg)
    {
        canvasGroup = cg;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isTransitioning)
        {
            StopAllCoroutines();
            StartCoroutine(OpenDoors());
        }
    }

    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    public void ReloadCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        TransitionToScene(currentScene);
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;
        yield return CloseDoors();

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        loadOperation.allowSceneActivation = false;

        while (loadOperation.progress < 0.9f)
            yield return null;

        yield return new WaitForSecondsRealtime(0.15f);

        loadOperation.allowSceneActivation = true;
        yield return null;

        yield return OpenDoors();
        isTransitioning = false;
    }

    private IEnumerator CloseDoors()
    {
        canvasGroup.blocksRaycasts = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / closeDuration;
            float e = doorEase.Evaluate(t);
            float ke = knobEase.Evaluate(e);

            doorTop.anchoredPosition = Vector2.Lerp(topOpenPosition, topClosedPosition, e);
            doorBottom.anchoredPosition = Vector2.Lerp(bottomOpenPosition, bottomClosedPosition, e);

            float angle = -ke * (knobRotation + knobOvershoot);
            doorKnob.localRotation = knobStartRotation * Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        yield return SettleKnob(
            -(knobRotation + knobOvershoot),
            -knobRotation
        );

        yield return ShakeDoors();
    }

    private IEnumerator OpenDoors()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / openDuration;
            float e = doorEase.Evaluate(t);
            float ke = knobEase.Evaluate(e);

            doorTop.anchoredPosition = Vector2.Lerp(topClosedPosition, topOpenPosition, e);
            doorBottom.anchoredPosition = Vector2.Lerp(bottomClosedPosition, bottomOpenPosition, e);
            
            float angle = -(1f - ke) * (knobRotation + knobOvershoot);
            doorKnob.localRotation = knobStartRotation * Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        yield return SettleKnob(
            -knobOvershoot,
            0f
        );

        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator SettleKnob(float fromAngle, float toAngle)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / knobSettleDuration;
            float e = knobSettleEase.Evaluate(t);

            float angle = Mathf.Lerp(fromAngle, toAngle, e);
            doorKnob.localRotation = knobStartRotation * Quaternion.Euler(0, 0, angle);

            yield return null;
        }
    }

    private IEnumerator ShakeDoors()
    {
        float t = 0f;

        Vector2 topBase = topClosedPosition;
        Vector2 bottomBase = bottomClosedPosition;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / shakeDuration;
            float damper = shakeDamping.Evaluate(t);

            float offset = Random.Range(-1f, 1f) * shakeAmplitude * damper;

            doorTop.anchoredPosition = topBase + Vector2.up * offset;
            doorBottom.anchoredPosition = bottomBase + Vector2.down * offset;
        
            yield return null;
        }

        doorTop.anchoredPosition = topClosedPosition;
        doorBottom.anchoredPosition = bottomClosedPosition;
    }

    public static SceneTransition GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        Instance = FindFirstObjectByType<SceneTransition>();
        if (Instance != null)
            return Instance;

#if UNITY_EDITOR
        const string prefabPath = "Assets/Prefabs/UI/SceneTransition/TransitionCanvas.prefab";

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"SceneTransition prefab not found at {prefabPath}");
            return null;
        }

        var instance = Instantiate(prefab);
        return instance.GetComponent<SceneTransition>();
#else
        Debug.LogError("SceneTransition missing in runtime build!");
        return null;
#endif
    }
}