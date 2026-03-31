using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using VContainer;

public class Menu_AskSaveMe : MonoBehaviour
{
    public Text timerTxt;
    public Image timerImage;

    public Button btnWatchVideoAd;

    private const float TimerDuration = 3f;
    private const float TimeStep = 0.02f;

    private float timerCountDown;
    private IAdsService adsService;
    private IProgressService progressService;
    private IAudioService audioService;
    private IGameSessionService gameSession;
    private IMenuFlowService menuFlowService;
    private ILevelMapSettingsService levelMapSettingsService;

    [Inject]
    public void Construct(IAdsService adsService, IProgressService progressService, IAudioService audioService, IGameSessionService gameSession, IMenuFlowService menuFlowService, ILevelMapSettingsService levelMapSettingsService)
    {
        this.adsService = adsService;
        this.progressService = progressService;
        this.audioService = audioService;
        this.gameSession = gameSession;
        this.menuFlowService = menuFlowService;
        this.levelMapSettingsService = levelMapSettingsService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void OnEnable()
    {
        if (progressService.SaveLives > 0 || levelMapSettingsService.PlayerNoLimitLife)
        {
            progressService.SaveLives--;
            Continue();
            return;
        }

        Time.timeScale = 0f;
#if UNITY_ANDROID || UNITY_IOS
        // btnWatchVideoAd.interactable = AdsManager.Instance && AdsManager.Instance.IsRewardedReady();
#else
        btnWatchVideoAd.interactable = false;
        btnWatchVideoAd.gameObject.SetActive(false);
#endif

        timerCountDown = btnWatchVideoAd.interactable ? TimerDuration : 0f;
    }

    private void Update()
    {
        if (gameSession.IsWatchingAd)
            return;

        timerCountDown -= TimeStep;
        timerTxt.text = ((int)timerCountDown).ToString();
        timerImage.fillAmount = Mathf.Clamp01(timerCountDown / TimerDuration);

        if (timerCountDown > 0f)
            return;

        adsService.ShowBanner(true);

        gameSession.GameOver(true);
        Time.timeScale = 1f;
        menuFlowService.OpenSaveMe(false);
        Destroy(this);
    }

    public void SaveByCoin()
    {
        audioService.PlayClick();
        progressService.SavedCoins -= gameSession.ContinueCoinCost;
        Continue();
    }

    public void WatchVideoAd()
    {
        audioService.PlayClick();
        if (!adsService.CanShowRewarded)
            return;

        adsService.ShowRewardedVideo(AdsManager_AdResult);
        adsService.ResetRewardCounters();
    }

    private void AdsManager_AdResult(bool isSuccess, int rewarded)
    {
        if (!isSuccess)
            return;

        progressService.SaveLives += 1;
        Continue();
    }

    private void Continue()
    {
        Time.timeScale = 1f;
        gameSession.ContinueGame();
    }
}
