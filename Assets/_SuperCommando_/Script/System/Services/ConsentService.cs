using System;
using System.Collections.Generic;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

public sealed class ConsentService : IConsentService
{
    private const string ConsentAcceptedKey = "privacy_consent_accepted";
    private const string ConsentAcceptedEvent = "consent_accepted";

    private readonly IKeyValueStore keyValueStore;
    private readonly IAnalyticsService analyticsService;

    private readonly List<Action> pendingAcceptedCallbacks = new List<Action>();
    private ConsentDialogController activeDialog;
    private bool hasRequestedUmpThisSession;

    public ConsentService(IKeyValueStore keyValueStore, IAnalyticsService analyticsService)
    {
        this.keyValueStore = keyValueStore;
        this.analyticsService = analyticsService;
    }

    public bool HasAcceptedConsent => keyValueStore.GetInt(ConsentAcceptedKey, 0) == 1;

    public bool IsConsentFlowVisible => activeDialog != null && activeDialog.IsVisible;

    public void EnsureLaunchConsent(Action onAccepted)
    {
        if (HasAcceptedConsent)
        {
            EnsureUmpSynced();
            onAccepted?.Invoke();
            return;
        }

        if (onAccepted != null)
            pendingAcceptedCallbacks.Add(onAccepted);

        if (activeDialog != null)
            return;

        var dialog = ResolveDialog();
        if (dialog == null)
            return;

        activeDialog = dialog;
        activeDialog.Show(AcceptConsent);
    }

    public void AcceptConsent()
    {
        keyValueStore.SetInt(ConsentAcceptedKey, 1);
        analyticsService.TrackEvent(ConsentAcceptedEvent);

        EnsureUmpSynced(forceRefresh: true);
        CloseDialog();

        if (pendingAcceptedCallbacks.Count == 0)
            return;

        var callbacks = pendingAcceptedCallbacks.ToArray();
        pendingAcceptedCallbacks.Clear();

        for (int i = 0; i < callbacks.Length; i++)
        {
            callbacks[i]?.Invoke();
        }
    }

    private void CloseDialog()
    {
        if (activeDialog == null)
            return;

        activeDialog.Hide();
        activeDialog = null;
    }

    private ConsentDialogController ResolveDialog()
    {
        if (activeDialog != null)
            return activeDialog;

        activeDialog = ConsentDialogController.FindSceneInstance();
        if (activeDialog == null)
            Debug.LogError("[Consent] ConsentDialogController was not found in the loaded scenes. Add a consent dialog object in the editor.");

        return activeDialog;
    }

    private void EnsureUmpSynced(bool forceRefresh = false)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (hasRequestedUmpThisSession && !forceRefresh)
            return;

        hasRequestedUmpThisSession = true;

        var parameters = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = false
        };

        ConsentInformation.Update(parameters, error =>
        {
            if (error != null)
            {
                Debug.LogWarning($"[Consent] UMP update failed: {error.Message}");
                return;
            }

            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                if (formError != null)
                    Debug.LogWarning($"[Consent] UMP consent form failed: {formError.Message}");
            });
        });
#endif
    }
}
