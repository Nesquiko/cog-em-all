using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TransitionCanvas : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private SceneTransition sceneTransition;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        sceneTransition = GetComponentInChildren<SceneTransition>();
        sceneTransition.SetCanvasGroup(canvasGroup);
        DontDestroyOnLoad(gameObject);
    }
}
