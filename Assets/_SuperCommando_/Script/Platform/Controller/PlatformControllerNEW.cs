using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformControllerNEW : RaycastController, IListener {
    public bool moveWhenPlayer = false;

    public bool autoMove = true;
	public bool isLoop = true;
	[Header("Look At Target")]
	public bool lookAtNextPoint = false;
	public float lookAtOffset = 0;
	public float speedLook = 1;
    bool isReverseMoving = false;
    Vector3 firstPoint, lastPoint;
    [Space]
	[Tooltip("Ignore All Value and just follow this object")]
	public Transform followTarget;	//ignore all value if follow target
	public float delayStart = 0;
	public LayerMask passengerMask;
	bool isStanding;

	[Tooltip("If has 2 or more, use them instead localWaypoints")]
	public GameObject Paths;
	public List<Transform> objectLocalWaypoints;
	public List<Vector3> localWaypoints;
	Vector3[] globalWaypoints;

	public float speed = 3;
	public bool cyclic;
	public float waitTime = 1;
	[Range(0,2)]
	public float easeAmount;
	public AudioClip soundEndPoint;
	public AudioSource ASource;

	[Header("Draw line")]
	public bool drawLine = false;
	public float lineWidth = 0.2f;
	public Material lineMat;
    public float offsetLineZ = -1;
	LineRenderer lineRen;

//	public bool isStickPlayer = false;

	int fromWaypointIndex;
	float percentBetweenWaypoints;
	float nextMoveTime;
	public bool isFinishedWays{ get; set; }

	public bool allowMoving { get; set; }

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform,Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

   [ReadOnly] public bool isStop = true;

    //Control moving by the PlatformControllerSwitcher script
    [ReadOnly] public bool isManualControl = false;

    public override void Start () {
		base.Start ();

		allowMoving = autoMove;

		if (Paths) {
			int childs = Paths.transform.childCount;
			objectLocalWaypoints.Clear ();
			if (childs > 0) {
				for (int i = 0; i < childs; i++) {
					if (Paths.transform.GetChild (i).gameObject.activeInHierarchy)
						objectLocalWaypoints.Add (Paths.transform.GetChild (i));
				}
			}
		}

		if (objectLocalWaypoints.Count >= 2) {
			localWaypoints.Clear ();
			for (int i = 0; i < objectLocalWaypoints.Count; i++) {
				localWaypoints.Add (objectLocalWaypoints [i].localPosition);
			}
		}

		globalWaypoints = new Vector3[localWaypoints.Count];
		for (int i =0; i < localWaypoints.Count; i++) {
			globalWaypoints[i] = localWaypoints[i] + transform.position;
		}

        firstPoint = globalWaypoints[0];
        lastPoint = globalWaypoints[globalWaypoints.Length - 1];

        
            Invoke("Play", delayStart);
	}

    void Play()
    {
        if (isManualControl)
            return;

        isStop = false;

        nextMoveTime = Time.time + waitTime;
    }

    float oldPercent;

    void LateUpdate () {
		if (!allowMoving)
			return;
		
		UpdateRaycastOrigins ();

        oldPercent = percentBetweenWaypoints;

        Vector3 velocity = CalculatePlatformMovement ();

		CalculatePassengerMovement (velocity);

		
//		if (followTarget)
//			transform.position = followTarget.position;
		
//		if (isStickPlayer) {
//			if (isStanding) {
//				GameManager.Instance.Player.transform.SetParent (transform);
//			} else {
//				if (!isStanding && !GameManager.Instance.Player.isRoping)
//					GameManager.Instance.Player.transform.SetParent (null);
//			}
//		} else {
			
        if ((!moveWhenPlayer || isStanding) && !isStop)
        {
            MovePassengers(true);
            transform.Translate(velocity, Space.World);
            MovePassengers(false);
        }
        else if ((moveWhenPlayer && !isStanding) || isStop)
        {
            percentBetweenWaypoints = oldPercent;
        }

       

        if (lookAtNextPoint)
            Look();
        //		}
    }
	int toWaypointIndex;
    public void Look()
    {
        if (lookTempStop)
            return;
        //		foreach (var obj in rotateObj) {
        if (toWaypointIndex >= globalWaypoints.Length)
            return;

        if (fromWaypointIndex >= globalWaypoints.Length)
            return;

        if (globalWaypoints[fromWaypointIndex] == globalWaypoints[toWaypointIndex])
            return;

        //if (!cyclic && ((isReverseMoving && globalWaypoints[toWaypointIndex] == lastPoint) || (!isReverseMoving && globalWaypoints[toWaypointIndex] == firstPoint)))
        //    return;

        Vector3 diff = globalWaypoints[toWaypointIndex] - globalWaypoints[fromWaypointIndex];

        //Debug.LogError(globalWaypoints[fromWaypointIndex] + " > " + globalWaypoints[toWaypointIndex]);

        //Debug.LogError(globalWaypoints[toWaypointIndex] + "/" + globalWaypoints[fromWaypointIndex]);
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        float finalZ = Mathf.Lerp(transform.rotation.eulerAngles.z, rot_z, speedLook);

        float offsetReverse = isReverseMoving ? 180 : 0;

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, finalZ + offsetReverse + lookAtOffset);
        //		}
    }


	float Ease(float x) {
		float a = easeAmount + 1;
		return Mathf.Pow(x,a) / (Mathf.Pow(x,a) + Mathf.Pow(1-x,a));
	}

	Vector3 CalculatePlatformMovement() {

		if (followTarget) {
			return  followTarget.position - transform.position;
		}

		if (Time.time < nextMoveTime) {
			return Vector3.zero;
		}

		fromWaypointIndex %= globalWaypoints.Length;
		toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance (globalWaypoints [fromWaypointIndex], globalWaypoints [toWaypointIndex]);
		percentBetweenWaypoints += Time.deltaTime * speed/distanceBetweenWaypoints;
		percentBetweenWaypoints = Mathf.Clamp01 (percentBetweenWaypoints);
		float easedPercentBetweenWaypoints = Ease (percentBetweenWaypoints);

		Vector3 newPos = Vector3.Lerp (globalWaypoints [fromWaypointIndex], globalWaypoints [toWaypointIndex], easedPercentBetweenWaypoints);


		if (percentBetweenWaypoints >= 1) {
			percentBetweenWaypoints = 0;
			fromWaypointIndex ++;

			if (!isLoop && (fromWaypointIndex >= globalWaypoints.Length - 1)) {
				isFinishedWays = true;
				enabled = false;
			} else if (!cyclic) {
				if (fromWaypointIndex >= globalWaypoints.Length - 1) {
					
					if (ASource) {
						ASource.clip = soundEndPoint;
						ASource.volume = 1;
						ASource.Play ();
					}
					fromWaypointIndex = 0;
					System.Array.Reverse (globalWaypoints);
                    
                    isReverseMoving = !isReverseMoving;
                    lookTempStop = true;
                    Invoke("AllowLookAgain", 0.1f);
                }
			}
			nextMoveTime = Time.time + waitTime;
		}

		return newPos - transform.position;
	}

    bool lookTempStop = false;
    void AllowLookAgain()
    {
        lookTempStop = false;
    }

	void MovePassengers(bool beforeMovePlatform) {
		foreach (PassengerMovement passenger in passengerMovement) {
			if (!passengerDictionary.ContainsKey(passenger.transform)) {
				passengerDictionary.Add(passenger.transform,passenger.transform.GetComponent<Controller2D>());
			}

			if (passenger.moveBeforePlatform == beforeMovePlatform) {
				passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
			}
		}
	}

	bool CalculatePassengerMovement(Vector3 velocity) {
		HashSet<Transform> movedPassengers = new HashSet<Transform> ();
		passengerMovement = new List<PassengerMovement> ();

		float directionX = Mathf.Sign (velocity.x);
		float directionY = Mathf.Sign (velocity.y);


		isStanding = false;

		// Vertically moving platform
		if (velocity.y != 0) {
			float rayLength = Mathf.Abs (velocity.y) + skinWidth;
			
			for (int i = 0; i < verticalRayCount; i ++) {
				Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

				if (hit && hit.distance != 0) {
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add(hit.transform);
						float pushX = (directionY == 1)?velocity.x:0;
						float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

						passengerMovement.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY), directionY == 1, true));

						isStanding = true;
					}
				}
			}
		}

		// Horizontally moving platform
		if (velocity.x != 0) {
			float rayLength = Mathf.Abs (velocity.x) + skinWidth;
			
			for (int i = 0; i < horizontalRayCount; i ++) {
				Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

				if (hit && hit.distance != 0) {
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
						float pushY = -skinWidth;
						
						passengerMovement.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY), false, true));

						isStanding = true;
					}
				}
			}
		}

		// Passenger on top of a horizontally or downward moving platform
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0) {
			float rayLength = skinWidth * 2;
			
			for (int i = 0; i < verticalRayCount; i ++) {
				Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
				
				if (hit && hit.distance != 0) {
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x;
						float pushY = velocity.y;
						
						passengerMovement.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY), true, false));

						isStanding = true;
					}
				}
			}
		}

		return isStanding;
	}

	struct PassengerMovement {
		public Transform transform;
		public Vector3 velocity;
		public bool standingOnPlatform;
		public bool moveBeforePlatform;

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform) {
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	}

//	void OnDrawGizmos() {
//		if (localWaypoints != null && this.enabled) {
//			Gizmos.color = Color.red;
//			float size = .3f;
//
//			for (int i =0; i < localWaypoints.Count; i ++) {
//				Vector3 globalWaypointPos = (Application.isPlaying)?globalWaypoints[i] : localWaypoints[i] + transform.position;
//				Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
//				Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
//			}
//		}
//	}

	void OnDrawGizmos() {
		if (followTarget) {
            if (!Application.isPlaying)
            {
                transform.position = followTarget.position;
                if (lineRen)
                    DestroyImmediate(lineRen);
            }
			
			return;
		}

		if (Paths) {
			int childs = Paths.transform.childCount;
			objectLocalWaypoints.Clear ();
			if (childs > 0) {
				for (int i = 0; i < childs; i++) {
					if (Paths.transform.GetChild (i).gameObject.activeInHierarchy)
						objectLocalWaypoints.Add (Paths.transform.GetChild (i));
				}
			}
		}

		if (objectLocalWaypoints.Count >= 2) {
			localWaypoints.Clear ();
			for (int i = 0; i < objectLocalWaypoints.Count; i++) {
				if (objectLocalWaypoints [i] == null)
					objectLocalWaypoints.RemoveAt (i);
				else
					localWaypoints.Add (objectLocalWaypoints [i].localPosition);
			}
		}

		if (!Application.isPlaying) {
			lineRen = GetComponent<LineRenderer> ();

			if (drawLine) {
				if (!lineRen)
					lineRen = gameObject.AddComponent<LineRenderer> ();

				lineRen.positionCount = localWaypoints.Count;
				lineRen.useWorldSpace = true;
				lineRen.startWidth = lineWidth;
				lineRen.material = lineMat;
				lineRen.textureMode = LineTextureMode.Tile;

				for (int i = 0; i < localWaypoints.Count; i++)
					lineRen.SetPosition (i, localWaypoints [i] + transform.position + Vector3.forward * offsetLineZ);

			} else if (lineRen)
				DestroyImmediate (lineRen);
		}
		
		if (localWaypoints != null && this.enabled) {
			for (int i =0; i < localWaypoints.Count; i ++) {
				Gizmos.color = Color.red;
				float size = .3f;
				Vector3 globalWaypointPos = (Application.isPlaying)?globalWaypoints[i] : localWaypoints[i] + transform.position;
//				Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
//				Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
				if ((i + 1 >= localWaypoints.Count) && !isLoop && !cyclic)
					Gizmos.DrawWireCube (globalWaypointPos,Vector3.one * 0.5f);
				else
					Gizmos.DrawWireSphere (globalWaypointPos, size);

				if (i + 1 >= localWaypoints.Count) {
					if (cyclic && isLoop) {
						Gizmos.color = Color.yellow;
						if (Application.isPlaying)
							Gizmos.DrawLine (globalWaypoints [i], globalWaypoints [0]);
						else
							Gizmos.DrawLine (localWaypoints [i] + transform.position, localWaypoints [0] + transform.position);
					}
					return;
				}

				Gizmos.color = Color.green;
				if (Application.isPlaying)
					Gizmos.DrawLine (globalWaypoints [i], globalWaypoints [i + 1]);
				else
					Gizmos.DrawLine (localWaypoints [i] + transform.position, localWaypoints [i + 1] + transform.position);
			}
		}
	}

	
	#region IListener implementation

	public void IPlay ()
	{
		//		throw new System.NotImplementedException ();
	}

	public void ISuccess ()
	{
		//		throw new System.NotImplementedException ();
	}

	public void IPause ()
	{
		//		throw new System.NotImplementedException ();
	}

	public void IUnPause ()
	{
		//		throw new System.NotImplementedException ();
	}

	public void IGameOver ()
	{
		//		throw new System.NotImplementedException ();
	}

	public void IOnRespawn ()
	{
		//		throw new System.NotImplementedException ();
	}

	public void IOnStopMovingOn ()
	{
		isStop = true;

	}

	public void IOnStopMovingOff ()
	{
		//		anim.enabled = true;
		isStop = false;
	}

	#endregion
}
