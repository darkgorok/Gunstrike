using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    public static CameraFollow Instance;
    [Tooltip("Litmited the camera moving within this box collider")]
    public float limitLeft = -6;
    public float limitRight = 1000;
	public float lookAheadDstX;
	public float lookSmoothTimeX;
	public Vector2 focusAreaSize;
	[HideInInspector]
	public Vector2 _min, _max;
	public bool isFollowing{ get; set; }
    
	FocusArea focusArea;
	float currentLookAheadX;
	float targetLookAheadX;
	float lookAheadDirX;
	float smoothLookVelocityX;

	bool lookAheadStopped;
    [Header("ZOOM IN ZOOM OUT")]
    [Tooltip("Zoom Speed")]
    public bool allowAutoZoomIn = false;
    [Tooltip("How long player don't move to active zoom action")]
    public float timeDelay = 3f;
    public float speed = 10f;
    [Range(50, 100)]
    public float minPercent = 80;
   [ReadOnly] public float maxSize;
    [ReadOnly] public float minSize;
    float timeCounting = 0;

    public float zoomSpeed = 1;
    private bool isZooming = false;
    float originalSize, ZoomSize;
    Camera camera;
    
  [ReadOnly]  public bool manualControl = false;

    [ReadOnly] public bool pauseCamera = false;

    private void Awake()
    {
        Instance = this;
    }

    bool isMovingCameraToPlayer = false;
    public void MoveCameraToPlayerPos()
    {
        isMovingCameraToPlayer = true;
    }

    void Start() {
        camera = GetComponent<Camera>();
        maxSize = camera.orthographicSize;
        minSize = maxSize * (minPercent / 100f);

        originalSize = camera.orthographicSize;

        focusArea = new FocusArea (GameManager.Instance.Player.controller.boxcollider.bounds, focusAreaSize);
        
        _min = new Vector2(limitLeft, -100);
        _max = new Vector2(limitRight, 100);
        isFollowing = true;
	}

    void Update()
    {
        if (pauseCamera || GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        timeCounting += Time.deltaTime;


        if (Input.anyKey || GameManager.Instance.Player.input != Vector2.zero)
        {
            timeCounting = 0;
        }
    }

    void LateUpdate() {
        if (!manualControl)
            DoFollowPlayer();
	}

    public void DoFollowPlayer()
    {
        if (!isFollowing)
            return;
        if (pauseCamera)
            return;
        if (isMovingCameraToPlayer)
        {
            focusArea.Update(GameManager.Instance.Player.controller.boxcollider.bounds);
            Vector2 focusPositionX = focusArea.centre;
            focusPositionX += Vector2.right * currentLookAheadX;
            focusPositionX.y = transform.position.y;     //fixed position Y
            transform.position = Vector3.Lerp(transform.position, focusPositionX, 5 * Time.deltaTime) + Vector3.forward * -10;
            if (Vector2.Distance(transform.position, focusPositionX) < 0.05f)
                isMovingCameraToPlayer = false;
            return;
        }

        focusArea.Update(GameManager.Instance.Player.controller.boxcollider.bounds);

        Vector2 focusPosition = focusArea.centre;

        if (focusArea.velocity.x != 0)
        {
            lookAheadDirX = Mathf.Sign(focusArea.velocity.x);
            if (Mathf.Sign(GameManager.Instance.Player.controller.playerInput.x) == Mathf.Sign(focusArea.velocity.x) && GameManager.Instance.Player.controller.playerInput.x != 0)
            {
                lookAheadStopped = false;
                targetLookAheadX = lookAheadDirX * lookAheadDstX;
            }
            else
            {
                if (!lookAheadStopped)
                {
                    targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
                }
            }
        }
        else if((GameManager.Instance.Player.isFacingRight && Mathf.Sign(currentLookAheadX) > 0) || (!GameManager.Instance.Player.isFacingRight && Mathf.Sign(currentLookAheadX) < 0))
        {
            targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
        }

        //ZOOM ZONE
        if (isZooming)
        {
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, ZoomSize, zoomSpeed * Time.deltaTime);
        }
        else
        {
            if (timeCounting >= timeDelay && allowAutoZoomIn)
            {
                camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, minSize, speed * Time.deltaTime);
            }
            else
                camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, maxSize, speed * Time.deltaTime * 3);
        }

        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

        focusPosition.y = transform.position.y;     //fixed position Y
        focusPosition += Vector2.right * currentLookAheadX;

        focusPosition.x = Mathf.Clamp(focusPosition.x, _min.x + CameraHalfWidth, _max.x - CameraHalfWidth);
 
        transform.position = (Vector3)focusPosition + Vector3.forward * -10;
    }

    public float CameraHalfWidth
    {
        get { return (Camera.main.orthographicSize * ((float)Screen.width / Screen.height)); }
    }


    void OnDrawGizmos() {
		Gizmos.color = new Color (1, 0, 0, .5f);
		Gizmos.DrawCube (focusArea.centre, focusAreaSize);
        Gizmos.color = Color.yellow;
        //Gizmos.DrawRay(transform.position + Vector3.up * Camera.main.orthographicSize, Vector3.right * 1000);
        //Gizmos.DrawRay(transform.position - Vector3.up * Camera.main.orthographicSize, Vector3.right * 1000);

        Vector2 boxSize = new Vector2(limitRight - limitLeft, Camera.main.orthographicSize * 2);
        Vector2 center = (new Vector2((limitRight + limitLeft) * 0.5f, transform.position.y));
        Gizmos.DrawWireCube(center, boxSize);
    }

	struct FocusArea {
		public Vector2 centre;
		public Vector2 velocity;
		float left,right;
		float top,bottom;


		public FocusArea(Bounds targetBounds, Vector2 size) {
			left = targetBounds.center.x - size.x/2;
			right = targetBounds.center.x + size.x/2;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;

			velocity = Vector2.zero;
			centre = new Vector2((left+right)/2,(top +bottom)/2);
		}

		public void Update(Bounds targetBounds) {
			float shiftX = 0;
			if (targetBounds.min.x < left) {
				shiftX = targetBounds.min.x - left;
			} else if (targetBounds.max.x > right) {
				shiftX = targetBounds.max.x - right;
			}
			left += shiftX;
			right += shiftX;

			float shiftY = 0;
			if (targetBounds.min.y < bottom) {
				shiftY = targetBounds.min.y - bottom;
			} else if (targetBounds.max.y > top) {
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;
			centre = new Vector2((left+right)/2,(top +bottom)/2);
			velocity = new Vector2 (shiftX, shiftY);
		}
	}

    public void ZoomIn(float value)
    {
        isZooming = true;
        ZoomSize = value;
    }

    public void ZoomOut()
    {
        isZooming = false;
    }
}
