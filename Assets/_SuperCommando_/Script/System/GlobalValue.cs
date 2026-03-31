using UnityEngine;
using System.Collections;

public enum BulletFeature { Normal, Explosion, Shocking }

public class GlobalValue : MonoBehaviour
{
    private static readonly IKeyValueStore FallbackStore = new UnityPrefsKeyValueStore();
    private const int BaseDartLimit = 6;

    public const string WorldReached = "WorldReached";
    public const string Coins = "Coins";
    public const string Character = "Character";
    public const string ChoosenCharacterID = "choosenCharacterID";
    public const string ChoosenCharacterInstanceID = "ChoosenCharacterInstanceID";

    private static IKeyValueStore Store => ProjectScope.IsInitialized ? ProjectScope.Resolve<IKeyValueStore>() : FallbackStore;

    private static int GetInt(string key, int defaultValue = 0)
    {
        return Store.GetInt(key, defaultValue);
    }

    private static void SetInt(string key, int value)
    {
        Store.SetInt(key, value);
    }

    private static float GetFloat(string key, float defaultValue = 0f)
    {
        return Store.GetFloat(key, defaultValue);
    }

    private static void SetFloat(string key, float value)
    {
        Store.SetFloat(key, value);
    }

    private static string GetString(string key, string defaultValue = "")
    {
        return Store.GetString(key, defaultValue);
    }

    private static void SetString(string key, string value)
    {
        Store.SetString(key, value);
    }

    public static int getDartLimited()
    {
       return BaseDartLimit + (int)UpgradeItemPower(UPGRADE_ITEM_TYPE.dartHoler.ToString());
    }
    
        public static bool isOpenRateButton
    {
        get { return GetInt("isOpenRateButton", 0) == 1 ? true : false; }
        set { SetInt("isOpenRateButton", value ? 1 : 0); }
    }

    public static int lastDayShowNativeAd1
    {
        get { return GetInt("lastDayShowNativeAd1", 0); }
        set { SetInt("lastDayShowNativeAd1", value); }
    }

    public static int lastDayShowNativeAd2
    {
        get { return GetInt("lastDayShowNativeAd2", 0); }
        set { SetInt("lastDayShowNativeAd2", value); }
    }

    public static int lastDayShowNativeAd3
    {
        get { return GetInt("lastDayShowNativeAd3", 0); }
        set { SetInt("lastDayShowNativeAd3", value); }
    }

    public static bool RemoveAds
    {
        get { return GetInt("RemoveAds", 0) == 1 ? true : false; }
        set { SetInt("RemoveAds", value ? 1 : 0); }
    }

    public static bool isSetDefaultValue
    {
        get { return GetInt("isSetDefaultValue", 0) == 1 ? true : false; }
        set { SetInt("isSetDefaultValue", value ? 1 : 0); }
    }


    public static int Attempt
    {
        get { return GetInt("Attempt", 0); }
        set { SetInt("Attempt", value); }
    }

    public static int SaveLives
    {
        get { return GetInt("SaveLives", 6); }
        set
        {
            int i = Mathf.Max(value, 0);
            SetInt("SaveLives", i);
        }
    }

    public static void ResetLives()
    {
        SetInt("SaveLives", 6);
    }

    public static int SavedCoins
    {
        get { return GetInt(Coins, DefaultValue.Instance != null ? DefaultValue.Instance.defaultCoin : 99999); }
        set { SetInt(Coins, value); }
    }

    public static int powerBullet
    {
        get { return GetInt("powerBullet", 0); }
        set { SetInt("powerBullet", value); }
    }

    public static int Bullets
    {
        get {
            int bullets = GetInt("Bullets", 3);
            bullets = Mathf.Clamp(bullets, 0, getDartLimited());
            return bullets; }
        set { SetInt("Bullets", value); }
    }

    public static int storeGod
    {
        get { return GetInt("storeGod", 0); }
        set { SetInt("storeGod", value); }
    }

    public static void SetScrollLevelAte(int scrollID)
    {
        SetInt(BuildScrollKey(scrollID), 1);
    }

    public static bool IsScrollLevelAte(int scrollID)
    {
        return GetInt(BuildScrollKey(scrollID), 0) == 1 ? true : false;
    }

    public static bool IsScrollLevelAte(int level, int scrollID)
    {
        return GetInt("AteScroll" + level + scrollID, 0) == 1 ? true : false;
    }


    public static int Scroll
    {
        get { return GetInt("Scroll", 0); }
        set { SetInt("Scroll", value); }
    }

    public static int LevelHighest
    {
        get { return GetInt("LevelHighest", 1); }
        set { SetInt("LevelHighest", value); }
    }

    public static void UpgradedItem(string name, int value)
    {
        SetInt(name, value);
    }

    public static int UpgradedItem(string name)
    {
        return GetInt(name, 0);
    }


    public static void UpgradeItemPower(string name, float value)
    {
        SetFloat(name + "UpgradeItemPower", value);
    }

    public static float UpgradeItemPower(string name)
    {
        return GetFloat(name + "UpgradeItemPower", 0f);
    }

    public static bool isPicked(GunTypeID gunID)
    {
        return GetString("GUNTYPE" + gunID.gunType, "") == gunID.gunID;
    }

    public static void pickGun(GunTypeID gunID)
    {
        SetString("GUNTYPE" + gunID.gunType, gunID.gunID);
    }

    private static string BuildScrollKey(int scrollId)
    {
        int currentLevel = ProjectScope.IsInitialized ? ProjectScope.Resolve<IProgressService>()?.LevelPlaying ?? -1 : -1;
        return "AteScroll" + currentLevel + scrollId;
    }
}
