using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class Menu_AskSaveMe : MonoBehaviour
{
    public Text timerTxt;
    public Image timerImage;

    float timer = 3;
    float timerCountDown = 0;

    public Button btnWatchVideoAd;

    float timeStep = 0.02f;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (GlobalValue.SaveLives > 0 || (LevelMapType.Instance && LevelMapType.Instance.playerNoLimitLife))
        {
            GlobalValue.SaveLives--;
            Continue();
        }
        else
        {
            Time.timeScale = 0;
#if UNITY_ANDROID || UNITY_IOS
            //  btnWatchVideoAd.interactable = AdsManager.Instance && AdsManager.Instance.IsRewardedReady();
#else
            btnWatchVideoAd.interactable = false;
            btnWatchVideoAd.gameObject.SetActive(false);
#endif

            if (!btnWatchVideoAd.interactable)
                timerCountDown = 0;
            else
                timerCountDown = timer;
        }
    }

    void Update()
    { 
        if (!GameManager.Instance.isWatchingAd)
        {
            timerCountDown -= timeStep;
            timerTxt.text = (int)timerCountDown + "" ;
            timerImage.fillAmount = Mathf.Clamp01(timerCountDown / timer);

            if (timerCountDown <= 0)
            {
                if(AdsManager.Instance)
                AdsManager.Instance.ShowBanner(true);
                GameManager.Instance.GameOver(true);
                Time.timeScale = 1;
                MenuManager.Instance.OpenSaveMe(false);
                Destroy(this);      //destroy this script
            }
        }
    }

    

    public void SaveByCoin()
    {
        SoundManager.Click();
        GlobalValue.SavedCoins -= GameManager.Instance.continueCoinCost;
        Continue();
    }

    public void WatchVideoAd()
    {
        SoundManager.Click();
        if (AdsManager.Instance)
        {
            AdsManager.OnRewardedResult += AdsManager_AdResult;
            AdsManager.Instance.ShowRewardedVideo();
            AdsManager.Instance.ResetCounters();
        } 
    }

    private void AdsManager_AdResult(bool isSuccess, int rewarded)
    {
        AdsManager.OnRewardedResult -= AdsManager_AdResult;
        if (isSuccess)
        {
            GlobalValue.SaveLives += 1;
            Continue();
        }
    }

    void Continue()
    {
        Time.timeScale = 1;
        GameManager.Instance.Continue();
    }
}
