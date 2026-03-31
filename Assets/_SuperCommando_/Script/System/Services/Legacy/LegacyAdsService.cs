using System;

public sealed class LegacyAdsService : IAdsService
{
    private readonly IConsentService consentService;

    public LegacyAdsService(IConsentService consentService)
    {
        this.consentService = consentService;
    }

    public bool IsInitialized => consentService.HasAcceptedConsent && AdsManager.Instance != null;
    public bool CanShowRewarded => IsInitialized;

    public void ShowBanner(bool show)
    {
        if (!IsInitialized)
            return;

        AdsManager.Instance.ShowBanner(show);
    }

    public void ShowRectBanner(bool show)
    {
        if (!IsInitialized)
            return;

        AdsManager.Instance.ShowRectBanner(show);
    }

    public void TryShowInterstitial(GameManager.GameState state)
    {
        if (!IsInitialized)
            return;

        AdsManager.Instance.TryShowInterstitial(state);
    }

    public float TimeUntilNextReward()
    {
        if (!IsInitialized)
            return 0f;

        return AdsManager.Instance.TimeUntilNextReward();
    }

    public void ResetRewardCounters()
    {
        if (!IsInitialized)
            return;

        AdsManager.Instance.ResetCounters();
    }

    public void ShowRewardedVideo(Action<bool, int> onCompleted)
    {
        if (!IsInitialized)
        {
            onCompleted?.Invoke(false, 0);
            return;
        }

        void HandleResult(bool success, int rewardAmount)
        {
            AdsManager.OnRewardedResult -= HandleResult;
            onCompleted?.Invoke(success, rewardAmount);
        }

        AdsManager.OnRewardedResult += HandleResult;
        AdsManager.Instance.ShowRewardedVideo();
    }
}
