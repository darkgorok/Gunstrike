using UnityEngine;
using VContainer;

public class ControllerInput : MonoBehaviour, IListener
{
    public GameObject rangeAttack;
    Player Player;

    [Header("Button")]
    public GameObject btnJump;
    public GameObject btnRange;

    public float Vertical, Horizontak;
    public Vector2 MoveInput
    {
        get => new Vector2(Horizontak, Vertical);
        set
        {
            Horizontak = value.x;
            Vertical = value.y;
        }
    }

    private IGameSessionService gameSession;
    private IMenuFlowService menuFlowService;
    private IKeyboardBindingService keyboardBindingService;
    bool shooting;
    bool isMovingLeft, isMovingRight;

    [Inject]
    public void Construct(IGameSessionService gameSession, IMenuFlowService menuFlowService, IKeyboardBindingService keyboardBindingService)
    {
        this.gameSession = gameSession;
        this.menuFlowService = menuFlowService;
        this.keyboardBindingService = keyboardBindingService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void OnEnable()
    {
        if (gameSession != null)
            StopMove();
    }

    private Player CurrentPlayer
    {
        get
        {
            if (Player == null && gameSession != null)
                Player = gameSession.Player;

            return Player;
        }
    }

    void Start()
    {
        if (CurrentPlayer == null)
            Debug.LogError("There are no Player character on scene");
    }

    void Update()
    {
        if (Input.GetKeyDown(keyboardBindingService.Pause))
            menuFlowService.Pause();

        if (isMovingRight)
            MoveRight();
        else if (isMovingLeft)
            MoveLeft();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            menuFlowService.RestartGame();

        if (CurrentPlayer != null)
            CurrentPlayer.Shoot(shooting);

        if (Input.GetKeyDown(keyboardBindingService.Shot))
            Shot(true);
        else if (Input.GetKeyUp(keyboardBindingService.Shot))
            Shot(false);

        if (Input.GetKeyDown(keyboardBindingService.Jump))
            Jump();
        else if (Input.GetKeyUp(keyboardBindingService.Jump))
            JumpOff();

        if (Input.GetKeyDown(keyboardBindingService.Throw))
            ThrowGrenade();
    }

    public void TurnJump(bool isOn)
    {
        btnJump.SetActive(isOn);
    }

    public void TurnMelee(bool isOn) { }

    public void TurnRange(bool isOn)
    {
        btnRange.SetActive(isOn);
    }

    public void TurnDash(bool isOn) { }

    public void MoveLeft()
    {
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing && CurrentPlayer != null)
        {
            CurrentPlayer.MoveLeft();
            isMovingLeft = true;
        }
    }

    public void MoveRight()
    {
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing && CurrentPlayer != null)
        {
            CurrentPlayer.MoveRight();
            isMovingRight = true;
        }
    }

    public void FallDown()
    {
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing && CurrentPlayer != null)
            CurrentPlayer.FallDown();
    }

    public void StopMove()
    {
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing && CurrentPlayer != null)
        {
            CurrentPlayer.StopMove();
            isMovingLeft = false;
            isMovingRight = false;
        }
    }

    public void Jump()
    {
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing && CurrentPlayer != null)
            CurrentPlayer.Jump();
    }

    public void JumpOff()
    {
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing && CurrentPlayer != null)
            CurrentPlayer.JumpOff();
    }

    public void Shot(bool hold)
    {
        shooting = hold;
    }

    private void OnDisable()
    {
        if (CurrentPlayer != null)
            CurrentPlayer.StopMove();

        isMovingLeft = false;
        isMovingRight = false;
    }

    public void ThrowGrenade()
    {
        if (CurrentPlayer != null)
            CurrentPlayer.ThrowGrenade();
    }

    public void IPlay() { }
    public void ISuccess() { }
    public void IPause() { }
    public void IUnPause() { }
    public void IGameOver() { }
    public void IOnRespawn() { }
    public void IOnStopMovingOn() { }
    public void IOnStopMovingOff() { }
}
