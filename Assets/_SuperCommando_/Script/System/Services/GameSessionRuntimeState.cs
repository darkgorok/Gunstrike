using UnityEngine;

public sealed class GameSessionRuntimeState : IGameSessionRuntimeState
{
    public GameManager.GameState State { get; set; } = GameManager.GameState.Menu;
    public bool IsWatchingAd { get; set; }
    public bool IsSpecialBullet { get; set; }
    public bool HasKey { get; set; }
    public bool IsNoLives { get; set; }
    public bool HideGui { get; set; }
    public Vector2 CurrentCheckpoint { get; set; } = Vector2.zero;
    public int MissionStarCollected { get; set; }
    public int Point { get; set; }
    public int Coin { get; set; }
}
