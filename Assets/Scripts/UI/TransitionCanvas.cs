using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TransitionCanvas : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    public CanvasGroup CanvasGroup => canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        DontDestroyOnLoad(gameObject);
    }
}
