using UnityEngine;

public sealed class LegacyGameplayPresentationService : IGameplayPresentationService
{
    public void SetControllerVisible(bool visible)
    {
        if (MenuManager.Instance != null)
            MenuManager.Instance.TurnController(visible);
    }

    public void SetGameplayUiVisible(bool visible)
    {
        if (MenuManager.Instance != null)
            MenuManager.Instance.TurnGUI(visible);
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
