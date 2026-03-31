using UnityEngine;

public sealed class LegacyGameSessionService : IGameSessionService
{
    private readonly IGameSessionRuntimeState runtimeState;
    private GameManager cachedGameManager;

    public LegacyGameSessionService(IGameSessionRuntimeState runtimeState)
    {
        this.runtimeState = runtimeState;
    }

    private GameManager Current
    {
        get
        {
            if (cachedGameManager == null)
                cachedGameManager = Object.FindFirstObjectByType<GameManager>();

            if (cachedGameManager == null)
                Debug.LogError("GameSession requested before GameManager was initialized.");

            return cachedGameManager;
        }
    }

    public GameManager.GameState State
    {
        get => runtimeState.State;
        set => runtimeState.State = value;
    }

    public Player Player => Current.Player;
    public LayerMask PlayerLayer => Current.playerLayer;
    public LayerMask EnemyLayer => Current.enemyLayer;
    public LayerMask GroundLayer => Current.groundLayer;

    public bool IsWatchingAd
    {
        get => runtimeState.IsWatchingAd;
        set => runtimeState.IsWatchingAd = value;
    }

    public bool IsSpecialBullet
    {
        get => runtimeState.IsSpecialBullet;
        set => runtimeState.IsSpecialBullet = value;
    }

    public bool HasKey
    {
        get => runtimeState.HasKey;
        set => Current.isHasKey = value;
    }

    public bool HideGui
    {
        get => runtimeState.HideGui;
        set => runtimeState.HideGui = value;
    }

    public bool IsNoLives
    {
        get => runtimeState.IsNoLives;
        set => runtimeState.IsNoLives = value;
    }

    public Vector2 CurrentCheckpoint => runtimeState.CurrentCheckpoint;
    public int ContinueCoinCost => Current.continueCoinCost;

    public int MissionStarCollected
    {
        get => runtimeState.MissionStarCollected;
        set => runtimeState.MissionStarCollected = value;
    }

    public int Point
    {
        get => runtimeState.Point;
        set => runtimeState.Point = value;
    }

    public int Coin
    {
        get => runtimeState.Coin;
        set => runtimeState.Coin = value;
    }

    public bool CanContinueWithCoins()
    {
        return Current.canBeSave();
    }

    public void AddPoint(int amount)
    {
        Current.AddPoint(amount);
    }

    public void AddBullet(int amount)
    {
        Current.AddBullet(amount);
    }

    public void PauseCamera(bool pause)
    {
        Current.PauseCamera(pause);
    }

    public void SaveCheckpoint(Vector2 checkpoint)
    {
        Current.SaveCheckPoint(checkpoint);
    }

    public void StartGame()
    {
        Current.StartGame();
    }

    public void GameFinish(int delay = 0)
    {
        Current.GameFinish(delay);
    }

    public void GameOver(bool forceGameOver = false)
    {
        Current.GameOver(forceGameOver);
    }

    public void ContinueGame()
    {
        Current.Continue();
    }

    public void ResetLevel()
    {
        Current.ResetLevel();
    }

    public void UnlockLevel()
    {
        Current.UnlockLevel();
    }
}
