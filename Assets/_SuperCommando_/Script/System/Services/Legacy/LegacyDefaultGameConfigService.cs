using UnityEngine;

public sealed class LegacyDefaultGameConfigService : IDefaultGameConfigService
{
    private DefaultValue cachedDefaultValue;

    private DefaultValue Current
    {
        get
        {
            if (cachedDefaultValue == null)
                cachedDefaultValue = Object.FindFirstObjectByType<DefaultValue>();

            return cachedDefaultValue;
        }
    }

    public int DefaultLives => Current != null ? Current.defaultLives : 3;
    public int DefaultCoin => Current != null ? Current.defaultCoin : 99999;
    public bool DefaultBulletMax => Current != null && Current.defaultBulletMax;
    public int DefaultBullet => Current != null ? Current.defaultBullet : 0;
    public bool HasDefaults => Current != null;
}
