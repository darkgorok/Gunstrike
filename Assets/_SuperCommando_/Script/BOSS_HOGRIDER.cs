using UnityEngine;
using VContainer;

public class BOSS_HOGRIDER : BossManager, ICanTakeDamage, IListener
{
    public float walkSpeed = 1f;
    public float runningSpeed = 3f;
    [Range(1, 1000)]
    public int health = 350;
    [ReadOnly] public int currentHealth;
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    protected HealthBarEnemyNew healthBar;
    public float gravity = 35f;

    [Header("EARTH QUAKE")]
    public float _eqTime = 0.3f;
    public float _eqSpeed = 60f;
    public float _eqSize = 1.5f;

    [Header("SOUND")]
    public AudioClip attackSound;
    public AudioClip deadSound;
    public AudioClip hurtSound;
    public AudioClip detectSound;
    public AudioClip[] hitWallSound;

    [Header("*** HODRIDER ***")]
    public float minDelay = 2f;
    public float maxDelay = 4f;
    public float stunningTime = 3f;
    public GameObject hitWallFX;
    public GameObject stunningFX;

    [Header("EFFECT WHEN DIE")]
    public GameObject dieExplosionFX;
    public Vector2 dieExplosionSize = new Vector2(2, 3);
    public AudioClip dieExplosionSound;

    [HideInInspector]
    public bool isDead = false;

    [HideInInspector]
    protected Vector3 velocity;
    protected float velocityXSmoothing = 0f;
    [ReadOnly] public bool moving = false;
    [ReadOnly] public bool isPlaying = false;

    private Controller2D controller;
    private Animator anim;
    private bool isRunning;
    private Vector2 direction;
    private readonly EnemyStateMachine stateMachine = new EnemyStateMachine();
    private WaitingState waitingState;
    private ChargingState chargingState;
    private StunningState stunningState;
    private DeathState deathState;
    private BossDeathSequence deathSequence;
    private float stateTimer;

    private IAudioService audioService;
    private IControllerInputService controllerInputService;
    private IGameplayPresentationService presentationService;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IAudioService audioService, IControllerInputService controllerInputService, IGameplayPresentationService presentationService, IGameSessionService gameSession)
    {
        this.audioService = audioService;
        this.controllerInputService = controllerInputService;
        this.presentationService = presentationService;
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        waitingState = new WaitingState(this);
        chargingState = new ChargingState(this);
        stunningState = new StunningState(this);
        deathState = new DeathState(this);
    }

    private void Start()
    {
        controller = GetComponent<Controller2D>();
        anim = GetComponent<Animator>();

        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);

        direction = isFacingRight() ? Vector2.right : Vector2.left;
        currentHealth = health;
        stunningFX.SetActive(false);

        deathSequence = new BossDeathSequence(
            audioService,
            controllerInputService,
            presentationService,
            gameSession,
            transform,
            () => gameObject.SetActive(false));
    }

    private void Update()
    {
        anim.SetFloat("speed", Mathf.Abs(velocity.x));

        if (!isPlaying && !isDead)
            return;

        stateMachine.Tick(Time.deltaTime);

        if (isDead || gameSession.State != GameManager.GameState.Playing || gameSession.Player.isFinish)
        {
            velocity.x = 0f;
            return;
        }

        float targetVelocityX = direction.x * (isRunning ? runningSpeed : walkSpeed);
        velocity.x = moving ? Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, controller.collisions.below ? 0.1f : 0.2f) : 0f;
        velocity.y += -gravity * Time.deltaTime;

        if (!moving)
            velocity.x = 0f;
    }

    private void LateUpdate()
    {
        if (isDead)
            return;

        controller.Move(velocity * Time.deltaTime, false);

        if (controller.collisions.above || controller.collisions.below)
            velocity.y = 0f;
    }

    public bool isFacingRight()
    {
        return transform.rotation.y == 0;
    }

    public override void Play()
    {
        if (isPlaying)
            return;

        isPlaying = true;
        stateMachine.ChangeState(waitingState);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!isPlaying || isDead)
            return;

        currentHealth -= damage;
        isDead = currentHealth <= 0;

        if (healthBar)
            healthBar.UpdateValue(currentHealth / (float)health);

        if (!isDead)
        {
            anim.SetTrigger("hurt");
            audioService.PlaySfx(hurtSound, 0.7f);
            return;
        }

        isPlaying = false;
        anim.SetBool("isDead", true);
        anim.enabled = false;
        DisableCombatColliders();
        deathSequence.Begin(CreateDeathSettings());
        stateMachine.ChangeState(deathState);
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
        isPlaying = false;
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

    private bool CanRunCombat()
    {
        return isPlaying && !isDead && gameSession.State == GameManager.GameState.Playing && !gameSession.Player.isFinish;
    }

    private void EnterWaiting()
    {
        moving = false;
        isRunning = false;
        stateTimer = Random.Range(minDelay, maxDelay);
    }

    private void TickWaiting(float deltaTime)
    {
        if (!CanRunCombat())
            return;

        stateTimer -= deltaTime;
        if (stateTimer <= 0f)
            stateMachine.ChangeState(chargingState);
    }

    private void EnterCharging()
    {
        LookAtPlayer();
        isRunning = true;
        moving = true;
        audioService.PlaySfx(attackSound);
    }

    private void TickCharging()
    {
        if (!CanRunCombat())
            return;

        if ((direction.x > 0f && controller.collisions.right) || (direction.x < 0f && controller.collisions.left))
            stateMachine.ChangeState(stunningState);
    }

    private void EnterStunning()
    {
        moving = false;
        isRunning = false;
        stateTimer = stunningTime;

        CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);
        anim.SetBool("isStunning", true);
        foreach (var sound in hitWallSound)
            audioService.PlaySfx(sound);

        stunningFX.SetActive(true);
        if (hitWallFX)
            Instantiate(hitWallFX, transform.position + Vector3.up * 1.5f, Quaternion.identity);
    }

    private void TickStunning(float deltaTime)
    {
        if (!CanRunCombat())
            return;

        stateTimer -= deltaTime;
        if (stateTimer <= 0f)
            stateMachine.ChangeState(waitingState);
    }

    private void ExitStunning()
    {
        audioService.PlaySfx(detectSound);
        anim.SetBool("isStunning", false);
        stunningFX.SetActive(false);
        LookAtPlayer();
    }

    private void TickDeath(float deltaTime)
    {
        deathSequence.Tick(deltaTime);
    }

    private void LookAtPlayer()
    {
        if ((isFacingRight() && transform.position.x > gameSession.Player.transform.position.x) ||
            (!isFacingRight() && transform.position.x < gameSession.Player.transform.position.x))
        {
            Flip();
        }
    }

    private void Flip()
    {
        direction = -direction;
        transform.rotation = Quaternion.Euler(transform.rotation.x, isFacingRight() ? 180f : 0f, transform.rotation.z);
    }

    private void DisableCombatColliders()
    {
        foreach (var box in GetComponents<BoxCollider2D>())
            box.enabled = false;

        foreach (var circle in GetComponents<CircleCollider2D>())
            circle.enabled = false;
    }

    private BossDeathSequenceSettings CreateDeathSettings()
    {
        return new BossDeathSequenceSettings
        {
            IntroDelay = 1f,
            FirstWaveExplosions = 3,
            SecondWaveExplosions = 4,
            ExplosionInterval = 0.5f,
            BlackScreenShowDuration = 2f,
            BlackScreenHideDuration = 1f,
            EarthQuakeTime = _eqTime,
            EarthQuakeSpeed = _eqSpeed,
            EarthQuakeSize = _eqSize,
            ExplosionPrefab = dieExplosionFX,
            ExplosionArea = dieExplosionSize,
            DeathSound = deadSound,
            ExplosionSound = dieExplosionSound
        };
    }

    private sealed class WaitingState : IEnemyState
    {
        private readonly BOSS_HOGRIDER owner;

        public WaitingState(BOSS_HOGRIDER owner)
        {
            this.owner = owner;
        }

        public void Enter()
        {
            owner.EnterWaiting();
        }

        public void Tick(float deltaTime)
        {
            owner.TickWaiting(deltaTime);
        }

        public void Exit()
        {
        }
    }

    private sealed class ChargingState : IEnemyState
    {
        private readonly BOSS_HOGRIDER owner;

        public ChargingState(BOSS_HOGRIDER owner)
        {
            this.owner = owner;
        }

        public void Enter()
        {
            owner.EnterCharging();
        }

        public void Tick(float deltaTime)
        {
            owner.TickCharging();
        }

        public void Exit()
        {
        }
    }

    private sealed class StunningState : IEnemyState
    {
        private readonly BOSS_HOGRIDER owner;

        public StunningState(BOSS_HOGRIDER owner)
        {
            this.owner = owner;
        }

        public void Enter()
        {
            owner.EnterStunning();
        }

        public void Tick(float deltaTime)
        {
            owner.TickStunning(deltaTime);
        }

        public void Exit()
        {
            owner.ExitStunning();
        }
    }

    private sealed class DeathState : IEnemyState
    {
        private readonly BOSS_HOGRIDER owner;

        public DeathState(BOSS_HOGRIDER owner)
        {
            this.owner = owner;
        }

        public void Enter()
        {
        }

        public void Tick(float deltaTime)
        {
            owner.TickDeath(deltaTime);
        }

        public void Exit()
        {
        }
    }
}
