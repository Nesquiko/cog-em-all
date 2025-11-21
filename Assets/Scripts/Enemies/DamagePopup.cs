using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float riseDistance = 1.0f;
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private Vector3 randomSpawnOffset = new(1f, 0.5f, 1f);

    [Header("Scaling")]
    [SerializeField] private float sizeOnScreen = 1.0f;
    [SerializeField] private bool keepConstantScreenSize = true;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color critColor = new(1.0f, 0.85f, 0.2f);

    [Header("References")]
    [SerializeField] private TextMeshPro text;

    private float timer;
    private Vector3 startPosition;
    private Color startColor;
    private bool isActive;
    private float initialScale;
    private float lastDistance = 1f;

    public void Initialize()
    {
        text = GetComponentInChildren<TextMeshPro>();
        initialScale = transform.localScale.x;
    }

    public void Activate(Vector3 worldPosition, float damage, bool isCritical = false)
    {
        Vector3 offset = new(
            Random.Range(-randomSpawnOffset.x, randomSpawnOffset.x),
            Random.Range(0f, randomSpawnOffset.y),
            Random.Range(-randomSpawnOffset.z, randomSpawnOffset.z)
        );

        startPosition = worldPosition + offset;
        transform.position = startPosition;

        text.text = Mathf.RoundToInt(damage).ToString();
        text.color = isCritical ? critColor : normalColor;
        startColor = text.color;

        text.fontStyle = isCritical ? FontStyles.Bold : FontStyles.Normal;
        float multi = isCritical ? 1.3f : 1f;
        transform.localScale = initialScale * multi * Vector3.one;

        timer = 0f;
        isActive = true;
        gameObject.SetActive(true);
    }

    public bool Tick(float deltaTime, float cachedDistance)
    {
        if (cachedDistance > 0f)
            lastDistance = Mathf.Lerp(lastDistance, cachedDistance, 0.25f);

        float usedDistance = keepConstantScreenSize ? lastDistance : 1f;

        if (!isActive) return false;

        timer += deltaTime;
        float t = timer / duration;

        float scale = Mathf.Lerp(initialScale, 0.5f * initialScale, t);
        if (keepConstantScreenSize)
        {
            float distanceScale = usedDistance * 0.1f * sizeOnScreen;
            scale += distanceScale;
        }
        transform.localScale = Vector3.one * scale;

        float rise = riseDistance;
        if (keepConstantScreenSize)
            rise *= usedDistance * 0.1f * sizeOnScreen;

        transform.position = startPosition + Vector3.up * Mathf.Lerp(0, rise, t);

        Color c = startColor;
        c.a = 1f - t;
        text.color = c;

        if (timer >= duration)
        {
            isActive = false;
            gameObject.SetActive(false);
            return false;
        }

        return true;
    }
}
