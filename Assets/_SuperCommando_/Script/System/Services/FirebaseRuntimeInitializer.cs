using System;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using UnityEngine;

public sealed class FirebaseRuntimeInitializer : MonoBehaviour
{
    private static bool bootstrapCreated;

    public static bool IsReady { get; private set; }
    public static bool HasFailed { get; private set; }
    public static string FailureReason { get; private set; }

    public static event Action Ready;
    public static event Action<string> Failed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (bootstrapCreated)
            return;

        bootstrapCreated = true;

        var gameObject = new GameObject(nameof(FirebaseRuntimeInitializer));
        DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<FirebaseRuntimeInitializer>();
    }

    private void Awake()
    {
        if (transform.parent != null)
            transform.SetParent(null);
    }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Fail(task.Exception?.GetBaseException().Message ?? "Unknown Firebase dependency error.");
                return;
            }

            if (task.IsCanceled)
            {
                Fail("Firebase dependency check was canceled.");
                return;
            }

            if (task.Result != DependencyStatus.Available)
            {
                Fail($"Firebase dependencies are unavailable: {task.Result}");
                return;
            }

            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            IsReady = true;
            Ready?.Invoke();
        });
    }

    private static void Fail(string reason)
    {
        HasFailed = true;
        FailureReason = reason;
        Debug.LogWarning($"[Firebase] Initialization failed: {reason}");
        Failed?.Invoke(reason);
    }
}
