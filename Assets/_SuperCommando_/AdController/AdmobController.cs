/*using UnityEngine;
using System;

public class AdmobController : MonoBehaviour
{
    public static AdmobController Instance;

    [Header("General Settings")]
    public bool useTestAds = true;
    public bool showDebugLogs = true;

    [Header("Android IDs")]
    public string androidAppId = "ca-app-pub-3940256099942544~3347511713";
    public string androidBannerId = "ca-app-pub-3940256099942544/6300978111";
    public string androidRectBannerId = "ca-app-pub-3940256099942544/6300978111";
    public string androidInterstitialId = "ca-app-pub-3940256099942544/1033173712";
    public string androidRewardedId = "ca-app-pub-3940256099942544/5224354917";
    public string androidRewardedInterstitialId = "ca-app-pub-3940256099942544/5354046379";

    [Header("iOS IDs")]
    public string iosAppId = "ca-app-pub-3940256099942544~1458002511";
    public string iosBannerId = "ca-app-pub-3940256099942544/2934735716";
    public string iosRectBannerId = "ca-app-pub-3940256099942544/2934735716";
    public string iosInterstitialId = "ca-app-pub-3940256099942544/4411468910";
    public string iosRewardedId = "ca-app-pub-3940256099942544/1712485313";
    public string iosRewardedInterstitialId = "ca-app-pub-3940256099942544/6978759866";

    [Header("Banner Placement (pick ONE)")]
    public bool bannerTopCenter = false;
    public bool bannerBottomCenter = true; // default

    [Header("Rect Banner Placement (pick ONE)")]
    public bool rectTopLeft = false;
    public bool rectTopRight = false;
    public bool rectBottomLeft = false;
    public bool rectBottomRight = true; // default

    // Google Mobile Ads objects
  //  private BannerView bannerView;
 //   private BannerView rectBannerView;
 //   private InterstitialAd interstitialAd;
 //   private RewardedAd rewardedAd;
 //   private RewardedInterstitialAd rewardedInterstitialAd;

    // Events used by AdsManager
    public static event Action OnInterstitialLoaded;
    public static event Action<string> OnInterstitialFailedToLoad;
    public static event Action OnInterstitialClosed;

    public static event Action OnRewardedLoaded;
    public static event Action<string> OnRewardedFailedToLoad;
    public static event Action<bool> OnRewardedClosed;

    public static event Action OnRewardedInterstitialLoaded;
    public static event Action<string> OnRewardedInterstitialFailedToLoad;
    public static event Action OnRewardedInterstitialClosed;

    public static event Action OnBannerLoaded;
    public static event Action<string> OnBannerFailedToLoad;

    public static event Action<bool> AdResult; // simple “earned reward?” callback

    // Readiness flags for banners
    public bool IsBannerLoaded { get; private set; }
    public bool IsRectBannerLoaded { get; private set; }

    // Internal earned flags
    private bool _rewardEarnedFlag;
    private bool _riEarnedFlag;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        string appId =
#if UNITY_IOS
            iosAppId;
#else
            androidAppId;
#endif
        //MobileAds.Initialize(_ =>
        //{
        //    Log("AdMob initialized");
        //    LoadAllAds();
        //});
    }

    public void LoadAllAds()
    {
        //LoadBannerAd();         // will respect selected bool position when shown
        //LoadRectBannerAd();     // will respect selected bool corner when shown
        //LoadInterstitialAd();
        //LoadRewardedAd();
        //LoadRewardedInterstitialAd();
    }

    // ----------------------- Banner -----------------------
  //  public void LoadBannerAd(AdPosition position = AdPosition.Bottom)
    //{
       *//* DestroyBanner();

        string adUnitId = GetBannerAdUnitId();
        bannerView = new BannerView(adUnitId, AdSize.Banner, position);

        bannerView.OnBannerAdLoaded += () =>
        {
            IsBannerLoaded = true;
            OnBannerLoaded?.Invoke();
            Log("Banner loaded.");
        };
        bannerView.OnBannerAdLoadFailed += (LoadAdError err) =>
        {
            IsBannerLoaded = false;
            OnBannerFailedToLoad?.Invoke(err == null ? "unknown" : err.ToString());
            Log("Banner failed: " + err);
        };

        bannerView.LoadAd(CreateAdRequest());
        Log("Banner requested...");*//*
    }

  //  public void LoadRectBannerAd(AdPosition position = AdPosition.Bottom)
  //  {
       *//* DestroyRectBanner();

        string adUnitId = GetRectBannerAdUnitId();
        rectBannerView = new BannerView(adUnitId, AdSize.MediumRectangle, position);

        rectBannerView.OnBannerAdLoaded += () =>
        {
            IsRectBannerLoaded = true;
            OnBannerLoaded?.Invoke();
            Log("Rect banner loaded.");
        };
        rectBannerView.OnBannerAdLoadFailed += (LoadAdError err) =>
        {
            IsRectBannerLoaded = false;
            OnBannerFailedToLoad?.Invoke(err == null ? "unknown" : err.ToString());
            Log("Rect banner failed: " + err);
        };

        rectBannerView.LoadAd(CreateAdRequest());
        Log("Rect banner requested...");*//*
  //  }
  
    public void ShowBanner(bool show)
    {
       *//* if (show)
        {
            // Recreate at selected position to ensure correct placement
            var pos = GetSelectedBannerPosition();
            LoadBannerAd(pos);
            if (bannerView != null) bannerView.Show();
            Log($"Banner shown at {pos}");
        }
        else
        {
            if (bannerView != null) bannerView.Hide();
            Log("Banner hidden");
        }*//*
    }

    public void ShowRectBanner(bool show)
    {
       *//* if (show)
        {
            var pos = GetSelectedRectPosition();
            LoadRectBannerAd(pos);
            if (rectBannerView != null) rectBannerView.Show();
            Log($"Rect banner shown at {pos}");
        }
        else
        {
            if (rectBannerView != null) rectBannerView.Hide();
            Log("Rect banner hidden");
        }*//*
    }

    public void DestroyBanner()
    {
       *//* bannerView?.Destroy();
        bannerView = null;
        IsBannerLoaded = false;*//*
    }

    public void DestroyRectBanner()
    {
       *//* rectBannerView?.Destroy();
        rectBannerView = null;
        IsRectBannerLoaded = false;*//*
    }

    // --------------------- Interstitial -------------------
    public void LoadInterstitialAd()
    {
        //string adUnitId = GetInterstitialAdUnitId();

        //InterstitialAd.Load(adUnitId, CreateAdRequest(),
        //    (InterstitialAd ad, LoadAdError error) =>
        //    {
        //        if (error != null || ad == null)
        //        {
        //            Log("Failed to load interstitial: " + error);
        //            OnInterstitialFailedToLoad?.Invoke(error == null ? "unknown" : error.ToString());
        //            return;
        //        }

        //        interstitialAd = ad;
        //        OnInterstitialLoaded?.Invoke();
        //        Log("Interstitial loaded.");

        //        interstitialAd.OnAdFullScreenContentClosed += () =>
        //        {
        //            OnInterstitialClosed?.Invoke();
        //            Log("Interstitial closed.");
        //            interstitialAd?.Destroy();
        //            interstitialAd = null;
        //            LoadInterstitialAd(); // keep warm
        //        };
        //        interstitialAd.OnAdFullScreenContentFailed += (AdError adError) =>
        //        {
        //            Log("Interstitial show failed: " + adError);
        //            interstitialAd?.Destroy();
        //            interstitialAd = null;
        //        };
        //    });
    }

    public bool IsInterstitialReady() =>
       // interstitialAd != null && interstitialAd.CanShowAd();

    public void ShowInterstitialAd()
    {
       *//* if (IsInterstitialReady()) interstitialAd.Show();
        else Log("Interstitial not ready.");*//*
    }

    // Legacy helper kept for compatibility
  //  public bool ForceShowInterstitialAd()
    {
       *//* if (IsInterstitialReady())
        {
            interstitialAd.Show();
            return true;
        }
        Log("Interstitial not ready.");
        return false;*//*
    }

    // ------------------------ Rewarded --------------------
    public void LoadRewardedAd()
    {
        //string adUnitId = GetRewardedAdUnitId();

        //RewardedAd.Load(adUnitId, CreateAdRequest(),
        //    (RewardedAd ad, LoadAdError error) =>
        //    {
        //        if (error != null || ad == null)
        //        {
        //            Log("Failed to load rewarded: " + error);
        //            OnRewardedFailedToLoad?.Invoke(error == null ? "unknown" : error.ToString());
        //            return;
        //        }

        //        rewardedAd = ad;
        //        OnRewardedLoaded?.Invoke();
        //        Log("Rewarded loaded.");

        //        rewardedAd.OnAdFullScreenContentClosed += () =>
        //        {
        //            OnRewardedClosed?.Invoke(_rewardEarnedFlag);
        //            Log("Rewarded closed. earned=" + _rewardEarnedFlag);
        //            rewardedAd?.Destroy();
        //            rewardedAd = null;
        //            _rewardEarnedFlag = false;
        //            LoadRewardedAd();
        //        };
        //        rewardedAd.OnAdFullScreenContentFailed += (AdError adError) =>
        //        {
        //            Log("Rewarded show failed: " + adError);
        //            rewardedAd?.Destroy();
        //            rewardedAd = null;
        //            _rewardEarnedFlag = false;
        //        };
        //    });
    }

  //  public bool isRewardedVideoAdReady() =>
      //  rewardedAd != null && rewardedAd.CanShowAd();

    public void WatchRewardedVideoAd()
    {
        //if (isRewardedVideoAdReady())
        //{
        //    _rewardEarnedFlag = false;
        //    rewardedAd.Show(reward =>
        //    {
        //        _rewardEarnedFlag = true;
        //        Log($"Reward: {reward.Type} amount: {reward.Amount}");
        //        AdResult?.Invoke(true);
        //    });
        //}
        //else
        //{
        //    Log("Rewarded not ready. Reloading...");
        //    LoadRewardedAd();
        //    AdResult?.Invoke(false);
        //}
    }

    // --------------- Rewarded Interstitial ---------------
    public void LoadRewardedInterstitialAd()
    {
     *//*   string adUnitId = GetRewardedInterstitialAdUnitId();

        RewardedInterstitialAd.Load(adUnitId, CreateAdRequest(),
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Log("Failed to load rewarded interstitial: " + error);
                    OnRewardedInterstitialFailedToLoad?.Invoke(error == null ? "unknown" : error.ToString());
                    return;
                }

                rewardedInterstitialAd = ad;
                OnRewardedInterstitialLoaded?.Invoke();
                Log("Rewarded interstitial loaded.");

                rewardedInterstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    OnRewardedInterstitialClosed?.Invoke();
                    Log("Rewarded interstitial closed. earned=" + _riEarnedFlag);
                    rewardedInterstitialAd?.Destroy();
                    rewardedInterstitialAd = null;
                    _riEarnedFlag = false;
                    LoadRewardedInterstitialAd();
                };
                rewardedInterstitialAd.OnAdFullScreenContentFailed += (AdError adError) =>
                {
                    Log("Rewarded interstitial show failed: " + adError);
                    rewardedInterstitialAd?.Destroy();
                    rewardedInterstitialAd = null;
                    _riEarnedFlag = false;
                };
            });*//*
    }

    // Alias used by AdsManager
    public void LoadRewardedInterstitial() => LoadRewardedInterstitialAd();

    //public bool IsRewardedInterstitialReady() =>
    //   rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd();

    public void ShowRewardedInterstitial()
    {
        //if (IsRewardedInterstitialReady())
        //{
        //    _riEarnedFlag = false;
        //    rewardedInterstitialAd.Show(reward =>
        //    {
        //        _riEarnedFlag = true;
        //        Log($"RewardedInterstitial reward: {reward.Type} {reward.Amount}");
        //        AdResult?.Invoke(true);
        //    });
        //}
        //else
        //{
        //    Log("Rewarded interstitial not ready. Reloading...");
        //    LoadRewardedInterstitialAd();
        //    AdResult?.Invoke(false);
        //}
    }

    // ------------------------ Helpers --------------------
    //private AdRequest CreateAdRequest()
    //{
    //    return new AdRequest();
    //}

    private string GetBannerAdUnitId()
    {
#if UNITY_IOS
        return useTestAds ? "ca-app-pub-3940256099942544/2934735716" : iosBannerId;
#else
     //   return useTestAds ? "ca-app-pub-3940256099942544/6300978111" : androidBannerId;
#endif
    }

    private string GetRectBannerAdUnitId()
    {
#if UNITY_IOS
        return useTestAds ? "ca-app-pub-3940256099942544/2934735716" : iosRectBannerId;
#else
        return useTestAds ? "ca-app-pub-3940256099942544/6300978111" : androidRectBannerId;
#endif
    }

    private string GetInterstitialAdUnitId()
    {
#if UNITY_IOS
        return useTestAds ? "ca-app-pub-3940256099942544/4411468910" : iosInterstitialId;
#else
        return useTestAds ? "ca-app-pub-3940256099942544/1033173712" : androidInterstitialId;
#endif
    }

    private string GetRewardedAdUnitId()
    {
#if UNITY_IOS
        return useTestAds ? "ca-app-pub-3940256099942544/1712485313" : iosRewardedId;
#else
        return useTestAds ? "ca-app-pub-3940256099942544/5224354917" : androidRewardedId;
#endif
    }

    private string GetRewardedInterstitialAdUnitId()
    {
#if UNITY_IOS
        return useTestAds ? "ca-app-pub-3940256099942544/6978759866" : iosRewardedInterstitialId;
#else
        return useTestAds ? "ca-app-pub-3940256099942544/5354046379" : androidRewardedInterstitialId;
#endif
    }

  *//*  private AdPosition GetSelectedBannerPosition()
    {
        // Only two options; if both off, default to Bottom.
        if (bannerTopCenter && !bannerBottomCenter) return AdPosition.Top;
        if (!bannerTopCenter && bannerBottomCenter) return AdPosition.Bottom;

        // If both true or both false, prefer Bottom to avoid overlap/conflict.
        return AdPosition.Bottom;
    }*//*

    //private AdPosition GetSelectedRectPosition()
    //{
    //    int selected =
    //        (rectTopLeft ? 1 : 0) +
    //        (rectTopRight ? 1 : 0) +
    //        (rectBottomLeft ? 1 : 0) +
    //        (rectBottomRight ? 1 : 0);

    //    // Prefer a single selection; fallback to BottomRight as default.
    //    if (selected == 1)
    //    {
    //        if (rectTopLeft) return AdPosition.TopLeft;
    //        if (rectTopRight) return AdPosition.TopRight;
    //        if (rectBottomLeft) return AdPosition.BottomLeft;
    //        if (rectBottomRight) return AdPosition.BottomRight;
    //    }

    //    // If none or multiple are selected, choose BottomRight safely.
    //    return AdPosition.BottomRight;
    //}

    private void OnDestroy()
    {
        //bannerView?.Destroy();
        //rectBannerView?.Destroy();
        //interstitialAd?.Destroy();
        //rewardedAd?.Destroy();
        //rewardedInterstitialAd?.Destroy();
    }

    private void Log(string msg)
    {
        if (showDebugLogs) Debug.Log("[AdmobController] " + msg);
    }
}
*/