using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;

public sealed class FirebaseAnalyticsService : IAnalyticsService
{
    private readonly Queue<string> pendingEvents = new Queue<string>();
    private bool hasLoggedUnavailable;

    public FirebaseAnalyticsService()
    {
        FirebaseRuntimeInitializer.Ready += FlushPendingEvents;
        FirebaseRuntimeInitializer.Failed += HandleInitializationFailed;

        if (FirebaseRuntimeInitializer.IsReady)
            FlushPendingEvents();
    }

    public bool IsAvailable => FirebaseRuntimeInitializer.IsReady;

    public void TrackEvent(string eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            return;

        if (FirebaseRuntimeInitializer.IsReady)
        {
            FirebaseAnalytics.LogEvent(eventName);
            return;
        }

        if (FirebaseRuntimeInitializer.HasFailed)
        {
            LogUnavailable(eventName);
            return;
        }

        pendingEvents.Enqueue(eventName);
    }

    private void FlushPendingEvents()
    {
        while (pendingEvents.Count > 0)
            FirebaseAnalytics.LogEvent(pendingEvents.Dequeue());
    }

    private void HandleInitializationFailed(string reason)
    {
        if (pendingEvents.Count == 0)
            return;

        if (!hasLoggedUnavailable)
        {
            hasLoggedUnavailable = true;
            Debug.LogWarning($"[Analytics] Firebase initialization failed. {pendingEvents.Count} queued analytics event(s) were dropped. Reason: {reason}");
        }

        pendingEvents.Clear();
    }

    private void LogUnavailable(string eventName)
    {
        if (hasLoggedUnavailable)
            return;

        hasLoggedUnavailable = true;
        Debug.LogWarning($"[Analytics] Firebase Analytics is unavailable. Event '{eventName}' was not sent.");
    }
}
