using UnityEngine;
using VContainer;

public enum DoggeType { OverObject, HitObject }
public enum GunHandlerState { AVAILABLE, SWAPPING, RELOADING, EMPTY }
public enum ShootingMethob { SingleShoot, AutoShoot }

[RequireComponent (typeof (Controller2D))]
public partial class Player : MonoBehaviour, ICanTakeDamage, IListener {
    public Animator anim;

    public Collider2D standUpCollider;      //enable this collider 2d if the player lie down on the ground

    [Header("Moving")]
	public float moveSpeed = 3;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;

	[Header("Jump")]
	public float maxJumpHeight = 3;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	public int numberOfJumpMax = 1;
	int numberOfJumpLeft;
	public GameObject JumpEffect;
    public GameObject landingFX;

    [Header("Health")]
	public int maxHealth;
	public int Health{ get; private set;}
	public GameObject HurtEffect;
    public GameObject respawnFX;

    [Header("TAKE DAMAGE")]
    public float rateGetDmg = 0.5f;
    public Color blinkingColor = Color.green;
    [ReadOnly] public bool isBlinking = false;
    public float knockbackForce = 10f;

    [Header("Sound")]
    public AudioClip respawnSound;
	public AudioClip[] jumpSound;
	[Range(0,1)]
	public float jumpSoundVolume = 0.5f;
	public AudioClip landSound;
	[Range(0,1)]
	public float landSoundVolume = 0.5f;
	public AudioClip[] hurtSound;
	[Range(0,1)]
	public float hurtSoundVolume = 0.5f;
	public AudioClip[] deadSound;
	[Range(0,1)]
	public float deadSoundVolume = 0.5f;
    bool isPlayedLandSound;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	[HideInInspector]
	public Vector3 velocity;
	float velocityXSmoothing;

	[ReadOnly] public bool isFacingRight;

    [ReadOnly] public Vector2 input;
    bool isDead = false;

    [HideInInspector]
	public Controller2D controller;

	public bool isPlaying { get; private set;}
	public bool isFinish { get; set;}
    public bool isGrounded { get { return controller.collisions.below; } }
    bool forceStannding = false;
    private IAudioService audioService;
    private ICameraRigService cameraRigService;
    private IControllerInputService controllerInputService;
    private IGameSessionService gameSession;
    private IGunRuntimeService gunRuntimeService;
    private IInventoryService inventoryService;

    [Inject]
    public void Construct(IAudioService audioService, ICameraRigService cameraRigService, IControllerInputService controllerInputService, IGameSessionService gameSession, IGunRuntimeService gunRuntimeService, IInventoryService inventoryService)
    {
        this.audioService = audioService;
        this.cameraRigService = cameraRigService;
        this.controllerInputService = controllerInputService;
        this.gameSession = gameSession;
        this.gunRuntimeService = gunRuntimeService;
        this.inventoryService = inventoryService;
    }

    void Awake()
    {
        ProjectScope.Inject(this);
        controller = GetComponent<Controller2D>();
        if (anim == null)
            anim = GetComponent<Animator>();
    }

	void Start() {

        cameraRigService.ManualControl = true;
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);

		isFacingRight = transform.localScale.x > 0;
		Health = maxHealth;
		numberOfJumpLeft = numberOfJumpMax;

        gunTypeID = gunRuntimeService.GetCurrentGun();
        SetGun(gunTypeID);
        gunRuntimeService.ResetGunBullet();

        grenadeRemaining = maxGrenade;

        if (inventoryService.CurrentGunType != null)
            gunRuntimeService.SetNewGunDuringGameplay(inventoryService.CurrentGunType);
    }

    void Update() {
        
        if (isFrozen)
            return;

		HandleAnimation ();
        GetInput();

        standUpCollider.enabled = !isLieDown;

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

        if (Mathf.Abs(input.x) < 0.3f)
            velocity.x = 0;

        velocity.y += gravity * Time.deltaTime;

        if (controller.collisions.below && !isPlayedLandSound) {
            isPlayedLandSound = true;
			audioService.PlaySfx(landSound, landSoundVolume);
            if (landingFX)
                SpawnSystemHelper.GetNextObject(landingFX, true).transform.position = transform.position;
		} else if (!controller.collisions.below && isPlayedLandSound)
			isPlayedLandSound = false;

        if (isBlinking || !isPlaying)
        {
            ;
        }
        else
        {
           
                gameObject.layer = LayerMask.NameToLayer("Player");
        }

        if (controller.collisions.above)
        {
            CheckBlock();
        }

        if ((transform.position.y + 2) < cameraRigService.ViewportToWorldPoint(Vector3.zero).y)
            gameSession.GameOver();
    }

}
