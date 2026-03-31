using UnityEngine;
using VContainer;

/// <summary>
/// High-level ad manager controlling when and what to show.
/// Uses explicit polling/backoff timers instead of coroutine/invoke scheduling.
/// </summary>
public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    public delegate void RewardedAdResult(bool success, int rewardAmount);
    public static event RewardedAdResult OnRewardedResult;

    public enum AD_NETWORK { Unity, Admob }

    [Header("Reward Settings")]
    public AD_NETWORK rewardedNetwork = AD_NETWORK.Admob;
    public int rewardAmount = 5;
    public float minTimeBetweenWatches = 90f;
    private float lastWatchTime = -999f;

    [Header("Ad Frequency")]
    public int showInterstitialEveryDeaths = 2;
    public int showInterstitialEveryVictories = 1;

    [Header("Loading & Retry")]
    public float initialRetryDelay = 2f;
    public float maxRetryDelay = 30f;
    public float preloadPollInterval = 2f;

    private int deathCounter;
    private int victoryCounter;
    private float preloadTimer;

    private float bannerRetryDelay;
    private float interstitialRetryDelay;
    private float rewardedRetryDelay;
    private float rewardedInterstitialRetryDelay;

    private float bannerRetryTimer = -1f;
    private float interstitialRetryTimer = -1f;
    private float rewardedRetryTimer = -1f;
    private float rewardedInterstitialRetryTimer = -1f;

    private IProgressService progressService;

    [Inject]
    public void Construct(IProgressService progressService)
    {
        this.progressService = progressService;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        ProjectScope.Inject(this);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        preloadTimer = 0f;
    }

    private void Update()
    {
        if (TryRemoveAds())
            return;

        TickPreloadPoll();
        TickRetry(ref bannerRetryTimer, RetryLoadBanner);
        TickRetry(ref interstitialRetryTimer, LoadInterstitialIfNeeded);
        TickRetry(ref rewardedRetryTimer, LoadRewardedIfNeeded);
        TickRetry(ref rewardedInterstitialRetryTimer, LoadRewardedInterstitialIfNeeded);
    }

    public void ShowBanner(bool show)
    {
        if (TryRemoveAds())
            return;

        if (show)
            LoadBannerIfNeeded();
    }

    public void ShowRectBanner(bool show)
    {
        if (TryRemoveAds())
            return;

        if (show)
            LoadBannerIfNeeded(true);
    }

    public void TryShowInterstitial(GameManager.GameState state)
    {
        if (TryRemoveAds())
            return;

        if (state == GameManager.GameState.Dead)
        {
            deathCounter++;
            if (deathCounter >= Mathf.Max(1, showInterstitialEveryDeaths))
            {
                if (ShowInterstitialIfReady())
                    deathCounter = 0;
                else
                    LoadInterstitialIfNeeded();
            }
        }
        else if (state == GameManager.GameState.Finish)
        {
            victoryCounter++;
            if (victoryCounter >= Mathf.Max(1, showInterstitialEveryVictories))
            {
                if (ShowInterstitialIfReady())
                    victoryCounter = 0;
                else
                    LoadInterstitialIfNeeded();
            }
        }
    }

    public void ShowRewardedVideo()
    {
        if (TryRemoveAds())
            return;

        Debug.Log("[Ads] Rewarded not ready, loading now.");
        LoadRewardedIfNeeded();
        OnRewardedResult?.Invoke(false, 0);
        lastWatchTime = Time.realtimeSinceStartup;
    }

    public void ShowRewardedInterstitial()
    {
        if (TryRemoveAds())
            return;

        Debug.Log("[Ads] Rewarded-Interstitial not ready, loading now.");
        LoadRewardedInterstitialIfNeeded();
    }

    public float TimeUntilNextReward()
    {
        return minTimeBetweenWatches - (Time.realtimeSinceStartup - lastWatchTime);
    }

    public void ResetCounters()
    {
        deathCounter = 0;
        victoryCounter = 0;
    }

    private void TickPreloadPoll()
    {
        preloadTimer -= Time.deltaTime;
        if (preloadTimer > 0f)
            return;

        preloadTimer = preloadPollInterval;
        LoadBannerIfNeeded();
        LoadInterstitialIfNeeded();
        LoadRewardedIfNeeded();
        LoadRewardedInterstitialIfNeeded();
    }

    private void TickRetry(ref float timer, System.Action retryAction)
    {
        if (timer < 0f)
            return;

        timer -= Time.deltaTime;
        if (timer > 0f)
            return;

        timer = -1f;
        retryAction();
    }

    private void LoadBannerIfNeeded(bool isRect = false)
    {
        if (TryRemoveAds())
            return;

        if (bannerRetryDelay <= 0f)
            bannerRetryDelay = initialRetryDelay;
    }

    private void RetryLoadBanner()
    {
        LoadBannerIfNeeded();
    }

    private bool ShowInterstitialIfReady()
    {
        return false;
    }

    private void LoadInterstitialIfNeeded()
    {
        if (TryRemoveAds())
            return;

        if (interstitialRetryDelay <= 0f)
            interstitialRetryDelay = initialRetryDelay;
    }

    private void LoadRewardedIfNeeded()
    {
        if (TryRemoveAds())
            return;

        if (rewardedRetryDelay <= 0f)
            rewardedRetryDelay = initialRetryDelay;
    }

    private void LoadRewardedInterstitialIfNeeded()
    {
        if (TryRemoveAds())
            return;

        if (rewardedInterstitialRetryDelay <= 0f)
            rewardedInterstitialRetryDelay = initialRetryDelay;
    }

    private void OnBannerLoaded()
    {
        bannerRetryDelay = 0f;
        bannerRetryTimer = -1f;
    }

    private void OnBannerFailedToLoad(string error)
    {
        Backoff(ref bannerRetryDelay);
        bannerRetryTimer = bannerRetryDelay;
    }

    private void OnInterstitialLoaded()
    {
        interstitialRetryDelay = 0f;
        interstitialRetryTimer = -1f;
    }

    private void OnInterstitialFailedToLoad(string error)
    {
        Backoff(ref interstitialRetryDelay);
        interstitialRetryTimer = interstitialRetryDelay;
    }

    private void OnInterstitialClosed()
    {
        LoadInterstitialIfNeeded();
    }

    private void OnRewardedLoaded()
    {
        rewardedRetryDelay = 0f;
        rewardedRetryTimer = -1f;
    }

    private void OnRewardedFailedToLoad(string error)
    {
        Backoff(ref rewardedRetryDelay);
        rewardedRetryTimer = rewardedRetryDelay;
    }

    private void OnRewardedClosed(bool earnedReward)
    {
        OnRewardedResult?.Invoke(earnedReward, earnedReward ? rewardAmount : 0);
        LoadRewardedIfNeeded();
    }

    private void HandleRewardedResult(bool success)
    {
        OnRewardedResult?.Invoke(success, success ? rewardAmount : 0);
        Debug.Log(success ? $"Reward granted: {rewardAmount}" : "Rewarded ad failed or not watched.");
    }

    private void OnRewardedInterstitialLoaded()
    {
        rewardedInterstitialRetryDelay = 0f;
        rewardedInterstitialRetryTimer = -1f;
    }

    private void OnRewardedInterstitialFailedToLoad(string error)
    {
        Backoff(ref rewardedInterstitialRetryDelay);
        rewardedInterstitialRetryTimer = rewardedInterstitialRetryDelay;
    }

    private void OnRewardedInterstitialClosed()
    {
        LoadRewardedInterstitialIfNeeded();
    }

    private bool TryRemoveAds()
    {
        if (progressService.RemoveAds)
        {
            Debug.Log("Ads removed (IAP detected).");
            return true;
        }

        return false;
    }

    private void Backoff(ref float delayVar)
    {
        if (delayVar <= 0f)
            delayVar = initialRetryDelay;
        else
            delayVar = Mathf.Min(delayVar * 2f, maxRetryDelay);

        Debug.Log($"[Ads] Retry in {delayVar:0.#}s");
    }
}
