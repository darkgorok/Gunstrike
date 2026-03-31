public sealed class LegacyUpgradeService : IUpgradeService
{
    private readonly IKeyValueStore keyValueStore;

    public LegacyUpgradeService(IKeyValueStore keyValueStore)
    {
        this.keyValueStore = keyValueStore;
    }

    public int GetUpgradeLevel(string name)
    {
        return keyValueStore.GetInt(name, 0);
    }

    public void SetUpgradeLevel(string name, int level)
    {
        keyValueStore.SetInt(name, level);
    }

    public float GetUpgradePower(string name)
    {
        return keyValueStore.GetFloat(name + "UpgradeItemPower", 0f);
    }

    public void SetUpgradePower(string name, float power)
    {
        keyValueStore.SetFloat(name + "UpgradeItemPower", power);
    }
}
