using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class StandFallingSpikedObject : RaycastController, IListener
{
    public enum RespawnType { PlayerDead, AfterTime }

    public RespawnType respawnType;
    public float delayRespawn = 1;
    public LayerMask passengerMask;
    public float speed = 1;
    public GameObject destroyFX;
    public float fallingDelay = 1;
    [Tooltip("Active the object when Player in this range")]
    public float detectPlayerDistanceX = 3;
    public float detectPlayerDistanceY = 5;
    public AudioClip soundFalling, soundDestroy;

    private Vector3[] globalWaypoints;
    private Vector3 oriPos;
    private readonly Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();
    private List<PassengerMovement> passengerMovement;
    private bool cyclic;
    private float waitTime;
    private float easeAmount = 0;
    private bool isLoop = false;
    private int fromWaypointIndex;
    private float percentBetweenWaypoints;
    private float nextMoveTime;
    private bool isMoving;
    private bool isWorked;
    private bool isStop;
    private float fallingDelayRemaining = -1f;
    private float respawnDelayRemaining = -1f;
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

    public override void Start()
    {
        base.Start();
        oriPos = transform.position;
        globalWaypoints = new Vector3[2];
        globalWaypoints[0] = transform.position;
        globalWaypoints[1] = transform.position + Vector3.down * 100;
    }

    private void Update()
    {
        if (respawnDelayRemaining >= 0f)
        {
            respawnDelayRemaining -= Time.deltaTime;
            if (respawnDelayRemaining <= 0f)
                RespawnPos();
        }

        if (fallingDelayRemaining >= 0f && !isStop)
        {
            fallingDelayRemaining -= Time.deltaTime;
            if (fallingDelayRemaining <= 0f)
            {
                fallingDelayRemaining = -1f;
                isMoving = true;
                audioService?.PlaySfx(soundFalling);
            }
        }

        if (!isWorked)
        {
            TryStartWork();
            return;
        }

        if (isStop || !isMoving)
            return;

        UpdateRaycastOrigins();
        Vector3 velocity = CalculatePlatformMovement();
        CalculatePassengerMovement(velocity);
        MovePassengers(true);
        transform.Translate(velocity, Space.World);
        MovePassengers(false);

        if (HasHitBlockingSurface())
        {
            if (destroyFX != null)
                Instantiate(destroyFX, transform.position, Quaternion.identity);

            audioService?.PlaySfx(soundDestroy);
            gameObject.SetActive(false);
        }
    }

    private void TryStartWork()
    {
        Player player = gameSession?.Player;
        if (player == null)
            return;

        if (player.controller.collisions.below && player.controller.collisions.ClosestHit.collider.gameObject == gameObject)
        {
            Work();
            return;
        }

        bool isPlayerInRange =
            Mathf.Abs(transform.position.x - player.transform.position.x) <= detectPlayerDistanceX &&
            Mathf.Abs(transform.position.y - player.transform.position.y) <= detectPlayerDistanceY &&
            transform.position.y > player.transform.position.y;

        if (isPlayerInRange)
            Work();
    }

    public void Work()
    {
        if (isWorked)
            return;

        isWorked = true;
        GetComponent<Animator>().SetTrigger("shake");
        fallingDelayRemaining = fallingDelay;

        if (respawnType == RespawnType.AfterTime)
            respawnDelayRemaining = delayRespawn;
    }

    private void RespawnPos()
    {
        respawnDelayRemaining = -1f;
        fallingDelayRemaining = -1f;
        transform.position = oriPos;
        percentBetweenWaypoints = 0;
        fromWaypointIndex = 0;
        isWorked = false;
        isMoving = false;
        GetComponent<Animator>().SetTrigger("reset");
        gameObject.SetActive(true);
    }

    private bool HasHitBlockingSurface()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position + Vector3.down, 0.2f, Vector2.zero, 0, 1 << LayerMask.NameToLayer("Ground"));
        if (ContainsNonSpikeHit(hits))
            return true;

        hits = Physics2D.CircleCastAll(transform.position + Vector3.down, 0.2f, Vector2.zero, 0, 1 << LayerMask.NameToLayer("Platform"));
        return ContainsNonSpikeHit(hits);
    }

    private bool ContainsNonSpikeHit(RaycastHit2D[] hits)
    {
        if (hits == null || hits.Length == 0)
            return false;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.GetComponent<StandFallingSpikedObject>() == null)
                return true;
        }

        return false;
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

            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    if (!isLoop)
                        enabled = false;

                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }

            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
    }

    private void MovePassengers(bool beforeMovePlatform)
    {
        foreach (PassengerMovement passenger in passengerMovement)
        {
            if (!passengerDictionary.ContainsKey(passenger.transform))
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());

            if (passenger.moveBeforePlatform == beforeMovePlatform)
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
        }
    }

    private void CalculatePassengerMovement(Vector3 velocity)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                if (hit && hit.distance != 0 && !movedPassengers.Contains(hit.transform))
                {
                    movedPassengers.Add(hit.transform);
                    float pushX = (directionY == 1) ? velocity.x : 0;
                    float pushY = velocity.y - (hit.distance - skinWidth) * directionY;
                    passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                }
            }
        }

        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                if (hit && hit.distance != 0 && !movedPassengers.Contains(hit.transform))
                {
                    movedPassengers.Add(hit.transform);
                    float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                    float pushY = -skinWidth;
                    passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                }
            }
        }

        if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLength = skinWidth * 2;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit && hit.distance != 0 && !movedPassengers.Contains(hit.transform))
                {
                    movedPassengers.Add(hit.transform);
                    passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(velocity.x, velocity.y), true, false));
                }
            }
        }
    }

    private struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform transform, Vector3 velocity, bool standingOnPlatform, bool moveBeforePlatform)
        {
            this.transform = transform;
            this.velocity = velocity;
            this.standingOnPlatform = standingOnPlatform;
            this.moveBeforePlatform = moveBeforePlatform;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position - Vector3.right * detectPlayerDistanceX, transform.position + Vector3.right * detectPlayerDistanceX);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * detectPlayerDistanceY);
    }

    public void IPlay() { }
    public void ISuccess() { }
    public void IPause() { }
    public void IUnPause() { }
    public void IGameOver() { }

    public void IOnRespawn()
    {
        RespawnPos();
    }

    public void IOnStopMovingOn()
    {
        isStop = true;
    }

    public void IOnStopMovingOff()
    {
        isStop = false;
    }
}
