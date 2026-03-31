using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoadingScreenView : MonoBehaviour, ILoadingScreenView
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Text percentText;

    [Header("Preview / Fallback")]
    [Tooltip("Seconds to go from 0% to 100% when used as a standalone timed loader.")]
    [SerializeField] private float durationSeconds = 3f;

    [Tooltip("Start a fake timed fill automatically on enable when no external loader drives this UI.")]
    [SerializeField] private bool autoStart = false;

    [Header("Animation")]
    [Tooltip("How quickly the displayed value catches up to the target progress.")]
    [SerializeField] private float progressLerpSpeed = 3.5f;

    [Tooltip("Use unscaled time for UI animation.")]
    [SerializeField] private bool useUnscaledTime = true;

    [Tooltip("Invoke when the displayed progress visually reaches 100%.")]
    public UnityEvent onComplete;

    private Coroutine fakeLoadingRoutine;
    private float displayedProgress;
    private float targetProgress;
    private bool completionInvoked;

    public float DisplayedProgress => displayedProgress;
    public bool IsComplete => displayedProgress >= 0.999f;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ResetProgress();

        if (autoStart)
            StartLoading(durationSeconds);
    }

    private void OnDisable()
    {
        StopLoading();
    }

    private void Update()
    {
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float step = Mathf.Max(0.01f, progressLerpSpeed) * deltaTime;

        if (!Mathf.Approximately(displayedProgress, targetProgress))
        {
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, step);
            ApplyProgress(displayedProgress);
        }

        if (!completionInvoked && displayedProgress >= 0.999f && targetProgress >= 0.999f)
        {
            completionInvoked = true;
            onComplete?.Invoke();
        }
    }

    public void ResetProgress()
    {
        completionInvoked = false;
        displayedProgress = 0f;
        targetProgress = 0f;
        ApplyProgress(0f);
    }

    public void SetTargetProgress(float progress01)
    {
        targetProgress = Mathf.Clamp01(progress01);

        if (targetProgress < 0.999f)
            completionInvoked = false;
    }

    public void SetProgressImmediate(float progress01)
    {
        float clamped = Mathf.Clamp01(progress01);
        targetProgress = clamped;
        displayedProgress = clamped;

        if (clamped < 0.999f)
            completionInvoked = false;

        ApplyProgress(clamped);

        if (!completionInvoked && clamped >= 0.999f)
        {
            completionInvoked = true;
            onComplete?.Invoke();
        }
    }

    public void Complete()
    {
        SetTargetProgress(1f);
    }

    public void StartLoading(float seconds)
    {
        durationSeconds = Mathf.Max(0.0001f, seconds);

        if (fakeLoadingRoutine != null)
            StopCoroutine(fakeLoadingRoutine);

        ResetProgress();
        fakeLoadingRoutine = StartCoroutine(FillRoutine());
    }

    public void StopLoading()
    {
        if (fakeLoadingRoutine != null)
            StopCoroutine(fakeLoadingRoutine);

        fakeLoadingRoutine = null;
    }

    private IEnumerator FillRoutine()
    {
        float elapsed = 0f;

        while (elapsed < durationSeconds)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            SetTargetProgress(elapsed / durationSeconds);
            yield return null;
        }

        Complete();
        fakeLoadingRoutine = null;
    }

    private void ApplyProgress(float progress01)
    {
        if (fillImage != null)
            fillImage.fillAmount = progress01;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(progress01 * 100f) + "%";
    }
}
