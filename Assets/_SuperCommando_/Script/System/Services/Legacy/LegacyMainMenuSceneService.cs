using UnityEngine;

public sealed class LegacyMainMenuSceneService : IMainMenuSceneService
{
    private MainMenuHomeScene cachedMainMenuHomeScene;

    private MainMenuHomeScene Current
    {
        get
        {
            if (cachedMainMenuHomeScene == null)
                cachedMainMenuHomeScene = Object.FindFirstObjectByType<MainMenuHomeScene>();

            return cachedMainMenuHomeScene;
        }
    }

    public void LoadScene(string sceneName)
    {
        Current?.LoadScene(sceneName);
    }

    public void OpenStartMenu()
    {
        Current?.OpenStartMenu();
    }
}
