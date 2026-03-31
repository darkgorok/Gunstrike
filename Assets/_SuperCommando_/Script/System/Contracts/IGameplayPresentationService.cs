using UnityEngine;

public interface IGameplayPresentationService
{
    void SetControllerVisible(bool visible);
    void SetGameplayUiVisible(bool visible);
    void ShowWarning(bool visible);
    void ShowClean();
    void ShowBlackScreen(float duration, Color color);
    void HideBlackScreen(float duration);
}
