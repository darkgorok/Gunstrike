using UnityEngine;
using VContainer;

public class BOSS_FLYING_BOMBER : BossManager, ICanTakeDamage, IListener
{
    public float flyingSpeed = 3;
    [Range(1, 1000)]
    public int health = 500;
    [ReadOnly] public int currentHealth;
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    protected HealthBarEnemyNew healthBar;

    [Header("EARTH QUAKE")]
    public float _eqTime = 0.3f;
    public float _eqSpeed = 60;
    public float _eqSize = 1.5f;

    [Header("SOUND")]
    public AudioClip attackSound;
    public AudioClip deadSound;
    public AudioClip hurtSound;

    [HideInInspector]
    public bool isDead = false;

    private Animator anim;
    [ReadOnly] public bool moving = false;
    [ReadOnly] public bool isPlaying = false;

    [Header("*** FLYING BOMBER ***")]
    public float localLeftPosX = -5f;
    public float localRightPosX = 5f;
    public GameObject bombObj;
    public int totalBombInARow = 8;
    public Transform throwPoint;

    [Header("EFFECT WHEN DIE")]
    public GameObject dieExplosionFX;
    public Vector2 dieExplosionSize = new Vector2(2, 3);
    public AudioClip dieExplosionSound;

    private Vector2 leftPos;
    private Vector2 rightPos;
    [ReadOnly] public float distance2bombs;
    [ReadOnly] public Vector2 _direction;

    private float lastThrowPosX;
    private bool isBombRunActive;
    private int bombCounter;
    private int misspoint;

    private readonly EnemyStateMachine stateMachine = new EnemyStateMachine();
    private PatrolState patrolState;
    private DeathState deathState;
    private BossDeathSequence deathSequence;

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
        patrolState = new PatrolState(this);
        deathState = new DeathState(this);
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);

        currentHealth = health;
        leftPos = transform.position + Vector3.right * localLeftPosX;
        rightPos = transform.position + Vector3.right * localRightPosX;
        _direction = isFacingRight() ? Vector2.right : Vector2.left;

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
        if (!isPlaying && !isDead)
            return;

        stateMachine.Tick(Time.deltaTime);
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
        stateMachine.ChangeState(patrolState);
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

        anim.SetBool("isDead", true);
        anim.enabled = false;
        isPlaying = false;
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

    private void TickPatrol(float deltaTime)
    {
        if (!CanRunCombat())
            return;

        transform.Translate(flyingSpeed * deltaTime * _direction.x, 0f, 0f, Space.World);
        TickBombRun();

        if (isFacingRight() && transform.position.x >= rightPos.x)
        {
            Flip();
            BeginBombRun();
        }
        else if (!isFacingRight() && transform.position.x <= leftPos.x)
        {
            Flip();
            BeginBombRun();
        }
    }

    private void TickDeath(float deltaTime)
    {
        deathSequence.Tick(deltaTime);
    }

    private void BeginBombRun()
    {
        ResetThrowBomb();
        distance2bombs = Vector2.Distance(leftPos, rightPos) / Mathf.Max(1, totalBombInARow - 1);
        lastThrowPosX = transform.position.x - (_direction.x * distance2bombs);
        isBombRunActive = true;
    }

    private void TickBombRun()
    {
        if (!isBombRunActive)
            return;

        if (bombCounter >= totalBombInARow)
        {
            isBombRunActive = false;
            return;
        }

        if (Mathf.Abs(lastThrowPosX - transform.position.x) < distance2bombs)
            return;

        if (bombCounter != misspoint)
        {
            audioService.PlaySfx(attackSound);
            Instantiate(bombObj, throwPoint.position, Quaternion.identity).transform.right = transform.right + Vector3.down * 0.3f;
        }

        bombCounter++;
        lastThrowPosX = transform.position.x;
    }

    private void ResetThrowBomb()
    {
        bombCounter = 0;
        misspoint = Random.Range(0, Mathf.Max(1, totalBombInARow));
    }

    private void Flip()
    {
        _direction = -_direction;
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

    private sealed class PatrolState : IEnemyState
    {
        private readonly BOSS_FLYING_BOMBER owner;

        public PatrolState(BOSS_FLYING_BOMBER owner)
        {
            this.owner = owner;
        }

        public void Enter()
        {
            owner.moving = true;
        }

        public void Tick(float deltaTime)
        {
            owner.TickPatrol(deltaTime);
        }

        public void Exit()
        {
            owner.moving = false;
        }
    }

    private sealed class DeathState : IEnemyState
    {
        private readonly BOSS_FLYING_BOMBER owner;

        public DeathState(BOSS_FLYING_BOMBER owner)
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

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(leftPos, 0.3f);
            Gizmos.DrawWireSphere(rightPos, 0.3f);
            Gizmos.DrawLine(leftPos, rightPos);

            var distance = Vector2.Distance(leftPos, rightPos) / Mathf.Max(1, totalBombInARow - 1);
            for (int i = 0; i < totalBombInARow; i++)
            {
                Gizmos.DrawSphere(transform.position + Vector3.right * localLeftPosX + Vector3.right * i * distance, 0.2f);
            }
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position + Vector3.right * localLeftPosX, 0.3f);
            Gizmos.DrawWireSphere(transform.position + Vector3.right * localRightPosX, 0.3f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * localRightPosX);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * localLeftPosX);

            var distance = Vector2.Distance(transform.position + Vector3.right * localLeftPosX, transform.position + Vector3.right * localRightPosX) / Mathf.Max(1, totalBombInARow - 1);
            for (int i = 0; i < totalBombInARow; i++)
            {
                Gizmos.DrawSphere(transform.position + Vector3.right * localLeftPosX + Vector3.right * i * distance, 0.2f);
            }
        }
    }
}
