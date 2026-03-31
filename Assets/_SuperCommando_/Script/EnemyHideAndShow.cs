using UnityEngine;
using VContainer;

[RequireComponent(typeof(CheckTargetHelper))]
public class EnemyHideAndShow : MonoBehaviour, ICanTakeDamage
{
    [Range(0, 1000)]
    public int health = 100;
    [Space]
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    public GameObject hitFX;

    [ReadOnly] public CheckTargetHelper checkTarget;

    protected EnemyThrowAttack throwAttack;
    protected HealthBarEnemyNew healthBar;

    private Animator anim;
    private bool isDead;
    private bool isDetectedPlayer;
    private bool isAttackLoopStarted;
    private int currentHealth;
    private float showDelayRemaining = -1f;
    private float attackIntervalRemaining = -1f;
    private float destroyDelayRemaining = -1f;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IGameSessionService gameSession)
    {
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    public bool isFacingRight()
    {
        return transform.rotation.y == 0;
    }

    private void Start()
    {
        currentHealth = health;
        throwAttack = GetComponent<EnemyThrowAttack>();
        checkTarget = GetComponent<CheckTargetHelper>();
        anim = GetComponent<Animator>();

        HealthBarEnemyNew healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);
    }

    private void Update()
    {
        if (destroyDelayRemaining >= 0f)
        {
            destroyDelayRemaining -= Time.deltaTime;
            if (destroyDelayRemaining <= 0f)
                Destroy(gameObject);
        }

        if (isDead)
            return;

        if (!isDetectedPlayer && checkTarget.CheckTarget(isFacingRight() ? 1 : -1))
        {
            isDetectedPlayer = true;
            isAttackLoopStarted = true;
            showDelayRemaining = 1f;
            attackIntervalRemaining = -1f;
            anim.SetTrigger("show");
        }

        if (!isAttackLoopStarted)
            return;

        if (showDelayRemaining >= 0f)
        {
            showDelayRemaining -= Time.deltaTime;
            if (showDelayRemaining > 0f)
                return;

            showDelayRemaining = -1f;
            attackIntervalRemaining = 0f;
        }

        attackIntervalRemaining -= Time.deltaTime;
        if (attackIntervalRemaining > 0f)
            return;

        attackIntervalRemaining = 0.1f;
        CheckTargetAndFlip();
        if (throwAttack.AllowAction() && throwAttack.CheckPlayer())
        {
            throwAttack.Action();
            anim.SetTrigger("throw");
        }
    }

    private void CheckTargetAndFlip()
    {
        Player player = gameSession?.Player;
        if (player == null)
            return;

        bool shouldFlip =
            Mathf.Abs(transform.position.x - player.transform.position.x) > 0.1f &&
            ((isFacingRight() && transform.position.x > player.transform.position.x) ||
             (!isFacingRight() && transform.position.x < player.transform.position.x));

        if (shouldFlip)
            Flip();
    }

    private void Flip()
    {
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, isFacingRight() ? 180 : 0, transform.rotation.z));
    }

    public void AnimThrow()
    {
        CheckTargetDirection();
        throwAttack.Throw(isFacingRight(), lookAtPlayerDirection);
    }

    private Vector2 lookAtPlayerDirection;

    private void CheckTargetDirection()
    {
        if (gameSession?.Player == null)
            return;

        RaycastHit2D hitPlayer = Physics2D.CircleCast(transform.position, 8, Vector2.zero, 0, gameSession.PlayerLayer);
        if (!hitPlayer)
            return;

        lookAtPlayerDirection = hitPlayer.transform.position - transform.position;
        lookAtPlayerDirection.Normalize();
    }

    public virtual void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        if (hitFX)
        {
            SpawnSystemHelper.GetNextObject(hitFX, true).transform.position =
                hitPoint + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
        }

        if (healthBar)
            healthBar.UpdateValue(currentHealth / (float)health);

        if (currentHealth <= 0)
        {
            isDead = true;
            isAttackLoopStarted = false;
            showDelayRemaining = -1f;
            attackIntervalRemaining = -1f;
            destroyDelayRemaining = 3f;
            anim.SetTrigger("die");
            return;
        }

        anim.SetTrigger("hurt");
    }
}
