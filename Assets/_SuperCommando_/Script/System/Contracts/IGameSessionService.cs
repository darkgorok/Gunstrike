using UnityEngine;

public interface IGameSessionService
{
    GameManager.GameState State { get; set; }
    Player Player { get; }
    LayerMask PlayerLayer { get; }
    LayerMask EnemyLayer { get; }
    LayerMask GroundLayer { get; }
    bool IsWatchingAd { get; set; }
    bool IsSpecialBullet { get; set; }
    bool HasKey { get; set; }
    bool HideGui { get; set; }
    bool IsNoLives { get; set; }
    Vector2 CurrentCheckpoint { get; }
    int ContinueCoinCost { get; }
    int MissionStarCollected { get; set; }
    int Point { get; set; }
    int Coin { get; set; }

    bool CanContinueWithCoins();
    void AddPoint(int amount);
    void AddBullet(int amount);
    void PauseCamera(bool pause);
    void SaveCheckpoint(Vector2 checkpoint);
    void StartGame();
    void GameFinish(int delay = 0);
    void GameOver(bool forceGameOver = false);
    void ContinueGame();
    void ResetLevel();
    void UnlockLevel();
}
