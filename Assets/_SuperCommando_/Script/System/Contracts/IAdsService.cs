using System;

public interface IAdsService
{
    bool IsInitialized { get; }
    bool CanShowRewarded { get; }

    void ShowBanner(bool show);
    void ShowRectBanner(bool show);
    void TryShowInterstitial(GameManager.GameState state);
    float TimeUntilNextReward();
    void ResetRewardCounters();
    void ShowRewardedVideo(Action<bool, int> onCompleted);
}
