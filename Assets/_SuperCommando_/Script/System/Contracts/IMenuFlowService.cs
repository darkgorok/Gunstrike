using UnityEngine;

public interface IMenuFlowService
{
    Transform UiRoot { get; }
    void Pause();
    void RestartGame();
    void OpenSaveMe(bool open);
    void ShowGameOver();
    void LoadNextLevel();
}
