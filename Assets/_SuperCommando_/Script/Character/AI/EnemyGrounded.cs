/*
 * ENEMY GROUNDED dung Enemy binh thuong, co nhieu skill tan cong, nhung khong truy duoi Player
 * neu muon su dung Enemy co kha nang truy duoi Player, chon su dung SmartEnemyGrounded
 */

using System.Collections.Generic;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Controller2D))]
public class EnemyGrounded : MonoBehaviour, ICanTakeDamage, IListener
{
    public enum WeaponsType { None, Melee, Fire, Throw, FireProjectileObj, Spawn }

    [Header("Setup")]
    public float gravity = 35f;
    public float moveSpeed = 3f;
    public float waitingTurn = 0.5f;
    public float sockingTime = 0.5f;
    public GameObject BonusItem;
    public GameObject HurtEffect;
    public GameObject DestroyEffect;
    public float destroyTime = 1.5f;
    public int pointToGivePlayer = 100;

    [Header("Behavior")]
    public bool isAllowChasingPlayer = false;
    public bool isChasing { get; set; }
    public bool canBeFallDown = false;
    public DIEBEHAVIOR dieBehavior;

    [Header("Health")]
    [Range(0, 100)] public float health = 50;
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    protected HealthBarEnemyNew healthBar;

    [Header("Patrol")]
    public bool doPatrol = true;
    public float localLimitLeft = 3f;
    public float localLimitRight = 3f;

    [Header("Attack")]
    public WeaponsType attackType;
    public bool attackOnStart = false;
    public CheckTargetHelper checkTargetHelper;
    public float attackRate = 2f;

    [Header("Melee Attack")]
    public Transform meleePoint;
    public float meleeAttackZone = 0.7f;
    public float meleeAttackCheckPlayer = 0.1f;
    public float meleeRate = 2f;
    public int meleeDamage = 20;

    [Header("Range Attack")]
    public Transform rangePoint;
    public Projectile rangeprojectile;
    public int bulletDamage = 50;
    public float bulletSpeed = 10f;

    [Header("Grenade")]
    public float angleThrow = 60f;
    public float throwForce = 300f;
    public Transform throwPosition;
    public GameObject _Grenade;

    [Header("Spawn Obj")]
    public Transform spawnPoint;
    public GameObject spawnObj;
    public float spawnRate = 2f;
    public int maxSpawnActive = 3;

    [Header("Chasing")]
    public float chaseSpeed = 3f;
    public float offsetPlayerY = 0.5f;
    public float finishDistance = 0.5f;

    [Header("Sound")]
    public AudioClip soundMeleeAttack;
    public AudioClip soundRangAttack;
    public AudioClip soundSpawnttack;
    public AudioClip hurtSound;
    [Range(0, 1)] public float hurtSoundVolume = 0.5f;
    public AudioClip deadSound;
    [Range(0, 1)] public float deadSoundVolume = 0.5f;

    public bool isPlaying { get; set; }
    public bool isSocking { get; set; }
    public bool isDead { get; set; }

    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public Controller2D controller;

    private Animator animator;
    private Vector2 direction;
    private float currentHealth;
    private float limitLeft;
    private float limitRight;
    private float attackRateCounter;
    private float waitingTime;
    private float velocityXSmoothing;
    private float directionFace;
    private bool isWaiting;
    private bool isDetectingPlayer;
    private bool isStop;
    private bool detectToWaiting;
    private Vector3 waitingAfterPlayerGoHidingZone;
    private Vector2 pushForce;
    private readonly List<GameObject> activeSpawnedObj = new List<GameObject>();
    private SmartProjectileAttack smartProjectileAttack;
    private bool pendingFaceDirInit = true;
    private float pushBackTimer = -1f;
    private float destroyTimer = -1f;
    private float meleeHitTimer = -1f;
    private float meleeRecoverTimer = -1f;

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
        controller = GetComponent<Controller2D>();
        animator = GetComponent<Animator>();
        if (checkTargetHelper == null)
            checkTargetHelper = GetComponent<CheckTargetHelper>();
        if (attackType == WeaponsType.FireProjectileObj)
            smartProjectileAttack = GetComponent<SmartProjectileAttack>();

        currentHealth = health;
        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);

        if (moveSpeed == 0)
            direction = isFacingRight() ? Vector2.right : Vector2.left;

        attackRateCounter = attackRate;
        limitLeft = transform.position.x - localLimitLeft;
        limitRight = transform.position.x + localLimitRight;

        isPlaying = true;
        isSocking = false;
        isChasing = false;
    }

    private void Update()
    {
        TickTimers();
        if (pendingFaceDirInit)
        {
            pendingFaceDirInit = false;
            controller.collisions.faceDir = -1;
        }

        if (!isPlaying || isStop)
            return;

        HandleAnimation();
        if (healthBar)
        {
            healthBar.transform.localScale = new Vector2(
                transform.localScale.x > 0 ? Mathf.Abs(healthBar.transform.localScale.x) : -Mathf.Abs(healthBar.transform.localScale.x),
                healthBar.transform.localScale.y);
        }

        attackRateCounter -= Time.deltaTime;

        if (!isPlaying || isSocking || !gameSession.Player.isPlaying)
        {
            velocity.x = 0f;
            return;
        }

        if (isWaiting)
        {
            waitingTime += Time.deltaTime;
            if (waitingTime >= waitingTurn)
            {
                isWaiting = false;
                waitingTime = 0f;
                Flip();
            }
        }
        else if ((direction.x > 0f && controller.collisions.right) ||
                 (direction.x < 0f && controller.collisions.left) ||
                 (!canBeFallDown && !controller.isGrounedAhead(velocity.x > 0f) && controller.collisions.below) ||
                 (doPatrol && ((direction.x > 0f && transform.position.x > limitRight) || (direction.x < 0f && transform.position.x < limitLeft))))
        {
            isWaiting = true;
        }

        if (attackOnStart)
        {
            DoAttack();
            return;
        }

        if (attackType == WeaponsType.None || isSocking)
            return;

        var hit = checkTargetHelper.CheckTarget((int)direction.x);
        isDetectingPlayer = hit != null;
        if (hit)
            DoAttack(hit);
    }

    public virtual void LateUpdate()
    {
        if (!isPlaying)
        {
            if (isDead && dieBehavior == DIEBEHAVIOR.FALLOUT)
            {
                velocity.y += -35f * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime, false);
            }
            return;
        }

        if (isStop || gameSession.State != GameManager.GameState.Playing)
            return;

        if (!isPlaying || isSocking || isDetectingPlayer)
        {
            velocity = Vector2.zero;
            return;
        }

        if (!gameSession.Player.isPlaying)
            return;

        if (isChasing && isAllowChasingPlayer)
        {
            if (gameSession.Player.gameObject.layer != LayerMask.NameToLayer("HidingZone"))
            {
                if (Mathf.Abs(Vector3.Distance(transform.position, gameSession.Player.transform.position)) > finishDistance)
                {
                    transform.position = Vector3.MoveTowards(transform.position, gameSession.Player.transform.position + new Vector3(0, offsetPlayerY, 0), chaseSpeed * Time.deltaTime);
                    directionFace = transform.position.x > gameSession.Player.transform.position.x ? -1 : 1;
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * directionFace, transform.localScale.y, transform.localScale.z);
                }
            }
            else
            {
                isChasing = false;
                if (gravity == 0)
                {
                    detectToWaiting = true;
                    waitingAfterPlayerGoHidingZone = gameSession.Player.transform.position + Vector3.up * 5;
                }
                else
                {
                    transform.localScale = new Vector3((velocity.x > 0 ? -1 : 1) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
            }
        }
        else if (detectToWaiting)
        {
            transform.position = Vector3.MoveTowards(transform.position, waitingAfterPlayerGoHidingZone, chaseSpeed * Time.deltaTime);
            if (gameSession.Player.gameObject.layer != LayerMask.NameToLayer("HidingZone"))
            {
                isChasing = true;
                detectToWaiting = false;
            }
        }
        else
        {
            float targetVelocityX = direction.x * moveSpeed;
            velocity.x = isWaiting ? 0f : Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, controller.collisions.below ? 0.1f : 0.2f);
            velocity.y += -gravity * Time.deltaTime;
            if (isStop)
                velocity = Vector2.zero;

            controller.Move(velocity * Time.deltaTime, false);
            if (controller.collisions.above || controller.collisions.below)
                velocity.y = 0f;
        }
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (isDead)
            return;

        pushForce = force;
        if (HurtEffect)
            SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = instigator.transform.position;

        currentHealth -= damage;
        if (healthBar)
            healthBar.UpdateValue(currentHealth / health);

        if (currentHealth <= 0f)
            isDead = true;

        if (instigator && instigator.GetComponent<Block>() != null)
            isDead = true;

        if (isDead)
            Dead();
        else
            HitEvent();
    }

    protected virtual void HitEvent()
    {
        audioService.PlaySfx(hurtSound, hurtSoundVolume);
        if (HurtEffect)
            SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = transform.position;

        animator.SetTrigger("hurt");
        BeginPushBack(sockingTime);
    }

    protected virtual void Dead()
    {
        isPlaying = false;
        audioService.PlaySfx(deadSound, deadSoundVolume);

        if (BonusItem != null)
            Instantiate(BonusItem, transform.position, transform.rotation);

        animator.SetTrigger("die");
        velocity.x = 0f;

        var boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
            boxCollider.enabled = false;

        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
            spawnItem.SpawnItem();

        if (dieBehavior == DIEBEHAVIOR.BLOWUP)
        {
            if (DestroyEffect)
                SpawnSystemHelper.GetNextObject(DestroyEffect, true).transform.position = transform.position + Vector3.up * 0.5f;
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

        var havePathMoving = GetComponent<SimplePathedMoving>();
        if (havePathMoving)
            Destroy(havePathMoving);
    }

    protected virtual void OnRespawn()
    {
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
        if (!this)
            return;

        animator.enabled = false;
        isStop = true;
    }

    public void IOnStopMovingOff()
    {
        if (!this)
            return;

        animator.enabled = true;
        isStop = false;
    }

    public bool isFacingRight()
    {
        return transform.rotation.y == 0;
    }

    public void SetForce(float x, float y)
    {
        velocity = new Vector3(x, y, 0f);
    }

    private void Flip()
    {
        direction = -direction;
        transform.rotation = Quaternion.Euler(transform.rotation.x, isFacingRight() ? 180f : 0f, transform.rotation.z);
    }

    private void DoAttack(GameObject targetHit = null)
    {
        if (attackRateCounter > 0f)
            return;

        switch (attackType)
        {
            case WeaponsType.Fire:
                FireProjectile();
                attackRateCounter = attackRate;
                break;
            case WeaponsType.Throw:
                ThrowGrenade();
                attackRateCounter = attackRate;
                break;
            case WeaponsType.Spawn:
                if (SpawnObject())
                    attackRateCounter = attackRate;
                break;
            case WeaponsType.FireProjectileObj:
                smartProjectileAttack.Shoot(targetHit);
                break;
            case WeaponsType.Melee:
                BeginMeleeAttack(meleeAttackCheckPlayer);
                attackRateCounter = attackRate;
                break;
        }
    }

    private bool SpawnObject()
    {
        for (int i = activeSpawnedObj.Count - 1; i >= 0; i--)
        {
            if (activeSpawnedObj[i] == null || !activeSpawnedObj[i].activeInHierarchy)
                activeSpawnedObj.RemoveAt(i);
        }

        if (activeSpawnedObj.Count >= maxSpawnActive)
            return false;

        animator.SetTrigger("spawn");
        return true;
    }

    private void HandleAnimation()
    {
        animator.SetFloat("speed", Mathf.Abs(velocity.x));
    }

    private void BeginMeleeAttack(float delay)
    {
        animator.SetTrigger("attack");
        isPlaying = false;
        meleeHitTimer = delay;
        meleeRecoverTimer = -1f;
    }

    private void TickTimers()
    {
        if (pushBackTimer >= 0f)
        {
            pushBackTimer -= Time.deltaTime;
            if (pushBackTimer <= 0f)
            {
                pushBackTimer = -1f;
                SetForce(0f, 0f);
                isSocking = false;
                isPlaying = true;
            }
        }

        if (destroyTimer >= 0f)
        {
            destroyTimer -= Time.deltaTime;
            if (destroyTimer <= 0f)
            {
                destroyTimer = -1f;
                DestroyObject();
            }
        }

        if (meleeHitTimer >= 0f)
        {
            meleeHitTimer -= Time.deltaTime;
            if (meleeHitTimer <= 0f)
            {
                ResolveMeleeAttack();
                meleeHitTimer = -1f;
            }
        }

        if (meleeRecoverTimer >= 0f)
        {
            meleeRecoverTimer -= Time.deltaTime;
            if (meleeRecoverTimer <= 0f)
            {
                meleeRecoverTimer = -1f;
                isPlaying = true;
            }
        }
    }

    private void ResolveMeleeAttack()
    {
        if (isSocking)
        {
            isPlaying = true;
            return;
        }

        var hit = Physics2D.CircleCast(meleePoint.position, meleeAttackZone, Vector2.zero, 0, gameSession.PlayerLayer);
        if (!hit)
        {
            isPlaying = true;
            return;
        }

        var damage = (ICanTakeDamage)hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage));
        if (damage == null)
        {
            isPlaying = true;
            return;
        }

        damage.TakeDamage(meleeDamage, Vector2.zero, gameObject, Vector2.zero);
        meleeRecoverTimer = attackRate;
    }

    private void BeginPushBack(float delay)
    {
        isSocking = true;
        SetForce(gameSession.Player.transform.localScale.x * pushForce.x, pushForce.y);

        if (isDead)
        {
            Dead();
            return;
        }

        pushBackTimer = delay;
    }

    private void FireProjectile()
    {
        animator.SetTrigger("throw");
    }

    private void AnimFire()
    {
        var projectile = Instantiate(rangeprojectile, rangePoint.position, Quaternion.identity);
        projectile.Initialize(gameObject, direction, Vector2.zero, false, false, bulletDamage, bulletSpeed);
        audioService.PlaySfx(soundRangAttack);
    }

    private void AnimSpawn()
    {
        activeSpawnedObj.Add(Instantiate(spawnObj, spawnPoint.position, spawnObj.transform.rotation));
        audioService.PlaySfx(soundSpawnttack);
    }

    public void ThrowGrenade()
    {
        if (_Grenade == null)
            return;

        var obj = Instantiate(_Grenade, throwPosition.position, Quaternion.identity);
        obj.transform.rotation = Quaternion.Euler(0f, 0f, transform.localScale.x < 1 ? angleThrow : 180f - angleThrow);
        obj.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(throwForce, 0f));
        animator.SetTrigger("throw");
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (doPatrol)
        {
            if (Application.isPlaying)
            {
                Gizmos.DrawWireSphere(new Vector2(limitLeft, transform.position.y), 0.2f);
                Gizmos.DrawWireSphere(new Vector2(limitRight, transform.position.y), 0.2f);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position + Vector3.right * localLimitRight, 0.2f);
                Gizmos.DrawWireSphere(transform.position - Vector3.right * localLimitLeft, 0.2f);
            }
        }

        if (attackType == WeaponsType.None)
            return;

        if (attackType == WeaponsType.Melee)
        {
            if (meleePoint == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(meleePoint.position, meleeAttackZone);
        }

        if (attackType == WeaponsType.FireProjectileObj)
        {
            if (GetComponent<SmartProjectileAttack>() == null)
                gameObject.AddComponent<SmartProjectileAttack>();
        }
        else if (GetComponent<SmartProjectileAttack>() != null)
        {
            DestroyImmediate(GetComponent<SmartProjectileAttack>());
        }
    }
}
