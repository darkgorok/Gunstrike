using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UPGRADE_ITEM_TYPE { sword, dart, dartHoler, shield, doggeRecharge }
[System.Serializable]
public class UpgradeValue
{
    public int price = 100;
    public float power = 1;
}
public class UpgradeItemUI : MonoBehaviour
{
    public UPGRADE_ITEM_TYPE upgradeType;

    public string itemName = "ITEM NAME";
   [ReadOnly] public int maxUpgrade;
    public UpgradeValue[] itemUpgrade;
    public Image[] upgradeDots;
    public Sprite dotImageOn, dotImageOff;
    public Text nameTxt;
    public Text extraTxt;
    [ReadOnly] public int coinPrice = 1;
    public Text coinTxt;
    public Button upgradeButton;
    int nextUpgradeLevel;

    void Start()
    {
        maxUpgrade = itemUpgrade.Length;
        nameTxt.text = itemName;
       
        UpdateStatus();
    }

    void UpdateStatus()
    {
        if (upgradeType == UPGRADE_ITEM_TYPE.doggeRecharge)
            extraTxt.text = "-" + (int)GlobalValue.UpgradeItemPower(upgradeType.ToString());
        else
            extraTxt.text = "+" + (int)GlobalValue.UpgradeItemPower(upgradeType.ToString());
        nextUpgradeLevel = GlobalValue.UpgradedItem(upgradeType.ToString());
        
        if (nextUpgradeLevel >= maxUpgrade)
        {
            coinTxt.text = "MAX";
            upgradeButton.interactable = false;
            //upgradeButton.GetComponent<Image>().enabled = false;                            
            upgradeButton.gameObject.SetActive(false);
            SetDots(maxUpgrade);
        }
        else
        {
            coinPrice = itemUpgrade[GlobalValue.UpgradedItem(upgradeType.ToString())].price;
            coinTxt.text = coinPrice + "";
            SetDots(nextUpgradeLevel);
        }
    }

    void SetDots(int number)
    {
        for (int i = 0; i < upgradeDots.Length; i++)
        {
            if (i < number)
                upgradeDots[i].sprite = dotImageOn;
            else if(i < maxUpgrade)
                upgradeDots[i].sprite = dotImageOff;
            else
            {
                upgradeDots[i].enabled = false;
            }


            //if (i >= maxUpgrade)
            //    upgradeDots[i].gameObject.SetActive(false);
        }
    }

    public void Upgrade()
    {
        if (GlobalValue.SavedCoins >= coinPrice)
        {
            SoundManager.PlaySfx(SoundManager.Instance.soundUpgrade);
            GlobalValue.SavedCoins -= coinPrice;
            
            GlobalValue.UpgradeItemPower(upgradeType.ToString(), itemUpgrade[GlobalValue.UpgradedItem(upgradeType.ToString())].power);
            nextUpgradeLevel++;
            GlobalValue.UpgradedItem(upgradeType.ToString(), nextUpgradeLevel);
            UpdateStatus();
        }
        else
        {
            SoundManager.PlaySfx(SoundManager.Instance.soundNotEnoughCoin);
            //if (GameMode.Instance && GameMode.Instance.IsRewardedReady())
            //    NotEnoughCoins.Instance.ShowUp();
        }
    }
}
