using UnityEngine;

public sealed class LegacyLevelMapSettingsService : ILevelMapSettingsService
{
    private LevelMapType cachedLevelMapType;

    private LevelMapType Current
    {
        get
        {
            if (cachedLevelMapType == null)
                cachedLevelMapType = Object.FindFirstObjectByType<LevelMapType>();

            return cachedLevelMapType;
        }
    }

    public bool PlayerNoLimitLife => Current != null && Current.playerNoLimitLife;
}
