using UnityEngine;

public interface ISceneLoader
{
    Coroutine BeginLoad(MonoBehaviour runner, string sceneName, ILoadingScreenView loadingView = null, SceneLoadOptions options = null);
    Coroutine BeginReloadCurrent(MonoBehaviour runner, ILoadingScreenView loadingView = null, SceneLoadOptions options = null);

    void LoadImmediate(string sceneName);
    void LoadImmediate(int sceneBuildIndex);
    AsyncOperation LoadImmediateAsync(string sceneName);
    AsyncOperation LoadImmediateAsync(int sceneBuildIndex);
}
