using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ControllerInput : MonoBehaviour, IListener {
    public static ControllerInput Instance;
    public GameObject rangeAttack;
	Player Player;
    [Header("Button")]
    public GameObject btnJump;
    public GameObject btnRange;

    public float Vertical, Horizontak;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            StopMove();
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
    bool shooting;
    private void Awake()
    {
        Instance = this;
    }

    void Start () {
        
        Player = FindObjectOfType<Player> ();
		if(Player==null)
			Debug.LogError("There are no Player character on scene");
    }

	void Update(){

        if (Input.GetKeyDown(DefaultValueKeyboard.Instance.keyPause))
            MenuManager.Instance.Pause();

        if (isMovingRight)
            MoveRight();
        else if (isMovingLeft)
            MoveLeft();

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            MenuManager.Instance.RestartGame();

        GameManager.Instance.Player.Shoot(shooting);

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

    bool isMovingLeft, isMovingRight;
	
	public void MoveLeft(){
        if (GameManager.Instance.State == GameManager.GameState.Playing)
        {
            Player.MoveLeft();
            isMovingLeft = true;
        }
	}

	public void MoveRight(){
        if (GameManager.Instance.State == GameManager.GameState.Playing)
        {
            Player.MoveRight();
            isMovingRight = true;
        }
	}

	public void FallDown(){
		if (GameManager.Instance.State == GameManager.GameState.Playing)
			Player.FallDown ();
	}


	public void StopMove(){
        if (GameManager.Instance.State == GameManager.GameState.Playing)
        {
            Player.StopMove();
            isMovingLeft = false;
            isMovingRight = false;
        }
	}

	public void Jump (){
		if (GameManager.Instance.State == GameManager.GameState.Playing)
			Player.Jump ();
	}

	public void JumpOff(){
		if (GameManager.Instance.State == GameManager.GameState.Playing)
			Player.JumpOff ();
	}

    public void Shot(bool hold)
    {
        shooting = hold;
    }

    private void OnDisable()
    {
        Player.StopMove();
        isMovingLeft = false;
        isMovingRight = false;
    }

    public void ThrowGrenade()
    {
        GameManager.Instance.Player.ThrowGrenade();
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
