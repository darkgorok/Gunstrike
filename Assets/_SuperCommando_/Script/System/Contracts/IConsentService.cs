using System;

public interface IConsentService
{
    bool HasAcceptedConsent { get; }
    bool IsConsentFlowVisible { get; }

    void EnsureLaunchConsent(Action onAccepted);
    void AcceptConsent();
}
