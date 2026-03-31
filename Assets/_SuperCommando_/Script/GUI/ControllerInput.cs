using UnityEngine;
using VContainer;

public class ControllerInput : MonoBehaviour, IListener
{
    public static ControllerInput Instance;
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
    bool shooting;
    bool isMovingLeft, isMovingRight;

    [Inject]
    public void Construct(IGameSessionService gameSession)
    {
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        Instance = this;
    }

    private void OnEnable()
    {
        if (gameSession != null)
            StopMove();
    }

    void Start()
    {
        Player = gameSession != null ? gameSession.Player : FindObjectOfType<Player>();
        if (Player == null)
            Debug.LogError("There are no Player character on scene");
    }

    void Update()
    {
        if (Input.GetKeyDown(DefaultValueKeyboard.Instance.keyPause))
            MenuManager.Instance.Pause();

        if (isMovingRight)
            MoveRight();
        else if (isMovingLeft)
            MoveLeft();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            MenuManager.Instance.RestartGame();

        if (gameSession != null && gameSession.Player != null)
            gameSession.Player.Shoot(shooting);

        if (Input.GetKeyDown(DefaultValueKeyboard.Instance.keyShot))
            Shot(true);
        else if (Input.GetKeyUp(DefaultValueKeyboard.Instance.keyShot))
            Shot(false);

        if (Input.GetKeyDown(DefaultValueKeyboard.Instance.keyJump))
            Jump();
        else if (Input.GetKeyUp(DefaultValueKeyboard.Instance.keyJump))
            JumpOff();

        if (Input.GetKeyDown(DefaultValueKeyboard.Instance.keyThrow))
            ThrowGrenade();
    }

    public void TurnJump(bool isOn)
    {
        btnJump.SetActive(isOn);
    }

    public void TurnMelee(bool isOn)
    {
    }

    public void TurnRange(bool isOn)
    {
        btnRange.SetActive(isOn);
    }

    public void TurnDash(bool isOn)
    {
    }

    public void MoveLeft()
    {
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing)
        {
            Player.MoveLeft();
            isMovingLeft = true;
        }
    }

    public void MoveRight()
    {
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing)
        {
            Player.MoveRight();
            isMovingRight = true;
        }
    }

    public void FallDown()
    {
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing)
            Player.FallDown();
    }

    public void StopMove()
    {
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing)
        {
            Player.StopMove();
            isMovingLeft = false;
            isMovingRight = false;
        }
    }

    public void Jump()
    {
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing)
            Player.Jump();
    }

    public void JumpOff()
    {
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing)
            Player.JumpOff();
    }

    public void Shot(bool hold)
    {
        shooting = hold;
    }

    private void OnDisable()
    {
        if (Player != null)
            Player.StopMove();

        isMovingLeft = false;
        isMovingRight = false;
    }

    public void ThrowGrenade()
    {
        if (gameSession != null && gameSession.Player != null)
            gameSession.Player.ThrowGrenade();
    }

    public void IPlay()
    {
    }

    public void ISuccess()
    {
    }

    public void IPause()
    {
    }

    public void IUnPause()
    {
    }

    public void IGameOver()
    {
    }

    public void IOnRespawn()
    {
    }

    public void IOnStopMovingOn()
    {
    }

    public void IOnStopMovingOff()
    {
    }
}
