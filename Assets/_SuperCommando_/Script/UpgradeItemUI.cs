using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

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

    private int nextUpgradeLevel;
    private IProgressService progressService;
    private IAudioService audioService;
    private IUpgradeService upgradeService;

    [Inject]
    public void Construct(IProgressService progressService, IAudioService audioService, IUpgradeService upgradeService)
    {
        this.progressService = progressService;
        this.audioService = audioService;
        this.upgradeService = upgradeService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        maxUpgrade = itemUpgrade.Length;
        nameTxt.text = itemName;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        string upgradeKey = upgradeType.ToString();
        float upgradePower = upgradeService.GetUpgradePower(upgradeKey);
        nextUpgradeLevel = upgradeService.GetUpgradeLevel(upgradeKey);

        extraTxt.text = (upgradeType == UPGRADE_ITEM_TYPE.doggeRecharge ? "-" : "+") + (int)upgradePower;

        if (nextUpgradeLevel >= maxUpgrade)
        {
            coinTxt.text = "MAX";
            upgradeButton.interactable = false;
            upgradeButton.gameObject.SetActive(false);
            SetDots(maxUpgrade);
            return;
        }

        coinPrice = itemUpgrade[nextUpgradeLevel].price;
        coinTxt.text = coinPrice.ToString();
        SetDots(nextUpgradeLevel);
    }

    private void SetDots(int number)
    {
        for (int i = 0; i < upgradeDots.Length; i++)
        {
            if (i < number)
                upgradeDots[i].sprite = dotImageOn;
            else if (i < maxUpgrade)
                upgradeDots[i].sprite = dotImageOff;
            else
                upgradeDots[i].enabled = false;
        }
    }

    public void Upgrade()
    {
        if (progressService.SavedCoins >= coinPrice)
        {
            audioService.PlaySfx(audioService.UpgradeClip);
            progressService.SavedCoins -= coinPrice;

            string upgradeKey = upgradeType.ToString();
            upgradeService.SetUpgradePower(upgradeKey, itemUpgrade[upgradeService.GetUpgradeLevel(upgradeKey)].power);
            nextUpgradeLevel++;
            upgradeService.SetUpgradeLevel(upgradeKey, nextUpgradeLevel);
            UpdateStatus();
            return;
        }

        audioService.PlaySfx(audioService.NotEnoughCoinClip);
        // if (GameMode.Instance && GameMode.Instance.IsRewardedReady())
        //     NotEnoughCoins.Instance.ShowUp();
    }
}
