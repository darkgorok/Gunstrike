using System.Collections;
using UnityEngine;

/// <summary>
/// High-level ad manager controlling when and what to show.
/// Adds explicit preload, backoff, and readiness guards.
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
    [Tooltip("Initial retry delay after a failure.")]
    public float initialRetryDelay = 2f;
    [Tooltip("Maximum seconds between retries after failures.")]
    public float maxRetryDelay = 30f;

    private int deathCounter = 0;
    private int victoryCounter = 0;

    // Backoff state per format
    private float _bnRetryDelay = 0f;   // banner (incl. rect)
    private float _isRetryDelay = 0f;   // interstitial
    private float _rsRetryDelay = 0f;   // rewarded
    private float _riRetryDelay = 0f;   // rewarded interstitial

    private Coroutine _preloadLoop;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // if (AdmobController.Instance == null)
        {
            var go = new GameObject("[AdmobController]");
            go.hideFlags = HideFlags.DontSave;
            //  go.AddComponent<AdmobController>();
        }
    }

    private void OnEnable()
    {
        /*        // Interstitial
                AdmobController.OnInterstitialLoaded += OnInterstitialLoaded;
                AdmobController.OnInterstitialFailedToLoad += OnInterstitialFailedToLoad;
                AdmobController.OnInterstitialClosed += OnInterstitialClosed;

                // Rewarded
                AdmobController.OnRewardedLoaded += OnRewardedLoaded;
                AdmobController.OnRewardedFailedToLoad += OnRewardedFailedToLoad;
                AdmobController.OnRewardedClosed += OnRewardedClosed;

                // Rewarded Interstitial
                AdmobController.OnRewardedInterstitialLoaded += OnRewardedInterstitialLoaded;
                AdmobController.OnRewardedInterstitialFailedToLoad += OnRewardedInterstitialFailedToLoad;
                AdmobController.OnRewardedInterstitialClosed += OnRewardedInterstitialClosed;

                // Banner
                AdmobController.OnBannerLoaded += OnBannerLoaded;
                AdmobController.OnBannerFailedToLoad += OnBannerFailedToLoad;

                // Rewarded result passthrough
                AdmobController.AdResult += HandleRewardedResult;*/

        SafeInitAndPreload();
    }

    private void OnDisable()
    {
        /*    AdmobController.OnInterstitialLoaded -= OnInterstitialLoaded;
            AdmobController.OnInterstitialFailedToLoad -= OnInterstitialFailedToLoad;
            AdmobController.OnInterstitialClosed -= OnInterstitialClosed;

            AdmobController.OnRewardedLoaded -= OnRewardedLoaded;
            AdmobController.OnRewardedFailedToLoad -= OnRewardedFailedToLoad;
            AdmobController.OnRewardedClosed -= OnRewardedClosed;

            AdmobController.OnRewardedInterstitialLoaded -= OnRewardedInterstitialLoaded;
            AdmobController.OnRewardedInterstitialFailedToLoad -= OnRewardedInterstitialFailedToLoad;
            AdmobController.OnRewardedInterstitialClosed -= OnRewardedInterstitialClosed;

            AdmobController.OnBannerLoaded -= OnBannerLoaded;
            AdmobController.OnBannerFailedToLoad -= OnBannerFailedToLoad;

            AdmobController.AdResult -= HandleRewardedResult;*/

        if (_preloadLoop != null) StopCoroutine(_preloadLoop);
        _preloadLoop = null;
    }

    private void SafeInitAndPreload()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (_preloadLoop == null)
            _preloadLoop = StartCoroutine(PreloadLoop());
#endif
    }

    private IEnumerator PreloadLoop()
    {
        // If you gate by consent (UMP), wait here.
        // yield return UMPConsent.WaitForConsent(); // optional

        LoadBannerIfNeeded();
        LoadInterstitialIfNeeded();
        LoadRewardedIfNeeded();
        LoadRewardedInterstitialIfNeeded();

        var wait = new WaitForSeconds(2f);
        while (true)
        {
            if (TryRemoveAds())
                yield break;

            LoadBannerIfNeeded();
            LoadInterstitialIfNeeded();
            LoadRewardedIfNeeded();
            LoadRewardedInterstitialIfNeeded();
            yield return wait;
        }
    }

    // ------------------------ Banner ------------------------
    public void ShowBanner(bool show)
    {
        if (TryRemoveAds()) return;

        if (show)
        {
            LoadBannerIfNeeded();
            // AdmobController.Instance?.ShowBanner(true); // Instantiates at selected bool position
        }
        else
        {
            // AdmobController.Instance?.ShowBanner(false);
        }
    }

    public void ShowRectBanner(bool show)
    {
        if (TryRemoveAds()) return;

        if (show)
        {
            LoadBannerIfNeeded(isRect: true);
            // AdmobController.Instance?.ShowRectBanner(true); // Instantiates at selected bool corner
        }
        else
        {
            //  AdmobController.Instance?.ShowRectBanner(false);
        }
    }

    private void LoadBannerIfNeeded(bool isRect = false)
    {
        if (TryRemoveAds()) return;
        // if (AdmobController.Instance == null) return;

        //  bool isLoaded = isRect
        //      ? AdmobController.Instance.IsRectBannerLoaded
        // : AdmobController.Instance.IsBannerLoaded;

        // if (!isLoaded)
        {
            //   if (isRect) AdmobController.Instance.LoadRectBannerAd();
            //   else AdmobController.Instance.LoadBannerAd();

            if (_bnRetryDelay <= 0f) _bnRetryDelay = initialRetryDelay;
        }
    }

    private void RetryLoadBanner() => LoadBannerIfNeeded();

    private void OnBannerLoaded() => _bnRetryDelay = 0f;

    private void OnBannerFailedToLoad(string error)
    {
        Backoff(ref _bnRetryDelay);
        Invoke(nameof(RetryLoadBanner), _bnRetryDelay);
    }

    // ------------------------ Interstitial ------------------------
    public void TryShowInterstitial(GameManager.GameState state)
    {
        if (TryRemoveAds()) return;

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

    private bool ShowInterstitialIfReady()
    {
        /* if (AdmobController.Instance != null && AdmobController.Instance.IsInterstitialReady())
         {
             AdmobController.Instance.ShowInterstitialAd();
             return true;
         }*/
        return false;
    }

    private void LoadInterstitialIfNeeded()
    {
        if (TryRemoveAds()) return;
        // if (AdmobController.Instance == null) return;

        // if (!AdmobController.Instance.IsInterstitialReady())
        {
            //  AdmobController.Instance.LoadInterstitialAd();
            if (_isRetryDelay <= 0f) _isRetryDelay = initialRetryDelay;
        }
    }

    private void OnInterstitialLoaded() => _isRetryDelay = 0f;

    private void OnInterstitialFailedToLoad(string error)
    {
        Backoff(ref _isRetryDelay);
        Invoke(nameof(LoadInterstitialIfNeeded), _isRetryDelay);
    }

    private void OnInterstitialClosed() => LoadInterstitialIfNeeded();

    // ------------------------ Rewarded ------------------------
    // public bool IsRewardedReady() =>{}
    //  AdmobController.Instance?.isRewardedVideoAdReady() ?? false;

    public void ShowRewardedVideo()
    {
        if (TryRemoveAds()) return;

        // if (!IsRewardedReady())
        {
            Debug.Log("[Ads] Rewarded not ready, loading now.");
            LoadRewardedIfNeeded();
            OnRewardedResult?.Invoke(false, 0);
            return;
        }

        lastWatchTime = Time.realtimeSinceStartup;
      //  AdmobController.Instance?.WatchRewardedVideoAd();
    }

    private void LoadRewardedIfNeeded()
    {
        if (TryRemoveAds()) return;
      //  if (AdmobController.Instance == null) return;

       // if (!AdmobController.Instance.isRewardedVideoAdReady())
        {
          //  AdmobController.Instance.LoadRewardedAd();
            if (_rsRetryDelay <= 0f) _rsRetryDelay = initialRetryDelay;
        }
    }

    private void OnRewardedLoaded() => _rsRetryDelay = 0f;

    private void OnRewardedFailedToLoad(string error)
    {
        Backoff(ref _rsRetryDelay);
        Invoke(nameof(LoadRewardedIfNeeded), _rsRetryDelay);
    }

    private void OnRewardedClosed(bool earnedReward)
    {
        if (earnedReward)
            OnRewardedResult?.Invoke(true, rewardAmount);
        else
            OnRewardedResult?.Invoke(false, 0);

        LoadRewardedIfNeeded();
    }

    private void HandleRewardedResult(bool success)
    {
        if (success)
        {
            OnRewardedResult?.Invoke(true, rewardAmount);
            Debug.Log($"Reward granted: {rewardAmount}");
        }
        else
        {
            OnRewardedResult?.Invoke(false, 0);
            Debug.Log("Rewarded ad failed or not watched.");
        }
    }

    // ------------------------ Rewarded Interstitial ------------------------
    public void ShowRewardedInterstitial()
    {
        if (TryRemoveAds()) return;

      //  if (AdmobController.Instance != null && AdmobController.Instance.IsRewardedInterstitialReady())
      //  {
          //  AdmobController.Instance.ShowRewardedInterstitial();
       // }
        else
        {
            Debug.Log("[Ads] Rewarded-Interstitial not ready, loading now.");
            LoadRewardedInterstitialIfNeeded();
        }
    }

    private void LoadRewardedInterstitialIfNeeded()
    {
        if (TryRemoveAds()) return;
       // if (AdmobController.Instance == null) return;

       // if (!AdmobController.Instance.IsRewardedInterstitialReady())
        {
            //  AdmobController.Instance.LoadRewardedInterstitial();
            if (_riRetryDelay <= 0f) _riRetryDelay = initialRetryDelay;
        }
    }

    private void OnRewardedInterstitialLoaded() => _riRetryDelay = 0f;

    private void OnRewardedInterstitialFailedToLoad(string error)
    {
        Backoff(ref _riRetryDelay);
        Invoke(nameof(LoadRewardedInterstitialIfNeeded), _riRetryDelay);
    }

    private void OnRewardedInterstitialClosed() => LoadRewardedInterstitialIfNeeded();

    // ------------------------ Utility ------------------------
    public float TimeUntilNextReward()
    {
        return minTimeBetweenWatches - (Time.realtimeSinceStartup - lastWatchTime);
    }

    private bool TryRemoveAds()
    {
        if (GlobalValue.RemoveAds)
        {
            Debug.Log("Ads removed (IAP detected).");
            return true;
        }
        return false;
    }

    public void ResetCounters()
    {
        deathCounter = 0;
        victoryCounter = 0;
    }

    private void Backoff(ref float delayVar)
    {
        if (delayVar <= 0f) delayVar = initialRetryDelay;
        else delayVar = Mathf.Min(delayVar * 2f, maxRetryDelay);
        Debug.Log($"[Ads] Retry in {delayVar:0.#}s");
    }
}
