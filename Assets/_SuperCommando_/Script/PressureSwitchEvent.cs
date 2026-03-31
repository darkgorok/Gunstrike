using UnityEngine;
using VContainer;

public class PressureSwitchEvent : MonoBehaviour, IStandOnEvent
{
    [Header("SIMPLE MOVING")]
    public bool moveTarget = false;
    public Transform target;
    public Vector2 localActive;
    public Vector2 localDisactive = Vector2.one;
    public float moveSpeedA = 1;
    public float moveSpeedD = 3;

    [Header("SEND MESSSAGE")]
    public GameObject targetObject;
    public string eventOnMessage = "On";
    public string eventOffMessage = "Off";
    public AudioClip soundOn, soundOff;

    [ReadOnly] public bool state = false;

    private Vector3 targetA;
    private Vector3 targetD;
    private float movePercent;
    private bool waitForPlayerLeave;
    private Animator anim;
    private IGameSessionService gameSession;
    private IAudioService audioService;

    [Inject]
    public void Construct(IGameSessionService gameSession, IAudioService audioService)
    {
        this.gameSession = gameSession;
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        targetA = target.position + (Vector3)localActive;
        targetD = target.position + (Vector3)localDisactive;
        target.position = targetD;
    }

    private void Update()
    {
        if (moveTarget)
        {
            movePercent += (state ? moveSpeedA : -moveSpeedD) * Time.deltaTime;
            movePercent = Mathf.Clamp01(movePercent);
            target.position = Vector3.Lerp(targetD, targetA, movePercent);
        }

        if (!waitForPlayerLeave || gameSession?.Player == null)
            return;

        if (!gameSession.Player.isGrounded)
        {
            waitForPlayerLeave = false;
            SetState(false);
            NotifyTarget(eventOffMessage);
            audioService?.PlaySfx(soundOff);
        }
    }

    public void StandOnEvent(GameObject instigator)
    {
        if (state)
            return;

        SetState(true);
        NotifyTarget(eventOnMessage);
        audioService?.PlaySfx(soundOn);
        waitForPlayerLeave = instigator.GetComponent<Player>() != null;
    }

    private void SetState(bool value)
    {
        state = value;
        anim.SetBool("state", state);
    }

    private void NotifyTarget(string message)
    {
        if (targetObject)
            targetObject.SendMessage(message, SendMessageOptions.DontRequireReceiver);
        else
            Debug.LogWarning("NEED SET TARGET TO THIS: " + gameObject.name);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying || !enabled)
            return;

        if (target != null && moveTarget)
        {
            float size = .3f;
            Gizmos.DrawLine(target.position + (Vector3)localActive, target.position + (Vector3)localDisactive);
            Gizmos.DrawWireSphere(target.position + (Vector3)localActive, size);
            Gizmos.DrawWireSphere(target.position + (Vector3)localDisactive, size);
        }
    }
}
