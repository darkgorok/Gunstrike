using UnityEngine;

public sealed class LegacyGameplayPresentationService : IGameplayPresentationService
{
    private MenuManager cachedMenuManager;

    private MenuManager CurrentMenuManager
    {
        get
        {
            if (cachedMenuManager == null)
                cachedMenuManager = Object.FindFirstObjectByType<MenuManager>();

            return cachedMenuManager;
        }
    }

    public void SetControllerVisible(bool visible)
    {
        if (CurrentMenuManager != null)
            CurrentMenuManager.TurnController(visible);
    }

    public void SetGameplayUiVisible(bool visible)
    {
        if (CurrentMenuManager != null)
            CurrentMenuManager.TurnGUI(visible);
    }

    public void ShowWarning(bool visible)
    {
        if (GroupEnemySystemUI.Instance != null)
            GroupEnemySystemUI.Instance.ShowWarning(visible);
    }

    public void ShowClean()
    {
        if (GroupEnemySystemUI.Instance != null)
            GroupEnemySystemUI.Instance.ShowClean();
    }

    public void ShowBlackScreen(float duration, Color color)
    {
        if (BlackScreenUI.instance != null)
            BlackScreenUI.instance.Show(duration, color);
    }

    public void HideBlackScreen(float duration)
    {
        if (BlackScreenUI.instance != null)
            BlackScreenUI.instance.Hide(duration);
    }
}
