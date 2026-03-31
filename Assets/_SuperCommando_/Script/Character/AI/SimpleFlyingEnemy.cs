using UnityEngine;
using VContainer;

public class SimpleFlyingEnemy : MonoBehaviour, ICanTakeDamage, IListener
{
    public DIEBEHAVIOR dieBehavior;
    public float minX = 3f;
    public float maxX = 3f;
    public float minY = 3f;
    public float maxY = 3f;
    public float speedY = 3f;
    public float speedX = 5f;

    [ReadOnly] public bool isMovingRight = false;
    [ReadOnly] public bool isMovingTop = false;
    [Range(0, 1000)] public int health = 100;
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    public GameObject DestroyEffect;
    public AudioClip soundHit;
    public AudioClip soundDead;

    [Header("Contact Player")]
    public int makeDamage = 30;
    [Tooltip("delay a moment before give next damage to Player")]
    public float rateDamage = 0.2f;

    protected HealthBarEnemyNew healthBar;
    [HideInInspector] protected Vector3 velocity;
    [HideInInspector] public Controller2D controller;

    private float targetR;
    private float targetL;
    private float targetT;
    private float targetB;
    private int currentHealth;
    private bool isPlaying = true;
    private bool isDead = false;
    private bool isStop = false;
    private float nextDamage;
    private float destroyTimer = -1f;

    private IAudioService audioService;

    [Inject]
    public void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        controller = GetComponent<Controller2D>();

        targetR = transform.position.x + maxX;
        targetL = transform.position.x - minX;
        targetT = transform.position.y + maxY;
        targetB = transform.position.y - minY;

        currentHealth = health;
        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);
    }

    private void Update()
    {
        TickDestroyTimer();

        if (isDead)
        {
            if (dieBehavior == DIEBEHAVIOR.FALLOUT)
            {
                velocity.y += -35f * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime, false);
            }

            return;
        }

        if (!isPlaying || isStop)
            return;

        float y = transform.position.y;
        if (isMovingRight)
        {
            transform.Translate(speedX * Time.deltaTime, 0f, 0f, Space.World);
            if (transform.position.x >= targetR)
                isMovingRight = false;
        }
        else
        {
            transform.Translate(-speedX * Time.deltaTime, 0f, 0f, Space.World);
            if (transform.position.x <= targetL)
                isMovingRight = true;
        }

        if ((isFacingRight() && !isMovingRight) || (!isFacingRight() && isMovingRight))
            Flip();

        if (isMovingTop)
        {
            y = Mathf.Lerp(y, targetT, speedY * Time.deltaTime);
            if (Mathf.Abs(y - targetT) < 0.1f)
                isMovingTop = false;
        }
        else
        {
            y = Mathf.Lerp(y, targetB, speedY * Time.deltaTime);
            if (Mathf.Abs(y - targetB) < 0.1f)
                isMovingTop = true;
        }

        transform.position = new Vector2(transform.position.x, y);
        if (healthBar)
        {
            healthBar.transform.localScale = new Vector2(
                transform.localScale.x > 0 ? Mathf.Abs(healthBar.transform.localScale.x) : -Mathf.Abs(healthBar.transform.localScale.x),
                healthBar.transform.localScale.y);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isDead)
            return;

        var player = other.GetComponent<Player>();
        if (player == null || !player.isPlaying)
            return;

        if (player.gameObject.layer == LayerMask.NameToLayer("HidingZone"))
            return;

        if (Time.time < nextDamage + rateDamage)
            return;

        nextDamage = Time.time;
        if (makeDamage == 0)
            return;

        player.TakeDamage(makeDamage, Vector2.zero, gameObject, transform.position);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        if (healthBar)
            healthBar.UpdateValue(currentHealth / (float)health);

        if (currentHealth > 0)
        {
            audioService.PlaySfx(soundHit);
            return;
        }

        isDead = true;
        isPlaying = false;
        audioService.PlaySfx(soundDead);

        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
            spawnItem.SpawnItem();

        if (dieBehavior == DIEBEHAVIOR.BLOWUP)
        {
            if (DestroyEffect != null)
                SpawnSystemHelper.GetNextObject(DestroyEffect, true).transform.position = transform.position;
            DestroyObject();
        }
        else if (dieBehavior == DIEBEHAVIOR.FALLOUT)
        {
            controller.HandlePhysic = false;
            velocity = new Vector2(0f, 8f);
            destroyTimer = 1f;
        }
        else
        {
            destroyTimer = 1f;
        }
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
    }

    public void IOnRespawn()
    {
    }

    public void IOnStopMovingOn()
    {
        var animator = GetComponent<Animator>();
        if (animator != null)
            animator.enabled = false;
        isStop = true;
    }

    public void IOnStopMovingOff()
    {
        var animator = GetComponent<Animator>();
        if (animator != null)
            animator.enabled = true;
        isStop = false;
    }

    public bool isFacingRight()
    {
        return transform.rotation.y == 0;
    }

    private void Flip()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.x, isFacingRight() ? 180f : 0f, transform.rotation.z);
    }

    private void TickDestroyTimer()
    {
        if (destroyTimer < 0f)
            return;

        destroyTimer -= Time.deltaTime;
        if (destroyTimer <= 0f)
        {
            destroyTimer = -1f;
            DestroyObject();
        }
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.DrawWireCube(
                new Vector2((transform.position.x - minX + transform.position.x + maxX) * 0.5f, (transform.position.y - minY + transform.position.y + maxY) * 0.5f),
                new Vector2(minX + maxX, minY + maxY));
        }
    }
}
