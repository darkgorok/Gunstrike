using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
// If you use TextMeshPro for the label, uncomment the line below
// using TMPro;

public class TimedLoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;   // optional
    [SerializeField] private Image fillImage;         // optional (Image with Fill Method = Horizontal)
    [SerializeField] private Text percentText;        // <= Unity UI Text
    // [SerializeField] private TextMeshProUGUI percentTMP; // <= Use this instead of 'percentText' if you prefer TMP

    [Header("Timing")]
    [Tooltip("Seconds to go from 0% to 100%.")]
    [SerializeField] private float durationSeconds = 3f;

    [Tooltip("Start filling automatically on enable.")]
    [SerializeField] private bool autoStart = true;

    [Header("Behaviour")]
    [Tooltip("Use unscaled time (ignores Time.timeScale).")]
    [SerializeField] private bool useUnscaledTime = true;

    [Tooltip("Invoke when progress reaches 100%.")]
    public UnityEvent onComplete;

    Coroutine routine;

    void OnEnable()
    {
        if (autoStart) StartLoading(durationSeconds);
        else SetProgress(0f);
    }

    /// <summary>Call this to start (or restart) the timed loading.</summary>
    public void StartLoading(float seconds)
    {
        durationSeconds = Mathf.Max(0.0001f, seconds);
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FillRoutine());
    }

    IEnumerator FillRoutine()
    {
        float t = 0f;
        SetProgress(0f);

        while (t < durationSeconds)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float p = Mathf.Clamp01(t / durationSeconds);
            SetProgress(p);
            yield return null;
        }

        SetProgress(1f);
        routine = null;
        onComplete?.Invoke();
    }

    void SetProgress(float p01)
    {
        // Slider (expects min=0, max=1)
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = p01;
        }

        // Image fill (set Image.type=Filled, Fill Method=Horizontal)
        if (fillImage != null) fillImage.fillAmount = p01;

        // Percentage text
        int pct = Mathf.RoundToInt(p01 * 100f);
        if (percentText != null) percentText.text = pct + "%";
        // if (percentTMP != null) percentTMP.text = pct + "%";
    }

    // Optional: stop mid-way
    public void StopLoading()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
    }
}