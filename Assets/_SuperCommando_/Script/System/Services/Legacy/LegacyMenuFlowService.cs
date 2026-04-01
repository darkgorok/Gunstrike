using UnityEngine;

public sealed class LegacyMenuFlowService : IMenuFlowService
{
    private MenuManager cachedMenuManager;

    private MenuManager Current
    {
        get
        {
            if (cachedMenuManager == null)
                cachedMenuManager = Object.FindFirstObjectByType<MenuManager>();

            return cachedMenuManager;
        }
    }

    public Transform UiRoot => Current != null ? Current.transform : null;

    public void Pause()
    {
        if (Current != null)
            Current.Pause();
    }

    public void RestartGame()
    {
        if (Current != null)
            Current.RestartGame();
    }

    public void OpenSaveMe(bool open)
    {
        if (Current != null)
            Current.OpenSaveMe(open);
    }

    public void ShowGameOver()
    {
        if (Current != null)
            Current.GameOver();
    }

    public void ShowGameFinish()
    {
        if (Current != null)
            Current.Gamefinish();
    }

    public void LoadNextLevel()
    {
        if (Current != null)
            Current.NextLevel();
    }
}
