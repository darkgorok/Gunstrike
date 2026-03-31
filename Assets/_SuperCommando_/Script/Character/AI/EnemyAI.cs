using UnityEngine;
using VContainer;

[RequireComponent(typeof(Controller2D))]
public class EnemyAI : MonoBehaviour, ICanTakeDamage
{
    [Header("Behavior")]
    public DIEBEHAVIOR dieBehavior;
    public float gravity = 35f;
    [Tooltip("allow push the enemy back when hit by player")]
    public bool pushEnemyBack = true;

    [Header("Moving")]
    public float moveSpeed = 3f;
    public bool ignoreCheckGroundAhead = false;
    public GameObject DestroyEffect;
    public bool moveFastWhenDetectPlayer = false;
    public float movingDetectPlayerDistance = 5f;
    public float moveFastMultiple = 2f;

    public enum HealthType { HitToKill, HealthAmount, Immortal }

    [Header("Health")]
    public HealthType healthType;
    public int maxHitToKill = 1;
    [HideInInspector] public int currentHitLeft;
    public float health;
    public int pointToGivePlayer;
    public GameObject HurtEffect;

    [Header("Sound")]
    public AudioClip hurtSound;
    [Range(0, 1)] public float hurtSoundVolume = 0.5f;
    public AudioClip deadSound;
    [Range(0, 1)] public float deadSoundVolume = 0.5f;

    [Header("Projectile")]
    public bool isUseProjectile;
    public LayerMask shootableLayer;
    public Transform PointSpawn;
    public Projectile projectile;
    public AudioClip fireSound;
    public float fireRate = 1f;
    public float detectDistance = 10f;

    public bool isPlaying { get; set; }
    public bool isSocking { get; set; }
    public bool isDead { get; set; }

    [HideInInspector] public Controller2D controller;
    protected float velocityXSmoothing = 0f;
    protected Vector3 velocity;
    protected IAudioService AudioService => audioService;
    protected IGameSessionService GameSession => gameSession;

    private readonly EnemyStateMachine stateMachine = new EnemyStateMachine();
    private ActiveState activeState;
    private KnockbackState knockbackState;
    private DeathState deathState;

    private Vector2 pushForce;
    private Vector2 direction;
    private float currentHealth;
    private float fireCooldown;
    private float knockbackDuration;
    private float destroyDelayRemaining = -1f;
    private Vector2 originalPos;
    private bool deathHandled;

    private IAudioService audioService;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IAudioService audioService, IGameSessionService gameSession)
    {
        this.audioService = audioService;
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        activeState = new ActiveState(this);
        knockbackState = new KnockbackState(this);
        deathState = new DeathState(this);
    }

    public virtual void Start()
    {
        controller = GetComponent<Controller2D>();
        direction = Vector2.left;
        fireCooldown = fireRate;
        currentHealth = health;
        currentHitLeft = maxHitToKill;
        originalPos = transform.position;

        isPlaying = true;
        isSocking = false;
        isDead = false;
        deathHandled = false;
        stateMachine.ChangeState(activeState);
    }

    public virtual void Update()
    {
        if (gameSession.State == GameManager.GameState.Finish)
        {
            enabled = false;
            return;
        }

        stateMachine.Tick(Time.deltaTime);
        if (!ReferenceEquals(stateMachine.CurrentState, activeState))
            return;

        fireCooldown -= Time.deltaTime;

        if ((direction.x > 0f && controller.collisions.right) ||
            (direction.x < 0f && controller.collisions.left) ||
            (!ignoreCheckGroundAhead && !controller.isGrounedAhead(direction.x > 0f) && controller.collisions.below))
        {
            direction = -direction;
            velocity.x = 0f;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        if (!isUseProjectile)
            return;

        var position = PointSpawn != null ? PointSpawn.position : transform.position;
        var hit = Physics2D.Raycast(position, direction, detectDistance, shootableLayer);
        if (hit && hit.collider.gameObject.GetComponent<Player>() != null)
            FireProjectile();
    }

    public virtual void LateUpdate()
    {
        if (gameSession.State != GameManager.GameState.Playing)
            return;

        if (ReferenceEquals(stateMachine.CurrentState, deathState))
        {
            if (isDead && dieBehavior == DIEBEHAVIOR.FALLOUT)
            {
                velocity.y += -35f * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime, false);
            }

            return;
        }

        if (!ReferenceEquals(stateMachine.CurrentState, activeState))
            return;

        float targetVelocityX = direction.x * moveSpeed;
        if (moveFastWhenDetectPlayer)
        {
            if (Physics2D.Linecast(
                    transform.position + Vector3.left * movingDetectPlayerDistance + Vector3.up * 0.5f,
                    transform.position + Vector3.right * movingDetectPlayerDistance + Vector3.up * 0.5f,
                    gameSession.PlayerLayer))
            {
                targetVelocityX *= moveFastMultiple;
            }
        }

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, controller.collisions.below ? 0.1f : 0.2f);
        velocity.y += -gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime, false);

        if (controller.collisions.above || controller.collisions.below)
            velocity.y = 0f;
    }

    public void SetForce(float x, float y)
    {
        velocity = new Vector3(x, y, 0f);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (healthType == HealthType.Immortal || isDead)
            return;

        if (instigator.GetComponent<Grenade>())
        {
            if (HurtEffect)
                SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = instigator.transform.position;

            isDead = true;
            HitEvent();
            return;
        }

        pushForce = force;
        if (HurtEffect)
            SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = instigator.transform.position;

        if (healthType == HealthType.HitToKill)
        {
            currentHitLeft--;
            if (currentHitLeft <= 0)
                isDead = true;
        }
        else if (healthType == HealthType.HealthAmount)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
                isDead = true;
        }

        if (instigator.GetComponent<Block>() != null)
            isDead = true;

        HitEvent();
    }

    protected virtual void HitEvent()
    {
        audioService.PlaySfx(hurtSound, hurtSoundVolume);
        if (HurtEffect)
            SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = transform.position;

        BeginPushBack(0.35f);
    }

    protected virtual void Dead()
    {
        if (deathHandled)
            return;

        stateMachine.ChangeState(deathState);
    }

    protected virtual void OnRespawn()
    {
    }

    protected void BeginPushBack(float duration)
    {
        knockbackDuration = duration;
        stateMachine.ChangeState(knockbackState);
    }

    protected bool IsActiveState()
    {
        return ReferenceEquals(stateMachine.CurrentState, activeState);
    }

    protected virtual void OnEnterActiveState()
    {
        isPlaying = true;
        isSocking = false;
    }

    protected virtual void OnEnterKnockbackState(float duration)
    {
        isPlaying = false;
        isSocking = true;
        SetForce(gameSession.Player.transform.localScale.x * pushForce.x, pushForce.y);
    }

    protected virtual void OnExitKnockbackState()
    {
        SetForce(0f, 0f);
        isSocking = false;
        isPlaying = true;
    }

    protected virtual void OnDeathStateTick(float deltaTime)
    {
        if (destroyDelayRemaining < 0f)
            return;

        destroyDelayRemaining -= deltaTime;
        if (destroyDelayRemaining <= 0f)
            DestroyObject();
    }

    private void FireProjectile()
    {
        if (fireCooldown > 0f)
            return;

        fireCooldown = fireRate;
        var spawnedProjectile = Instantiate(projectile, PointSpawn.position, Quaternion.identity);
        spawnedProjectile.Initialize(gameObject, direction, Vector2.zero, false);
        audioService.PlaySfx(fireSound);
    }

    private void EnterDeathState()
    {
        if (deathHandled)
            return;

        deathHandled = true;
        isPlaying = false;
        audioService.PlaySfx(deadSound, deadSoundVolume);

        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
            spawnItem.SpawnItem();

        foreach (var box in GetComponents<BoxCollider2D>())
            box.enabled = false;

        foreach (var circle in GetComponents<CircleCollider2D>())
            circle.enabled = false;

        if (dieBehavior == DIEBEHAVIOR.BLOWUP)
        {
            if (DestroyEffect != null)
                SpawnSystemHelper.GetNextObject(DestroyEffect, true).transform.position = transform.position;

            destroyDelayRemaining = 0f;
            return;
        }

        if (dieBehavior == DIEBEHAVIOR.FALLOUT)
        {
            controller.HandlePhysic = false;
            velocity = new Vector2(0f, 8f);
            destroyDelayRemaining = 1f;
            return;
        }

        destroyDelayRemaining = 1f;
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }

    public void OnDrawGizmosSelected()
    {
        if (isUseProjectile)
        {
            Gizmos.color = Color.blue;
            if (direction.magnitude != 0f)
                Gizmos.DrawRay(PointSpawn.position, direction * detectDistance);
            else
                Gizmos.DrawRay(PointSpawn.position, Vector2.left * detectDistance);
        }

        if (moveFastWhenDetectPlayer)
        {
            Gizmos.DrawLine(
                transform.position + Vector3.left * movingDetectPlayerDistance + Vector3.up * 0.5f,
                transform.position + Vector3.right * movingDetectPlayerDistance + Vector3.up * 0.5f);
        }
    }

    private sealed class ActiveState : IEnemyState
    {
        private readonly EnemyAI owner;

        public ActiveState(EnemyAI owner)
        {
            this.owner = owner;
        }

        public void Enter()
        {
            owner.OnEnterActiveState();
        }

        public void Tick(float deltaTime)
        {
        }

        public void Exit()
        {
        }
    }

    private sealed class KnockbackState : IEnemyState
    {
        private readonly EnemyAI owner;
        private float remaining;

        public KnockbackState(EnemyAI owner)
        {
            this.owner = owner;
        }

        public void Enter()
        {
            remaining = owner.knockbackDuration;
            owner.OnEnterKnockbackState(remaining);
        }

        public void Tick(float deltaTime)
        {
            remaining -= deltaTime;
            if (remaining > 0f)
                return;

            owner.OnExitKnockbackState();
            if (owner.isDead)
                owner.Dead();
            else
                owner.stateMachine.ChangeState(owner.activeState);
        }

        public void Exit()
        {
        }
    }

    private sealed class DeathState : IEnemyState
    {
        private readonly EnemyAI owner;

        public DeathState(EnemyAI owner)
        {
            this.owner = owner;
        }

        public void Enter()
        {
            owner.EnterDeathState();
        }

        public void Tick(float deltaTime)
        {
            owner.OnDeathStateTick(deltaTime);
        }

        public void Exit()
        {
        }
    }
}
