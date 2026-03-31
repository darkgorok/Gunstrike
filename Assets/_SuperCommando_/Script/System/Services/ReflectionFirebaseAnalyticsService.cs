using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public sealed class ReflectionFirebaseAnalyticsService : IAnalyticsService
{
    private const string FirebaseAnalyticsTypeName = "Firebase.Analytics.FirebaseAnalytics";

    private readonly Type firebaseAnalyticsType;
    private readonly MethodInfo logEventMethod;
    private bool hasLoggedMissingSdk;

    public ReflectionFirebaseAnalyticsService()
    {
        firebaseAnalyticsType = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType(FirebaseAnalyticsTypeName, false))
            .FirstOrDefault(type => type != null);

        logEventMethod = firebaseAnalyticsType?
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(method =>
            {
                if (method.Name != "LogEvent")
                    return false;

                var parameters = method.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
            });
    }

    public bool IsAvailable => logEventMethod != null;

    public void TrackEvent(string eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            return;

        if (!IsAvailable)
        {
            if (hasLoggedMissingSdk)
                return;

            hasLoggedMissingSdk = true;
            Debug.LogWarning($"[Analytics] Firebase Analytics SDK not found. Event '{eventName}' was not forwarded to Firebase.");
            return;
        }

        try
        {
            logEventMethod.Invoke(null, new object[] { eventName });
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[Analytics] Failed to send event '{eventName}' to Firebase: {exception.Message}");
        }
    }
}
