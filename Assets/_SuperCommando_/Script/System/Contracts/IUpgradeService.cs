public interface IUpgradeService
{
    int GetUpgradeLevel(string name);
    void SetUpgradeLevel(string name, int level);

    float GetUpgradePower(string name);
    void SetUpgradePower(string name, float power);
}
