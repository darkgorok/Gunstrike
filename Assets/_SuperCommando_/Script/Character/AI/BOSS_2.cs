/*
 * BOSS ROOT GAMEOBJET DON'T CHILD ANY OTHER OBJECTS
 */

using UnityEngine;
using VContainer;

public class BOSS_2 : MonoBehaviour, ICanTakeDamage
{
    public bool isBoss = false;

    [Range(10, 500)] public int health = 500;
    [ReadOnly] public int currentHealth;

    public AudioClip deadSound;
    public AudioClip throwSound;
    public AudioClip hurtSound;

    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    protected HealthBarEnemyNew healthBar;

    public GameObject deadFX;

    [Header("Attack")]
    public GameObject Stone;
    public Transform attackPoint;
    public float MinAttackTime = 2f;
    public float MaxAttackTime = 4f;

    [Header("BLINKING")]
    public float blinking = 1.5f;
    public SpriteRenderer characterImage;
    public Material whiteMaterial;

    [HideInInspector] public GameObject saveOwner;

    private Animator anim;
    private Rigidbody2D rig;
    private Material objMat;
    private bool isDead;
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
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        currentHealth = health;
        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);

        saveOwner = Instantiate(transform.root.gameObject, transform.root.position, Quaternion.identity);
        saveOwner.SetActive(false);

        objMat = characterImage.material;
    }

    private void Update()
    {
        TickTimers();

        if (isDead || gameSession.Player == null)
            return;

        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * (transform.position.x > gameSession.Player.transform.position.x ? 1 : -1),
            transform.localScale.y,
            transform.localScale.z);
    }

    private void Play()
    {
        ScheduleNextAttack();
    }

    public void ThrowStone()
    {
        audioService.PlaySfx(throwSound);
        Instantiate(Stone, attackPoint.position, Quaternion.identity);
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
            rig.isKinematic = true;
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
    }

    private void TickTimers()
    {
        if (attackTimer >= 0f)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                attackTimer = -1f;
                if (!isDead && gameSession.State == GameManager.GameState.Playing)
                {
                    anim.SetTrigger("Attack");
                    ScheduleNextAttack();
                }
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
        attackTimer = Random.Range(MinAttackTime, MaxAttackTime);
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
}
