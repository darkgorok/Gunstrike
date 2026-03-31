using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ShopItemUI : MonoBehaviour
{
    public enum ITEM_TYPE { buyDart, iap1, iap2, iap3, watchVideo, buyLive }

    public ITEM_TYPE itemType;
    public int rewarded = 100;
    public float price = 100;
    public GameObject watchVideocontainer;
    public AudioClip soundRewarded;
    public Text priceTxt, rewardedTxt, rewardTimeCountDownTxt;

    private IAdsService adsService;
    private IProgressService progressService;
    private IInventoryService inventoryService;
    private IAudioService audioService;

    [Inject]
    public void Construct(IAdsService adsService, IProgressService progressService, IInventoryService inventoryService, IAudioService audioService)
    {
        this.adsService = adsService;
        this.progressService = progressService;
        this.inventoryService = inventoryService;
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Update()
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (itemType == ITEM_TYPE.buyLive)
        {
            priceTxt.text = price.ToString();
            rewardedTxt.text = progressService.SaveLives.ToString();
        }
        else if (itemType == ITEM_TYPE.buyDart)
        {
            priceTxt.text = price.ToString();
            rewardedTxt.text = inventoryService.Darts + "/" + inventoryService.MaxDarts;
        }
        else if (itemType == ITEM_TYPE.watchVideo)
        {
            priceTxt.text = "FREE";
            rewardedTxt.text = "+" + rewarded;

            if (watchVideocontainer != null)
            {
                if (adsService.IsInitialized && adsService.TimeUntilNextReward() > 0)
                {
                    watchVideocontainer.SetActive(false);
                    rewardTimeCountDownTxt.text =
                        ((int)adsService.TimeUntilNextReward() / 60).ToString("0") + ":" +
                        ((int)adsService.TimeUntilNextReward() % 60).ToString("00");
                }
                else if (rewardTimeCountDownTxt)
                {
                    rewardTimeCountDownTxt.text = "No Ads";
                }
            }
        }
        else
        {
            priceTxt.text = "$" + price;
            rewardedTxt.text = "+" + rewarded;
        }
    }

    public void Buy()
    {
#if UNITY_PURCHASING
        switch (itemType)
        {
            case ITEM_TYPE.buyLive:
                if (progressService.SavedCoins >= price && progressService.SaveLives < 100)
                {
                    progressService.SavedCoins -= (int)price;
                    progressService.SaveLives += rewarded;
                    audioService.PlaySfx(soundRewarded != null ? soundRewarded : audioService.PurchasedClip);
                }
                else
                {
                    audioService.PlaySfx(audioService.NotEnoughCoinClip);
                }
                break;

            case ITEM_TYPE.buyDart:
                if (progressService.SavedCoins >= price && inventoryService.Darts < inventoryService.MaxDarts)
                {
                    progressService.SavedCoins -= (int)price;
                    inventoryService.Darts += rewarded;
                    audioService.PlaySfx(soundRewarded != null ? soundRewarded : audioService.PurchasedClip);
                }
                else
                {
                    audioService.PlaySfx(audioService.NotEnoughCoinClip);
                }
                break;

            case ITEM_TYPE.watchVideo:
                if (adsService.CanShowRewarded)
                    adsService.ShowRewardedVideo(AdsManager_AdResult);
                break;

            case ITEM_TYPE.iap1:
                if (Purchaser.Instance)
                {
                    Purchaser.iAPResult += Purchaser_iAPResult;
                    Purchaser.Instance.BuyItem1();
                }
                break;

            case ITEM_TYPE.iap2:
                if (Purchaser.Instance)
                {
                    Purchaser.iAPResult += Purchaser_iAPResult;
                    Purchaser.Instance.BuyItem2();
                }
                break;

            case ITEM_TYPE.iap3:
                if (Purchaser.Instance)
                {
                    Purchaser.iAPResult += Purchaser_iAPResult;
                    Purchaser.Instance.BuyItem3();
                }
                break;
        }
#endif
    }

    private void AdsManager_AdResult(bool isSuccess, int rewarded)
    {
        if (!isSuccess)
            return;

        progressService.SavedCoins += rewarded;
        audioService.PlaySfx(soundRewarded != null ? soundRewarded : audioService.PurchasedClip);
        UpdateStatus();
    }

    private void Purchaser_iAPResult(int id)
    {
#if UNITY_PURCHASING
        Purchaser.iAPResult -= Purchaser_iAPResult;
        progressService.SavedCoins += rewarded;
        audioService.PlaySfx(soundRewarded != null ? soundRewarded : audioService.PurchasedClip);
        UpdateStatus();
#endif
    }
}
