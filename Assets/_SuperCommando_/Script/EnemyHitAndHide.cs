using UnityEngine;
using VContainer;

[RequireComponent(typeof(Controller2D))]
public class EnemyHitAndHide : MonoBehaviour, ICanTakeDamage
{
    private enum AttackPhase
    {
        Idle,
        Showing,
        Hiding,
        Cooldown
    }

    public DIEBEHAVIOR dieBehavior;
    public float showRandomMin = 1;
    public float showRandomMax = 2;
    public Projectile projectile;
    public Transform throwPoint;
    public GameObject DestroyEffect;
    public float destroyTime = 1.5f;

    [Header("Health")]
    [Range(0, 100)]
    public float health = 50;
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    [ReadOnly] public HealthBarEnemyNew healthBar;

    public AudioClip soundHit, soundDead, soundThrow;

    [HideInInspector] public Controller2D controller;
    [HideInInspector] protected Vector3 velocity;

    private CheckTargetHelper checkTargetHelper;
    private Animator anim;
    private Collider2D hitCollider;
    private bool isDead;
    private float currentHealth;
    private float phaseTimer;
    private float destroyTimer = -1f;
    private AttackPhase attackPhase = AttackPhase.Idle;
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

    public bool isFacingRight()
    {
        return transform.rotation.y == 0;
    }

    private void OnEnable()
    {
        attackPhase = AttackPhase.Idle;
        phaseTimer = 0f;
    }

    private void Start()
    {
        controller = GetComponent<Controller2D>();
        currentHealth = health;
        checkTargetHelper = GetComponent<CheckTargetHelper>();
        anim = GetComponent<Animator>();
        hitCollider = GetComponent<Collider2D>();
        hitCollider.enabled = false;

        HealthBarEnemyNew healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);
    }

    private void Update()
    {
        if (destroyTimer >= 0f)
        {
            destroyTimer -= Time.deltaTime;
            if (destroyTimer <= 0f)
                DestroyObject();
        }

        if (isDead)
            return;

        bool isDetectPlayer = checkTargetHelper.CheckTarget();
        switch (attackPhase)
        {
            case AttackPhase.Idle:
                if (isDetectPlayer)
                {
                    attackPhase = AttackPhase.Showing;
                    phaseTimer = 0.5f;
                    anim.SetBool("showUp", true);
                    hitCollider.enabled = true;
                }
                break;
            case AttackPhase.Showing:
                phaseTimer -= Time.deltaTime;
                if (phaseTimer > 0f)
                    break;

                AlignToPlayer();
                if (isDead)
                    return;

                anim.SetTrigger("throw");
                attackPhase = AttackPhase.Hiding;
                phaseTimer = 1.5f;
                break;
            case AttackPhase.Hiding:
                phaseTimer -= Time.deltaTime;
                if (phaseTimer > 0f)
                    break;

                anim.SetBool("showUp", false);
                hitCollider.enabled = false;
                attackPhase = AttackPhase.Cooldown;
                phaseTimer = Random.Range(showRandomMin, showRandomMax);
                break;
            case AttackPhase.Cooldown:
                phaseTimer -= Time.deltaTime;
                if (phaseTimer <= 0f)
                    attackPhase = AttackPhase.Idle;
                break;
        }
    }

    private void LateUpdate()
    {
        velocity.y += -35 * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime, false);
    }

    public void AnimThrow()
    {
        if (isDead || gameSession?.Player == null)
            return;

        audioService?.PlaySfx(soundThrow);
        Projectile spawnedProjectile = Instantiate(projectile, throwPoint.position, Quaternion.identity);
        Vector2 direction = gameSession.Player.transform.position + Vector3.up - throwPoint.position;
        spawnedProjectile.Initialize(gameObject, direction, Vector2.zero, false);
    }

    private void AlignToPlayer()
    {
        Player player = gameSession?.Player;
        if (player == null)
            return;

        bool shouldFlip =
            (isFacingRight() && transform.position.x > player.transform.position.x) ||
            (!isFacingRight() && transform.position.x < player.transform.position.x);

        if (shouldFlip)
            Flip();
    }

    private void Flip()
    {
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, isFacingRight() ? 180 : 0, transform.rotation.z));
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (healthBar)
            healthBar.UpdateValue(currentHealth / health);

        if (currentHealth > 0)
        {
            audioService?.PlaySfx(soundHit);
            return;
        }

        isDead = true;
        audioService?.PlaySfx(soundDead);

        Rigidbody2D rig = gameObject.AddComponent<Rigidbody2D>();
        rig.isKinematic = false;
        rig.gravityScale = 2;
        rig.linearVelocity = new Vector2(0, 5);

        Collider2D currentCollider = GetComponent<Collider2D>();
        if (currentCollider != null)
            currentCollider.enabled = false;

        if (dieBehavior == DIEBEHAVIOR.BLOWUP)
        {
            if (DestroyEffect != null)
                SpawnSystemHelper.GetNextObject(DestroyEffect, true).transform.position = transform.position;

            DestroyObject();
            return;
        }

        if (dieBehavior == DIEBEHAVIOR.FALLOUT)
        {
            controller.HandlePhysic = false;
            velocity = new Vector2(0, 8);
        }

        destroyTimer = 1f;
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}
