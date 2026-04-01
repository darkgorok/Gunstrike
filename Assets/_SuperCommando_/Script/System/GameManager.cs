using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

public class GameManager : MonoBehaviour
{
    private enum SessionTransition
    {
        None,
        Finish,
        Continue
    }

    public enum GameState { Menu, Playing, Dead, Finish, Waiting }
    public GameState State
    {
        get => runtimeState.State;
        set => runtimeState.State = value;
    }

    public LayerMask playerLayer;
    public LayerMask enemyLayer;
    public LayerMask groundLayer;

    public bool isWatchingAd
    {
        get => runtimeState.IsWatchingAd;
        set => runtimeState.IsWatchingAd = value;
    }

    [Header("CONTINUE GAME OPTION")]
    public int continueCoinCost = 100;

    public bool canBeSave()
    {
        return progressService.SavedCoins >= continueCoinCost;
    }

    public GameObject FadeInEffect;

    [Header("Floating Text")]
    public GameObject FloatingText;

    public Vector2 currentCheckpoint
    {
        get => runtimeState.CurrentCheckpoint;
        private set => runtimeState.CurrentCheckpoint = value;
    }

    public bool isSpecialBullet
    {
        get => runtimeState.IsSpecialBullet;
        set => runtimeState.IsSpecialBullet = value;
    }

    public bool isHasKey
    {
        get => runtimeState.HasKey;
        set
        {
            if (value)
                keyPresentationService.ShowCollected();
            else
                keyPresentationService.ShowUsed();

            runtimeState.HasKey = value;
        }
    }

    public Player Player { get; private set; }

    public bool isNoLives
    {
        get => runtimeState.IsNoLives;
        set => runtimeState.IsNoLives = value;
    }
    public int MissionStarCollected
    {
        get => runtimeState.MissionStarCollected;
        set => runtimeState.MissionStarCollected = value;
    }
    public bool hideGUI
    {
        get => runtimeState.HideGui;
        set => runtimeState.HideGui = value;
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

    [SerializeField] private Player cachedPlayer;
    private SessionTransition pendingTransition = SessionTransition.None;
    private float pendingTransitionTimer = -1f;

    private ISceneLoader sceneLoader;
    private IAudioService audioService;
    private ICameraRigService cameraRigService;
    private IControllerInputService controllerInputService;
    private ICharacterSelectionService characterSelectionService;
    private IProgressService progressService;
    private IInventoryService inventoryService;
    private IGameSessionRuntimeState runtimeState;
    private IListenerBroadcastService listenerBroadcastService;
    private IGameplayPresentationService gameplayPresentationService;
    private IMenuFlowService menuFlowService;
    private IKeyPresentationService keyPresentationService;
    private IDefaultGameConfigService defaultGameConfigService;

    [Inject]
    public void Construct(
        ISceneLoader sceneLoader,
        IAudioService audioService,
        ICameraRigService cameraRigService,
        IControllerInputService controllerInputService,
        ICharacterSelectionService characterSelectionService,
        IProgressService progressService,
        IInventoryService inventoryService,
        IGameSessionRuntimeState runtimeState,
        IListenerBroadcastService listenerBroadcastService,
        IGameplayPresentationService gameplayPresentationService,
        IMenuFlowService menuFlowService,
        IKeyPresentationService keyPresentationService,
        IDefaultGameConfigService defaultGameConfigService)
    {
        this.sceneLoader = sceneLoader;
        this.audioService = audioService;
        this.cameraRigService = cameraRigService;
        this.controllerInputService = controllerInputService;
        this.characterSelectionService = characterSelectionService;
        this.progressService = progressService;
        this.inventoryService = inventoryService;
        this.runtimeState = runtimeState;
        this.listenerBroadcastService = listenerBroadcastService;
        this.gameplayPresentationService = gameplayPresentationService;
        this.menuFlowService = menuFlowService;
        this.keyPresentationService = keyPresentationService;
        this.defaultGameConfigService = defaultGameConfigService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        Application.targetFrameRate = 60;
        isSpecialBullet = false;
        State = GameState.Menu;
        cachedPlayer = ResolvePlayer();
        Player = cachedPlayer;

        var selectedCharacter = characterSelectionService.CurrentCharacterPrefab;
        if (selectedCharacter != null && Player != null)
        {
            Instantiate(selectedCharacter, Player.transform.position, Player.transform.rotation);
            Destroy(Player.gameObject);
            cachedPlayer = ResolvePlayer();
            Player = cachedPlayer;
        }

        GameObject startPoint = GameObject.FindGameObjectWithTag("StartPoint");
        if (startPoint != null && Player != null)
            Player.transform.position = startPoint.transform.position;
    }

    private void Start()
    {
        currentCheckpoint = Player.transform.position;
        audioService.PlaySfx(audioService.BeginMainMenuClip);
    }

    private Player ResolvePlayer()
    {
        return cachedPlayer != null ? cachedPlayer : Object.FindFirstObjectByType<Player>();
    }

    private void Update()
    {
        ShortKey();
        TickTransition();
    }

    public void AddPoint(int addpoint)
    {
        Point += addpoint;
    }

    public void AddBullet(int addbullet)
    {
        if (defaultGameConfigService.DefaultBulletMax)
        {
            Debug.LogWarning("NO LIMIT BULLET");
            return;
        }

        inventoryService.Darts += addbullet;
    }

    public void PauseCamera(bool pause)
    {
        cameraRigService.PauseCamera = pause;
        if (!pause)
            cameraRigService.MoveToPlayerPosition();
    }

    public void SaveCheckPoint(Vector2 newCheckpoint)
    {
        currentCheckpoint = newCheckpoint;
    }

    public void ShowFloatingText(string text, Vector2 positon, Color color)
    {
        GameObject floatingText = Instantiate(FloatingText);
        Vector3 screenPosition = cameraRigService.WorldToScreenPoint(positon);

        floatingText.transform.SetParent(menuFlowService.UiRoot, false);
        floatingText.transform.position = screenPosition;

        FloatingText floatingTextComponent = floatingText.GetComponent<FloatingText>();
        floatingTextComponent.SetText(text, color);
    }

    public void StartGame()
    {
        State = GameState.Playing;
        listenerBroadcastService.Notify(listener => listener.IPlay());
        audioService.PlayGameMusic();
    }

    public void GameFinish(int delay = 0)
    {
        if (State == GameState.Finish)
            return;

        State = GameState.Finish;
        listenerBroadcastService.Notify(listener => listener.ISuccess());

        pendingTransition = SessionTransition.Finish;
        pendingTransitionTimer = delay;
        if (delay <= 0)
            CompleteFinishTransition();
    }

    public void UnlockLevel()
    {
        if (progressService.LevelPlaying == progressService.LevelHighest)
        {
            progressService.LevelHighest++;
            Debug.LogWarning("Unlock new level");
        }
    }

    public void GameOver(bool forceGameOver = false)
    {
        controllerInputService.SetShotHeld(false);

        if (State != GameState.Dead && State != GameState.Waiting)
            progressService.Attempts++;

        if (State == GameState.Dead)
            return;

        if (!forceGameOver && canBeSave())
        {
            if (State == GameState.Dead || State == GameState.Waiting)
                return;

            State = GameState.Waiting;
            Player.Kill();
            controllerInputService.StopMove();
            gameplayPresentationService.SetGameplayUiVisible(false && !hideGUI);
            audioService.PauseMusic(true);
            menuFlowService.OpenSaveMe(true);
            return;
        }

        listenerBroadcastService.Notify(listener => listener.IGameOver());
        controllerInputService.StopMove();
        State = GameState.Dead;
        menuFlowService.ShowGameOver();
        audioService.PlaySfx(audioService.GameOverClip, 0.5f);
        audioService.PauseMusic(true);
        progressService.ResetLives();
    }

    public void Continue()
    {
        listenerBroadcastService.Notify(listener => listener.IOnRespawn());
        menuFlowService.OpenSaveMe(false);
        Player.RespawnAt(currentCheckpoint);
        pendingTransition = SessionTransition.Continue;
        pendingTransitionTimer = 1.5f;
    }

    public void ResetLevel()
    {
        menuFlowService.RestartGame();
    }

    private void ShortKey()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            sceneLoader.LoadImmediate(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            progressService.ResetAllPreservingRemoveAds();
            sceneLoader.LoadImmediate(0);
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
        {
            progressService.SaveLives += 999;
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
        {
            progressService.SavedCoins += 999999;
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        {
            CompleteFinishTransition();
        }

        if (Input.GetKey(KeyCode.U))
        {
            progressService.UnlockAllLevels();
            sceneLoader.LoadImmediateAsync(1);
            audioService.PlaySfx(audioService.ClickClip);
        }
    }

    private void TickTransition()
    {
        if (pendingTransition == SessionTransition.None || pendingTransitionTimer < 0f)
            return;

        pendingTransitionTimer -= Time.deltaTime;
        if (pendingTransitionTimer > 0f)
            return;

        switch (pendingTransition)
        {
            case SessionTransition.Finish:
                CompleteFinishTransition();
                break;
            case SessionTransition.Continue:
                CompleteContinueTransition();
                break;
        }
    }

    private void CompleteFinishTransition()
    {
        pendingTransition = SessionTransition.None;
        pendingTransitionTimer = -1f;
        audioService.PauseMusic(true);
        audioService.PlaySfx(audioService.GameFinishClip, 0.3f);
        menuFlowService.ShowGameFinish();
    }

    private void CompleteContinueTransition()
    {
        pendingTransition = SessionTransition.None;
        pendingTransitionTimer = -1f;
        State = GameState.Playing;
        gameplayPresentationService.SetGameplayUiVisible(!hideGUI);
        audioService.PauseMusic(false);
    }

}
