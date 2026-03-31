public interface IProgressService
{
    bool RemoveAds { get; set; }
    bool IsSetDefaultValue { get; set; }
    bool IsFirstOpenMainMenu { get; set; }
    bool IsSoundEnabled { get; set; }
    bool IsMusicEnabled { get; set; }

    int Attempts { get; set; }
    int SavedCoins { get; set; }
    int SaveLives { get; set; }
    int Bullets { get; set; }
    int LevelHighest { get; set; }
    int LevelPlaying { get; set; }
    int TotalLevel { get; set; }
    int WorldReached { get; set; }

    void ResetLives();
    void ResetAllPreservingRemoveAds();
    void UnlockAllLevels(int maxWorlds = 100, int levelHighest = 9999);
}
