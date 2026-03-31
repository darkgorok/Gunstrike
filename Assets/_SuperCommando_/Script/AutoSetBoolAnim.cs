using UnityEngine;
using VContainer;

public class AutoSetBoolAnim : MonoBehaviour
{
    public Animator anim;
    public string boolName = "open";
    public bool stateOnStart = true;
    public float delayOnStart = 1;
    public float rate = 2;
    public AudioClip soundOpen, soundClose;
    public float playSoundPlayerInRange = 10;

    private bool state;
    private bool hasStartedLoop;
    private float startDelayRemaining;
    private float toggleTimer;
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
        state = stateOnStart;
        anim.SetBool(boolName, state);
        startDelayRemaining = delayOnStart;
        toggleTimer = Mathf.Max(0.01f, rate);
    }

    private void Update()
    {
        if (!hasStartedLoop)
        {
            startDelayRemaining -= Time.deltaTime;
            if (startDelayRemaining > 0f)
                return;

            hasStartedLoop = true;
        }

        toggleTimer -= Time.deltaTime;
        if (toggleTimer > 0f)
            return;

        toggleTimer = Mathf.Max(0.01f, rate);
        state = !state;
        anim.SetBool(boolName, state);

        if (gameSession?.Player != null &&
            Vector2.Distance(gameSession.Player.transform.position, transform.position) < playSoundPlayerInRange)
        {
            audioService?.PlaySfx(state ? soundOpen : soundClose);
        }
    }
}
