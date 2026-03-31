using UnityEngine;

public interface IGameSessionRuntimeState
{
    GameManager.GameState State { get; set; }
    bool IsWatchingAd { get; set; }
    bool IsSpecialBullet { get; set; }
    bool HasKey { get; set; }
    bool IsNoLives { get; set; }
    bool HideGui { get; set; }
    Vector2 CurrentCheckpoint { get; set; }
    int MissionStarCollected { get; set; }
    int Point { get; set; }
    int Coin { get; set; }
}
