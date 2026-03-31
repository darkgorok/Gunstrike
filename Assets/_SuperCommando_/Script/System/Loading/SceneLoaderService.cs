using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoaderService : ISceneLoader
{
    public Coroutine BeginLoad(MonoBehaviour runner, string sceneName, ILoadingScreenView loadingView = null, SceneLoadOptions options = null)
    {
        if (runner == null)
            return null;

        return runner.StartCoroutine(LoadRoutine(sceneName, loadingView, options ?? new SceneLoadOptions()));
    }

    public Coroutine BeginReloadCurrent(MonoBehaviour runner, ILoadingScreenView loadingView = null, SceneLoadOptions options = null)
    {
        return BeginLoad(runner, SceneManager.GetActiveScene().name, loadingView, options);
    }

    public void LoadImmediate(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadImmediate(int sceneBuildIndex)
    {
        SceneManager.LoadScene(sceneBuildIndex);
    }

    public AsyncOperation LoadImmediateAsync(string sceneName)
    {
        return SceneManager.LoadSceneAsync(sceneName);
    }

    public AsyncOperation LoadImmediateAsync(int sceneBuildIndex)
    {
        return SceneManager.LoadSceneAsync(sceneBuildIndex);
    }

    private IEnumerator LoadRoutine(string sceneName, ILoadingScreenView loadingView, SceneLoadOptions options)
    {
        if (loadingView != null)
        {
            loadingView.Show();
            loadingView.ResetProgress();
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float elapsed = 0f;
        float displayedTarget = 0f;

        while (operation.progress < 0.9f || elapsed < options.MinVisibleSeconds)
        {
            elapsed += Time.unscaledDeltaTime;

            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            displayedTarget = Mathf.Max(displayedTarget, realProgress);

            loadingView?.SetTargetProgress(displayedTarget);
            options.OnLoadingTick?.Invoke();
            yield return null;
        }

        loadingView?.SetTargetProgress(1f);

        float holdUntil = Time.unscaledTime + Mathf.Max(0f, options.CompleteHoldSeconds);
        while (Time.unscaledTime < holdUntil || (loadingView != null && !loadingView.IsComplete))
        {
            options.OnLoadingTick?.Invoke();
            yield return null;
        }

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            options.OnLoadingTick?.Invoke();
            yield return null;
        }
    }
}
