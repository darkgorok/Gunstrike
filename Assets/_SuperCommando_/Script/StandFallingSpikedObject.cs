using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandFallingSpikedObject : RaycastController,IListener
{
    public enum RespawnType { PlayerDead, AfterTime }
    public RespawnType respawnType;
    public float delayRespawn = 1;

    public LayerMask passengerMask;
    Vector3[] globalWaypoints;

    public float speed = 1;
    private bool cyclic;
    private float waitTime;
    //[Range(0, 2)]
    private float easeAmount = 0;
    private bool isLoop = false;
    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;
    public GameObject destroyFX;

    List<PassengerMovement> passengerMovement;
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();
    bool isMoving = false;

    public float fallingDelay = 1;
    [Tooltip("Active the object when Player in this range")]
    public float detectPlayerDistanceX = 3;
    public float detectPlayerDistanceY = 5;
    public AudioClip soundFalling, soundDestroy;
    Vector3 oriPos;
    public override void Start()
    {
        base.Start();
        oriPos = transform.position;
        globalWaypoints = new Vector3[2];
        globalWaypoints[0] = transform.position;
        globalWaypoints[1] = transform.position + Vector3.down * 100;
    }

    void Update()
    {
       

        if (!isWorked)
        {
            if (GameManager.Instance.Player.controller.collisions.below && GameManager.Instance.Player.controller.collisions.ClosestHit.collider.gameObject == gameObject)
                Work();
            if ((Mathf.Abs(transform.position.x - GameManager.Instance.Player.transform.position.x) <= detectPlayerDistanceX) && (Mathf.Abs(transform.position.y - GameManager.Instance.Player.transform.position.y) <= detectPlayerDistanceY) && (transform.position.y > GameManager.Instance.Player.transform.position.y))
                Work();

            return;
        }

        if (isStop)
            return;

        if (isMoving)
        {
            UpdateRaycastOrigins();

            Vector3 velocity = CalculatePlatformMovement();

            CalculatePassengerMovement(velocity);

            MovePassengers(true);
            transform.Translate(velocity, Space.World);
            MovePassengers(false);

            //var hit = Physics2D.CircleCast(transform.position, 0.5f, Vector2.zero, 0, 1 << LayerMask.NameToLayer("Ground"));
            //if (hit && hit.collider.GetComponent<StandFallingSpikedObject>()==null)
            //{
            //    Instantiate(destroyFX, transform.position, Quaternion.identity);
            //    SoundManager.PlaySfx(soundDestroy);
            //    Destroy(gameObject);
            //}

            var hits = Physics2D.CircleCastAll(transform.position + Vector3.down * 1, 0.2f, Vector2.zero, 0, 1 << LayerMask.NameToLayer("Ground"));
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.collider.GetComponent<StandFallingSpikedObject>() == null)
                    {
                        Instantiate(destroyFX, transform.position, Quaternion.identity);
                        SoundManager.PlaySfx(soundDestroy);
                        //Destroy(gameObject);
                        StopAllCoroutines();
                        gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                hits = Physics2D.CircleCastAll(transform.position + Vector3.down * 1, 0.2f, Vector2.zero, 0, 1 << LayerMask.NameToLayer("Platform"));
                if (hits.Length > 0)
                {
                    foreach (var hit in hits)
                    {
                        if (hit.collider.GetComponent<StandFallingSpikedObject>() == null)
                        {
                            Instantiate(destroyFX, transform.position, Quaternion.identity);
                            SoundManager.PlaySfx(soundDestroy);
                            //Destroy(gameObject);
                            StopAllCoroutines();
                            gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    bool isWorked = false;
    public void Work()
    {
        if (isWorked)
            return;

        StartCoroutine(WorkCo());
        if (respawnType == RespawnType.AfterTime)
            Invoke("RespawnPos", delayRespawn);
    }

    void RespawnPos()
    {
        StopAllCoroutines();
        transform.position = oriPos;
        percentBetweenWaypoints = 0;
        isWorked = false;
        isMoving = false;
        GetComponent<Animator>().SetTrigger("reset");
        gameObject.SetActive(true);
    }

    IEnumerator WorkCo()
    {
        isWorked = true;


        while (isStop) { yield return null; }
        GetComponent<Animator>().SetTrigger("shake");

        float counter = fallingDelay;

        while (counter > 0)
        {
            counter -= Time.deltaTime;


            while (isStop) { yield return null; }

            yield return null;
        }

        isMoving = true;
        SoundManager.PlaySfx(soundFalling);
    }

    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    Vector3 CalculatePlatformMovement()
    {

        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

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
                    {
                        //						if (SmokeFX)
                        //							SmokeFX.SetActive (false);
                        enabled = false;
                    }
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
    }

    void MovePassengers(bool beforeMovePlatform)
    {
        foreach (PassengerMovement passenger in passengerMovement)
        {
            if (!passengerDictionary.ContainsKey(passenger.transform))
            {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }

            if (passenger.moveBeforePlatform == beforeMovePlatform)
            {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }

    void CalculatePassengerMovement(Vector3 velocity)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                    }
                }
            }
        }

        // Horizontally moving platform
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = -skinWidth;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }

        // Passenger on top of a horizontally or downward moving platform
        if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLength = skinWidth * 2;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }

    struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position - Vector3.right * (detectPlayerDistanceX), transform.position + Vector3.right * (detectPlayerDistanceX));
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (detectPlayerDistanceY));
    }

    bool isStop = false;
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
