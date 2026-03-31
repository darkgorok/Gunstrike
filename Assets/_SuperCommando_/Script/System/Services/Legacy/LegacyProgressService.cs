using UnityEngine;

public sealed class LegacyProgressService : IProgressService
{
    private const string RemoveAdsKey = "RemoveAds";
    private const string IsSetDefaultValueKey = "isSetDefaultValue";
    private const string IsSoundEnabledKey = "isSoundEnabled";
    private const string IsMusicEnabledKey = "isMusicEnabled";
    private const string AttemptsKey = "Attempt";
    private const string SaveLivesKey = "SaveLives";
    private const string CoinsKey = "Coins";
    private const string WorldReachedKey = "WorldReached";

    private readonly IKeyValueStore keyValueStore;
    private readonly IInventoryService inventoryService;
    private readonly IDefaultGameConfigService defaultGameConfigService;
    private bool isFirstOpenMainMenu = true;
    private int levelPlaying = -1;
    private int totalLevel = 1;

    public LegacyProgressService(IKeyValueStore keyValueStore, IInventoryService inventoryService, IDefaultGameConfigService defaultGameConfigService)
    {
        this.keyValueStore = keyValueStore;
        this.inventoryService = inventoryService;
        this.defaultGameConfigService = defaultGameConfigService;
    }

    public bool RemoveAds
    {
        get => keyValueStore.GetInt(RemoveAdsKey, 0) == 1;
        set => keyValueStore.SetInt(RemoveAdsKey, value ? 1 : 0);
    }

    public bool IsSetDefaultValue
    {
        get => keyValueStore.GetInt(IsSetDefaultValueKey, 0) == 1;
        set => keyValueStore.SetInt(IsSetDefaultValueKey, value ? 1 : 0);
    }

    public bool IsFirstOpenMainMenu
    {
        get => isFirstOpenMainMenu;
        set => isFirstOpenMainMenu = value;
    }

    public bool IsSoundEnabled
    {
        get => keyValueStore.GetInt(IsSoundEnabledKey, 1) == 1;
        set => keyValueStore.SetInt(IsSoundEnabledKey, value ? 1 : 0);
    }

    public bool IsMusicEnabled
    {
        get => keyValueStore.GetInt(IsMusicEnabledKey, 1) == 1;
        set => keyValueStore.SetInt(IsMusicEnabledKey, value ? 1 : 0);
    }

    public int SavedCoins
    {
        get => keyValueStore.GetInt(CoinsKey, defaultGameConfigService.DefaultCoin);
        set => keyValueStore.SetInt(CoinsKey, value);
    }

    public int Attempts
    {
        get => keyValueStore.GetInt(AttemptsKey, 0);
        set => keyValueStore.SetInt(AttemptsKey, value);
    }

    public int SaveLives
    {
        get => keyValueStore.GetInt(SaveLivesKey, 6);
        set => keyValueStore.SetInt(SaveLivesKey, Mathf.Max(value, 0));
    }

    public int Bullets
    {
        get
        {
            int bullets = keyValueStore.GetInt("Bullets", 3);
            return Mathf.Clamp(bullets, 0, inventoryService.MaxDarts);
        }
        set => keyValueStore.SetInt("Bullets", value);
    }

    public int LevelHighest
    {
        get => keyValueStore.GetInt("LevelHighest", 1);
        set => keyValueStore.SetInt("LevelHighest", value);
    }

    public int LevelPlaying
    {
        get => levelPlaying;
        set => levelPlaying = value;
    }

    public int TotalLevel
    {
        get => totalLevel;
        set => totalLevel = value;
    }

    public int WorldReached
    {
        get => keyValueStore.GetInt(WorldReachedKey, 1);
        set => keyValueStore.SetInt(WorldReachedKey, value);
    }

    public void ResetLives()
    {
        SaveLives = 6;
        inventoryService.CurrentGunType = null;
    }

    public void ResetAllPreservingRemoveAds()
    {
        bool removeAds = RemoveAds;
        keyValueStore.DeleteAll();
        RemoveAds = removeAds;
    }

    public void UnlockAllLevels(int maxWorlds = 100, int levelHighest = 9999)
    {
        WorldReached = int.MaxValue;
        for (int i = 1; i < maxWorlds; i++)
        {
            keyValueStore.SetInt(i.ToString(), 10000);
        }

        LevelHighest = levelHighest;
    }
}
