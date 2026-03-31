using UnityEngine;
using System.Collections;

public enum BulletFeature { Normal, Explosion, Shocking }

public class GlobalValue : MonoBehaviour
{
    public static GunTypeID currentGunTypeID;
    public static Transform currentHighestLevelObj;
    public static bool isFirstOpenMainMenu = true;
    public static int levelPlaying = -1;
    public static int totalLevel = 1;
    public static string WorldReached = "WorldReached";
    public static string Coins = "Coins";
    public static string Character = "Character";
    public static string ChoosenCharacterID = "choosenCharacterID";
    public static string ChoosenCharacterInstanceID = "ChoosenCharacterInstanceID";
    public static GameObject CharacterPrefab;
    public static int normalBulletLimited = 6;

    public static bool isSound = true;
    public static bool isMusic = true;

    public static int getDartLimited()
    {
       return normalBulletLimited + (int)UpgradeItemPower(UPGRADE_ITEM_TYPE.dartHoler.ToString());
    }
    
        public static bool isOpenRateButton
    {
        get { return PlayerPrefs.GetInt("isOpenRateButton", 0) == 1 ? true : false; }
        set { PlayerPrefs.SetInt("isOpenRateButton", value ? 1 : 0); }
    }

    public static int lastDayShowNativeAd1
    {
        get { return PlayerPrefs.GetInt("lastDayShowNativeAd1", 0); }
        set { PlayerPrefs.SetInt("lastDayShowNativeAd1", value); }
    }

    public static int lastDayShowNativeAd2
    {
        get { return PlayerPrefs.GetInt("lastDayShowNativeAd2", 0); }
        set { PlayerPrefs.SetInt("lastDayShowNativeAd2", value); }
    }

    public static int lastDayShowNativeAd3
    {
        get { return PlayerPrefs.GetInt("lastDayShowNativeAd3", 0); }
        set { PlayerPrefs.SetInt("lastDayShowNativeAd3", value); }
    }

    public static bool RemoveAds
    {
        get { return PlayerPrefs.GetInt("RemoveAds", 0) == 1 ? true : false; }
        set { PlayerPrefs.SetInt("RemoveAds", value ? 1 : 0); }
    }

    public static bool isSetDefaultValue
    {
        get { return PlayerPrefs.GetInt("isSetDefaultValue", 0) == 1 ? true : false; }
        set { PlayerPrefs.SetInt("isSetDefaultValue", value ? 1 : 0); }
    }


    public static int Attempt
    {
        get { return PlayerPrefs.GetInt("Attempt", 0); }
        set { PlayerPrefs.SetInt("Attempt", value); }
    }

    public static int SaveLives
    {
        get { return PlayerPrefs.GetInt("SaveLives", 6); }
        set
        {
            int i = Mathf.Max(value, 0);
            PlayerPrefs.SetInt("SaveLives", i);
        }
    }

    public static void ResetLives()
    {
        PlayerPrefs.SetInt("SaveLives", 6);
        currentGunTypeID = null;
    }

    public static int SavedCoins
    {
        get { return PlayerPrefs.GetInt(GlobalValue.Coins, DefaultValue.Instance != null ? DefaultValue.Instance.defaultCoin : 99999); }
        set { PlayerPrefs.SetInt(GlobalValue.Coins, value); }
    }

    public static int powerBullet
    {
        get { return PlayerPrefs.GetInt("powerBullet", 0); }
        set { PlayerPrefs.SetInt("powerBullet", value); }
    }

    public static int Bullets
    {
        get {
            int bullets = PlayerPrefs.GetInt("Bullets", 3);
            bullets = Mathf.Clamp(bullets, 0, getDartLimited());
            return bullets; }
        set { PlayerPrefs.SetInt("Bullets", value); }
    }

    public static int storeGod
    {
        get { return PlayerPrefs.GetInt("storeGod", 0); }
        set { PlayerPrefs.SetInt("storeGod", value); }
    }

    public static void SetScrollLevelAte(int scrollID)
    {
        //Debug.LogError("EAT: " + scrollID);
        PlayerPrefs.SetInt("AteScroll" + levelPlaying + scrollID, 1);
    }

    public static bool IsScrollLevelAte(int scrollID)
    {
        //Debug.LogError(scrollID + ":" + (PlayerPrefs.GetInt("AteScroll" + levelPlaying + scrollID, 0) == 1));
        return PlayerPrefs.GetInt("AteScroll" + levelPlaying + scrollID, 0) == 1 ? true : false;
    }

    public static bool IsScrollLevelAte(int level, int scrollID)
    {
        return PlayerPrefs.GetInt("AteScroll" + level + scrollID, 0) == 1 ? true : false;
    }


    public static int Scroll
    {
        get { return PlayerPrefs.GetInt("Scroll", 0); }
        set { PlayerPrefs.SetInt("Scroll", value); }
    }

    public static int LevelHighest
    {
        get { return PlayerPrefs.GetInt("LevelHighest", 1); }
        set { PlayerPrefs.SetInt("LevelHighest", value); }
    }

    public static void UpgradedItem(string name, int value)
    {
        PlayerPrefs.SetInt(name, value);
    }

    public static int UpgradedItem(string name)
    {
        return PlayerPrefs.GetInt(name, 0);
    }


    public static void UpgradeItemPower(string name, float value)
    {
        PlayerPrefs.SetFloat(name + "UpgradeItemPower", value);
    }

    public static float UpgradeItemPower(string name)
    {
        return PlayerPrefs.GetFloat(name + "UpgradeItemPower", 0f);
    }

    public static bool isPicked(GunTypeID gunID)
    {
        return PlayerPrefs.GetString("GUNTYPE" + gunID.gunType, "") == gunID.gunID;
    }

    public static void pickGun(GunTypeID gunID)
    {
        PlayerPrefs.SetString("GUNTYPE" + gunID.gunType, gunID.gunID);
    }
}
