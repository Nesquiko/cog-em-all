using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private static string targetScene;
    private static readonly float minimumLoadingTime = 1.5f;

    public static void LoadScene(string sceneName, bool showLoadingScreen)
    {
        targetScene = sceneName;

        if (showLoadingScreen)
        {
            SceneManager.LoadScene("LoadScene");
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public static IEnumerator LoadTargetScene(LoadingScreenUI ui)
    {
        yield return null;

        AsyncOperation async = SceneManager.LoadSceneAsync(targetScene);
        async.allowSceneActivation = false;

        float timer = 0f;
        float progress;

        while (!async.isDone)
        {
            progress = Mathf.Clamp01(async.progress / 0.9f);
            ui.SetProgress(progress);

            timer += Time.deltaTime;

            if (async.progress >= 0.9f && timer >= minimumLoadingTime)
            {
                ui.SetProgress(1f);
                async.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
