public sealed class LegacyInventoryService : IInventoryService
{
    private const int DefaultDarts = 6;

    private readonly IKeyValueStore keyValueStore;
    private readonly IUpgradeService upgradeService;
    private GunTypeID currentGunType;

    public LegacyInventoryService(IKeyValueStore keyValueStore, IUpgradeService upgradeService)
    {
        this.keyValueStore = keyValueStore;
        this.upgradeService = upgradeService;
    }

    public int Darts
    {
        get
        {
            int bullets = keyValueStore.GetInt("Bullets", 3);
            return UnityEngine.Mathf.Clamp(bullets, 0, MaxDarts);
        }
        set => keyValueStore.SetInt("Bullets", value);
    }

    public int MaxDarts => DefaultDarts + (int)upgradeService.GetUpgradePower(UPGRADE_ITEM_TYPE.dartHoler.ToString());

    public int PowerBullets
    {
        get => keyValueStore.GetInt("powerBullet", 0);
        set => keyValueStore.SetInt("powerBullet", value);
    }

    public int StoredGodItems
    {
        get => keyValueStore.GetInt("storeGod", 0);
        set => keyValueStore.SetInt("storeGod", value);
    }

    public GunTypeID CurrentGunType
    {
        get => currentGunType;
        set => currentGunType = value;
    }

    public bool IsGunPicked(GunTypeID gunId)
    {
        return keyValueStore.GetString("GUNTYPE" + gunId.gunType, "") == gunId.gunID;
    }

    public void PickGun(GunTypeID gunId)
    {
        keyValueStore.SetString("GUNTYPE" + gunId.gunType, gunId.gunID);
    }
}
