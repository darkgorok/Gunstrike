using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class TheGate : MonoBehaviour {
	public Transform gate;
	public float speed = 2;
	public float moveY = 3;
    public float checkingRadius = 0.2f;
	public bool isLocked = false;

	public GameObject buttonTrigger;
	public SpriteRenderer statusDoorSr;
	public Sprite lockedSprite, openImage;

	public AudioClip soundOpen, soundLocked, soundUnlock;
	bool isOpening = false;
	Vector3 oriGatePos;
    private IAudioService audioService;
    private IGameSessionService gameSession;

	public FloatingTextParameter lockMessage;
	public FloatingTextParameter unlockMessage;

    [Inject]
    public void Construct(IAudioService audioService, IGameSessionService gameSession)
    {
        this.audioService = audioService;
        this.gameSession = gameSession;
    }

	void Awake(){
        ProjectScope.Inject(this);
		oriGatePos = gate.position;
	}

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(CheckPlayerCo());
    }

    void Start(){
		statusDoorSr.sprite = isLocked ? lockedSprite : openImage;
        buttonTrigger.SetActive(false);
       
	}

    

    IEnumerator CheckPlayerCo(){
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            var hit = Physics2D.CircleCast(transform.position, checkingRadius, Vector2.zero, 0, gameSession.PlayerLayer);
            if (hit)
            {
                buttonTrigger.SetActive(true);
                
                if (OpenGate())
                {
                    buttonTrigger.SetActive(false);
                    yield return new WaitForSeconds(1);
                    audioService.PlaySfx(soundOpen);
                    //open gate
                    while (true)
                    {
                        gate.position = Vector2.MoveTowards(gate.position, oriGatePos + Vector3.up * moveY, speed * Time.deltaTime);
                        yield return null;

                        if (Vector2.Distance(gate.position, oriGatePos + Vector3.up * moveY) < 0.01f)
                        {
                            buttonTrigger.SetActive(false);
                            yield break;
                        }
                    }
                }

                while (hit)     //wait until player leave
                {
                    hit = Physics2D.CircleCast(transform.position, checkingRadius, Vector2.zero, 0, gameSession.PlayerLayer);
                    yield return new WaitForSeconds(0.1f);
                }
                buttonTrigger.SetActive(false);
            }
        }
	}

	public bool OpenGate(){
        if (!gameSession.HasKey && isLocked)
        {
            audioService.PlaySfx(soundLocked);
            FloatingTextManager.Instance.ShowText(lockMessage, transform.position);
            return false;
        }
        else if (gameSession.HasKey && isLocked)
        {
            
            gameSession.HasKey = false;
            isLocked = false;
            statusDoorSr.sprite = isLocked ? lockedSprite : openImage;
            audioService.PlaySfx(soundUnlock);
            FloatingTextManager.Instance.ShowText(unlockMessage, transform.position);
            return true;
        }
        else
            return true;        //mean isLocked = false, and open the gate when detect player
	}

    void OnDrawGizmos()
    {
        

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkingRadius);
        if (Application.isPlaying)
            return;
        if (gate)
            Gizmos.DrawLine(gate.position, gate.position + Vector3.up * moveY);
    }
}
