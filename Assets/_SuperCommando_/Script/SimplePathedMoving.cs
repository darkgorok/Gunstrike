using UnityEngine;
using VContainer;

public class SimplePathedMoving : MonoBehaviour
{
    public bool use = true;
    [Header("Manual Control: Call Play()")]
    public bool manualControl = false;
    [Space]
    public float delayOnStart = 0;
    public AudioClip soundWhenMoveNextTarget;
    public float playSoundWhenPlayerInRange = 8;
    public Vector3[] localWaypoints;
    public float speed = 1;
    public bool cyclic;
    public bool loop = false;
    public float waitTime = 0.5f;
    [Range(0, 2)]
    public float easeAmount;

    private bool waitingForManualPlay;
    private bool isPlaying;
    private float startDelayRemaining;
    private Vector3[] globalWaypoints;
    private int fromWaypointIndex;
    private float percentBetweenWaypoints;
    private float nextMoveTime;
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

    public void Init(float delay, float speed, Vector3[] localWaypoints, bool loop = false)
    {
        delayOnStart = delay;
        this.speed = speed;
        this.localWaypoints = localWaypoints;
        this.loop = loop;
    }

    public void Play()
    {
        waitingForManualPlay = false;
    }

    private void Start()
    {
        if (!use)
        {
            Destroy(this);
            return;
        }

        waitingForManualPlay = manualControl;
        startDelayRemaining = delayOnStart;
    }

    private void Update()
    {
        if (!use)
            return;

        if (waitingForManualPlay)
            return;

        if (!isPlaying)
        {
            if (startDelayRemaining > 0f)
            {
                startDelayRemaining -= Time.deltaTime;
                return;
            }

            if (localWaypoints.Length < 2)
                return;

            isPlaying = true;
            globalWaypoints = new Vector3[localWaypoints.Length];
            for (int i = 0; i < localWaypoints.Length; i++)
                globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    private void LateUpdate()
    {
        if (!isPlaying)
            return;

        Vector3 velocity = CalculatePlatformMovement();
        transform.Translate(velocity);
    }

    private float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    private Vector3 CalculatePlatformMovement()
    {
        if (Time.time < nextMoveTime)
            return Vector3.zero;

        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);
        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (gameSession?.Player != null &&
                Vector2.Distance(gameSession.Player.transform.position, transform.position) < playSoundWhenPlayerInRange)
            {
                audioService?.PlaySfx(soundWhenMoveNextTarget);
            }

            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    if (!loop)
                    {
                        Destroy(this);
                        return Vector3.zero;
                    }

                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }

            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
    }

    private void OnDrawGizmos()
    {
        if (!use || Application.isPlaying || !enabled)
            return;

        if (localWaypoints == null)
            return;

        float size = .3f;
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            Gizmos.color = Color.red;
            Vector3 globalWaypointPos = Application.isPlaying ? globalWaypoints[i] : localWaypoints[i] + transform.position;
            Gizmos.DrawWireSphere(globalWaypointPos, size);

            if (i + 1 >= localWaypoints.Length)
            {
                if (cyclic)
                {
                    Gizmos.color = Color.yellow;
                    if (Application.isPlaying)
                        Gizmos.DrawLine(globalWaypoints[i], globalWaypoints[0]);
                    else
                        Gizmos.DrawLine(localWaypoints[i] + transform.position, localWaypoints[0] + transform.position);
                }

                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(localWaypoints[i] + transform.position, localWaypoints[i + 1] + transform.position);
        }
    }
}
