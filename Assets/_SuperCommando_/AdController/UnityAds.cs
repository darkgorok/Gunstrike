using UnityEngine;
using UnityEngine.Advertisements;
using System;
using System.Collections;

public enum WatchAdResult { Finished, Failed, Skipped }
/*
public class UnityAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener, IUnityAdsInitializationListener
{
    public static UnityAds Instance;

    [Header("Unity Ads Setup")]
    [Tooltip("Unity Game ID (auto-detected at runtime)")]
    public string androidGameId = "YOUR_ANDROID_GAME_ID";
    public string iosGameId = "YOUR_IOS_GAME_ID";
    public bool testMode = true;

    [Header("Placement IDs")]
    public string interstitialAdId = "Interstitial_Android";
    public string rewardedAdId = "Rewarded_Android";

    private string _gameId;
    private bool _interstitialLoaded;
    private bool _rewardedLoaded;

    public delegate void RewardedAdResult(WatchAdResult result);
    public static event RewardedAdResult AdResult;

    #region UNITY LIFECYCLE
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAds();
    }
    #endregion

    #region INITIALIZATION
    private void InitializeAds()
    {
#if UNITY_ANDROID
        _gameId = androidGameId;
#elif UNITY_IOS
        _gameId = iosGameId;
#else
        _gameId = null;
#endif
        if (string.IsNullOrEmpty(_gameId))
        {
            Debug.LogError("Unity Ads Game ID is missing!");
            return;
        }

        Debug.Log($"Initializing Unity Ads with Game ID: {_gameId}");
        Advertisement.Initialize(_gameId, testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialized successfully.");
        LoadInterstitial();
        LoadRewarded();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads Initialization Failed: {error} - {message}");
    }
    #endregion

    #region LOAD ADS
    public void LoadInterstitial()
    {
        Debug.Log("Loading Interstitial Ad...");
        Advertisement.Load(interstitialAdId, this);
    }

    public void LoadRewarded()
    {
        Debug.Log("Loading Rewarded Ad...");
        Advertisement.Load(rewardedAdId, this);
    }
    #endregion

    #region SHOW ADS
    public void ShowInterstitial()
    {
        if (_interstitialLoaded)
        {
            Debug.Log("Showing Interstitial...");
            Advertisement.Show(interstitialAdId, this);
        }
        else
        {
            Debug.Log("Interstitial not ready, loading again...");
            LoadInterstitial();
        }
    }

    public void ShowRewarded()
    {
        if (_rewardedLoaded)
        {
            Debug.Log("Showing Rewarded Ad...");
            Advertisement.Show(rewardedAdId, this);
        }
        else
        {
            Debug.Log("Rewarded not ready, loading again...");
            LoadRewarded();
        }
    }
    #endregion

    #region IUnityAdsLoadListener
    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId.Equals(interstitialAdId))
        {
            _interstitialLoaded = true;
            Debug.Log("Interstitial Ad Loaded.");
        }
        else if (placementId.Equals(rewardedAdId))
        {
            _rewardedLoaded = true;
            Debug.Log("Rewarded Ad Loaded.");
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Failed to load Ad {placementId}: {error} - {message}");
        StartCoroutine(RetryLoadAd(placementId));
    }

    private IEnumerator RetryLoadAd(string placementId)
    {
        yield return new WaitForSeconds(3);
        if (placementId == interstitialAdId)
            LoadInterstitial();
        else if (placementId == rewardedAdId)
            LoadRewarded();
    }
    #endregion

    #region IUnityAdsShowListener
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Ad Show Failed for {placementId}: {error} - {message}");
        if (placementId == rewardedAdId)
            AdResult?.Invoke(WatchAdResult.Failed);
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log($"Ad Started: {placementId}");
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log($"Ad Clicked: {placementId}");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"Ad Completed: {placementId} - State: {showCompletionState}");

        if (placementId == rewardedAdId)
        {
            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                AdResult?.Invoke(WatchAdResult.Finished);
                Debug.Log("Reward Granted!");
            }
            else
            {
                AdResult?.Invoke(WatchAdResult.Skipped);
                Debug.Log("Reward Skipped.");
            }

            _rewardedLoaded = false;
            LoadRewarded();
        }

        if (placementId == interstitialAdId)
        {
            _interstitialLoaded = false;
            LoadInterstitial();
        }
    }
    #endregion
}
*/