public interface ILoadingScreenView
{
    bool IsComplete { get; }

    void Show();
    void Hide();
    void ResetProgress();
    void SetTargetProgress(float progress01);
    void SetProgressImmediate(float progress01);
}
