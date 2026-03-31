public interface IDefaultGameConfigService
{
    int DefaultLives { get; }
    int DefaultCoin { get; }
    bool DefaultBulletMax { get; }
    int DefaultBullet { get; }
    bool HasDefaults { get; }
}
