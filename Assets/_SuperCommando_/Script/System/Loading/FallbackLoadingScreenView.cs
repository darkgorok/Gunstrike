using UnityEngine;
using UnityEngine.UI;

public sealed class FallbackLoadingScreenView : ILoadingScreenView
{
    private readonly GameObject rootObject;
    private readonly Slider progressSlider;
    private readonly Text percentText;
    private float currentProgress;

    public FallbackLoadingScreenView(GameObject rootObject, Slider progressSlider, Text percentText)
    {
        this.rootObject = rootObject;
        this.progressSlider = progressSlider;
        this.percentText = percentText;
    }

    public bool IsComplete => currentProgress >= 0.999f;

    public void Show()
    {
        if (rootObject != null)
            rootObject.SetActive(true);
    }

    public void Hide()
    {
        if (rootObject != null)
            rootObject.SetActive(false);
    }

    public void ResetProgress()
    {
        SetProgressImmediate(0f);
    }

    public void SetTargetProgress(float progress01)
    {
        SetProgressImmediate(progress01);
    }

    public void SetProgressImmediate(float progress01)
    {
        currentProgress = Mathf.Clamp01(progress01);

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = currentProgress;
        }

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(currentProgress * 100f) + "%";
    }
}
