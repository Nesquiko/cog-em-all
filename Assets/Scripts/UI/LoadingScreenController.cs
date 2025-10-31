using UnityEngine;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private LoadingScreenUI ui;

    private void Start()
    {
        StartCoroutine(SceneLoader.LoadTargetScene(ui));
    }
}
