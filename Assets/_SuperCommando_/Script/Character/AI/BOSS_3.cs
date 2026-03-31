using UnityEngine;
using VContainer;

public class BOSS_3 : MonoBehaviour, ICanTakeDamage
{
    public bool isBoss = false;
    [Range(10, 500)] public int health = 500;
    [ReadOnly] public int currentHealth;
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    protected HealthBarEnemyNew healthBar;

    public AudioClip attackSound;
    public AudioClip deadSound;
    public AudioClip hurtSound;
    public GameObject deadFX;

    public float speedFly = 3f;
    public float speedAttack;
    public float attackMin = 5f;
    public float attackMax = 10f;
    public Transform[] PointBackUps;

    [Header("damage")]
    public int DamageToPlayer;
    public float rateDamage = 0.2f;
    public Vector2 pushPlayer = new Vector2(0, 10);
    public bool canBeKillOnHead = false;

    [Header("BLINKING")]
    public float blinking = 1.5f;
    public SpriteRenderer characterImage;
    public Material whiteMaterial;

    [HideInInspector] public GameObject saveOwner;

    private Animator anim;
    private Rigidbody2D rig;
    private Transform pointBackUp;
    private Transform currentPatrolPoint;
    private bool attack;
    private bool backup;
    private float oldPos;
    private float currentPos;
    private bool isDead;
    private float nextDamage;
    private Material objMat;
    private float attackTimer = -1f;
    private float disableTimer = -1f;
    private float blinkTimer = -1f;
    private float nextBlinkToggle;
    private bool blinkUsingWhite;

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
    }

    private void Start()
    {
        currentPatrolPoint = PointBackUps[Random.Range(0, PointBackUps.Length)];
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        objMat = characterImage.material;

        currentHealth = health;
        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);

        oldPos = currentPos = transform.position.x;
        saveOwner = Instantiate(transform.root.gameObject, transform.root.position, Quaternion.identity);
        saveOwner.SetActive(false);
    }

    public void Play()
    {
        ScheduleNextAttack();
    }

    private void Update()
    {
        TickTimers();

        if (isDead || gameSession.Player == null)
            return;

        if (attack)
        {
            transform.position = Vector2.MoveTowards(transform.position, gameSession.Player.transform.position, speedAttack * Time.deltaTime);
            if (Vector2.Distance(transform.position, gameSession.Player.transform.position) < 0.1f || gameSession.State == GameManager.GameState.Dead)
                BackUp();
        }
        else if (backup)
        {
            transform.position = Vector2.MoveTowards(transform.position, pointBackUp.position, speedFly * Time.deltaTime * 2f);
            if (Vector2.Distance(transform.position, pointBackUp.position) < 0.1f)
            {
                backup = false;
                ScheduleNextAttack();
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, currentPatrolPoint.position, speedFly * Time.deltaTime / 2f);
            if (Vector2.Distance(transform.position, currentPatrolPoint.position) < 0.1f)
                currentPatrolPoint = PointBackUps[Random.Range(0, PointBackUps.Length)];
        }

        currentPos = transform.position.x;
        if (currentPos < oldPos)
            transform.localScale = new Vector3(1, 1, 1);
        else if (currentPos > oldPos)
            transform.localScale = new Vector3(-1, 1, 1);

        oldPos = currentPos;
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        isDead = currentHealth <= 0;
        if (healthBar)
            healthBar.UpdateValue(currentHealth / (float)health);

        if (isDead)
        {
            audioService.PlaySfx(deadSound);
            anim.SetTrigger("Dead");
            DisableAllColliders();
            rig.isKinematic = false;
            rig.AddForce(new Vector2(0, 200));
            attack = false;
            backup = false;
            attackTimer = -1f;
            blinkTimer = -1f;
            characterImage.material = objMat;

            if (isBoss)
            {
                gameSession.MissionStarCollected = 3;
                gameSession.GameFinish();
            }
            else
            {
                disableTimer = 2f;
                var spawnItem = GetComponent<EnemySpawnItem>();
                if (spawnItem != null)
                    spawnItem.SpawnItem();
            }
        }
        else
        {
            StartBlink();
            audioService.PlaySfx(hurtSound);
        }

        BackUp();
    }

    private void TickTimers()
    {
        if (attackTimer >= 0f)
        {
            if (gameSession.State == GameManager.GameState.Playing)
                attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                attackTimer = -1f;
                attack = true;
                audioService.PlaySfx(attackSound);
            }
        }

        if (disableTimer >= 0f)
        {
            disableTimer -= Time.deltaTime;
            if (disableTimer <= 0f)
            {
                disableTimer = -1f;
                DisableBoss();
            }
        }

        if (blinkTimer >= 0f)
        {
            blinkTimer -= Time.deltaTime;
            nextBlinkToggle -= Time.deltaTime;

            if (nextBlinkToggle <= 0f)
            {
                blinkUsingWhite = !blinkUsingWhite;
                characterImage.material = blinkUsingWhite ? whiteMaterial : objMat;
                nextBlinkToggle = 0.2f;
            }

            if (blinkTimer <= 0f)
            {
                blinkTimer = -1f;
                blinkUsingWhite = false;
                characterImage.material = objMat;
            }
        }
    }

    private void ScheduleNextAttack()
    {
        attack = false;
        attackTimer = Random.Range(attackMin, attackMax);
    }

    private void BackUp()
    {
        attack = false;
        backup = true;
        pointBackUp = PointBackUps[Random.Range(0, PointBackUps.Length)];
    }

    private void StartBlink()
    {
        blinkTimer = blinking;
        nextBlinkToggle = 0f;
        blinkUsingWhite = false;
    }

    private void DisableAllColliders()
    {
        foreach (var box in GetComponents<BoxCollider2D>())
            box.enabled = false;

        foreach (var cir in GetComponents<CircleCollider2D>())
            cir.enabled = false;
    }

    private void DisableBoss()
    {
        if (deadFX)
            Instantiate(deadFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        var player = other.GetComponent<Player>();
        if (player == null || !player.isPlaying)
            return;

        if (Time.time < nextDamage + rateDamage)
            return;

        nextDamage = Time.time;

        float facingDirectionX = Mathf.Sign(player.transform.position.x - transform.position.x);
        float facingDirectionY = Mathf.Sign(player.velocity.y);

        player.SetForce(new Vector2(
            Mathf.Clamp(Mathf.Abs(player.velocity.x), 10, 15) * facingDirectionX,
            Mathf.Clamp(Mathf.Abs(player.velocity.y), 5, 15) * facingDirectionY * -1));

        if (DamageToPlayer == 0)
            return;

        player.TakeDamage(DamageToPlayer, Vector2.zero, gameObject, Vector2.zero);
    }
}
