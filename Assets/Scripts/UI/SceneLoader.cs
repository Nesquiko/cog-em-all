using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private static string targetScene;
    private static readonly float minimumLoadingTime = 2f;

    public static void LoadScene(string sceneName)
    {
        targetScene = sceneName;
        SceneManager.LoadScene("LoadScene");
    }

    public static IEnumerator LoadTargetScene(Action<float> onProgress)
    {
        yield return null;

        AsyncOperation async = SceneManager.LoadSceneAsync(targetScene);
        async.allowSceneActivation = false;

        float timer = 0f;
        float progress;

        while (!async.isDone)
        {
            progress = Mathf.Clamp01(async.progress / 0.9f);
            onProgress?.Invoke(progress);

            timer += Time.deltaTime;

            if (async.progress >= 0.9f && timer >= minimumLoadingTime)
            {
                onProgress?.Invoke(1f);
                async.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    public static void ReloadCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene);
    }
}
