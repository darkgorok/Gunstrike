using System.Collections.Generic;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Boss1AttackOrder))]
[RequireComponent(typeof(Controller2D))]
public class BOSS_1 : BossManager, ICanTakeDamage, IListener
{
    private enum DeathSequencePhase
    {
        None,
        InitialDelay,
        FirstWave,
        WhiteFlash,
        SecondWave,
        Finish
    }

    private enum DisappearPhase
    {
        None,
        BeforeTeleport,
        AfterTeleport
    }

    private enum FallingObjectPhase
    {
        None,
        Windup,
        Impact,
        Cooldown
    }

    private enum FlyingAttackPhase
    {
        None,
        MoveToPoint,
        WaitBeforeThrowSpread,
        WaitAfterThrowSpread,
        PrepareCharge,
        Charge
    }

    private enum TornadoPhase
    {
        None,
        WaitingForAnim,
        Spawning,
        Cooldown
    }

    private enum BoomerangPhase
    {
        None,
        WaitingForAnim,
        Traveling
    }

    public enum BOSSTYPE { BOSS, SUPER_ENEMY }
    public enum PUNCHTYPE { Auto, OnlySpeedAttack, None }
    public enum DEAD_BEHAIVIOR { FinishLevel, None }

    [Header("SETUP")]
    public BOSSTYPE bossType = BOSSTYPE.BOSS;
    public Sprite bossIcon;
    public DEAD_BEHAIVIOR deadBehavior;
    public float speed = 1f;
    [Range(50, 1000)] public int health = 500;
    [ReadOnly] public int currentHealth;
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    protected HealthBarEnemyNew healthBar;
    public float gravity = 35f;

    [Header("EARTH QUAKE")]
    public float _eqTime = 0.3f;
    public float _eqSpeed = 60f;
    public float _eqSize = 1.5f;

    [Space]
    public Transform centerPoint;
    public GameObject deadFX;

    [Header("SOUND")]
    public AudioClip showupSound;
    public AudioClip attackSound;
    public AudioClip deadSound;
    public AudioClip hurtSound;

    [HideInInspector] public bool isDead;
    [HideInInspector] protected Vector3 velocity;
    [ReadOnly] public bool isPlaying;
    [ReadOnly] public bool isPlayerInRange;

    protected float velocityXSmoothing;

    private Boss1AttackOrder boss1AttackOrder;
    private Controller2D controller;
    private Animator anim;
    private bool moving;
    private float mulSpeed = 1f;
    private CheckTargetHelper checkTargetHelper;
    private Vector2 direction;
    private Material objMat;
    private GhostSprites ghostSprite;

    private float startupTimer = -1f;
    private float idleResumeTimer = -1f;
    private float disableBossTimer = -1f;
    private float blinkTimer = -1f;
    private float nextBlinkToggle;
    private bool blinkUsingWhite;
    private float meleeRecoverTimer = -1f;
    private bool finishTriggered;

    private DeathSequencePhase deathPhase = DeathSequencePhase.None;
    private float deathPhaseTimer = -1f;
    private int deathExplosionsRemaining;

    private DisappearPhase disappearPhase = DisappearPhase.None;
    private float disappearTimer = -1f;

    private float speedAttackTimer = -1f;

    private float superAttackTimer = -1f;
    private int superAttackRemaining;

    private FallingObjectPhase fallingObjectPhase = FallingObjectPhase.None;
    private float fallingObjectTimer = -1f;

    private FlyingAttackPhase flyingAttackPhase = FlyingAttackPhase.None;
    private float flyingAttackTimer = -1f;
    private bool flyingAttackThrowStone;
    private bool flyingAttackSpreadBullet;
    private Vector3 currentFlyingPoint;
    private Vector2 targetFlyingAttack;

    private TornadoPhase tornadoPhase = TornadoPhase.None;
    private float tornadoTimer = -1f;
    private int tornadoRemaining;
    private bool tornadoSpawnAllowed;

    private BoomerangPhase boomerangPhase = BoomerangPhase.None;
    private bool boomerangThrowAllowed;
    private float boomerangAngle;

    private IAudioService audioService;
    private IControllerInputService controllerInputService;
    private IGameplayPresentationService presentationService;
    private IGameSessionService gameSession;
    private IBossHealthbarService bossHealthbarService;

    [Inject]
    public void Construct(IAudioService audioService, IControllerInputService controllerInputService, IGameplayPresentationService presentationService, IGameSessionService gameSession, IBossHealthbarService bossHealthbarService)
    {
        this.audioService = audioService;
        this.controllerInputService = controllerInputService;
        this.presentationService = presentationService;
        this.gameSession = gameSession;
        this.bossHealthbarService = bossHealthbarService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        controller = GetComponent<Controller2D>();
        checkTargetHelper = GetComponent<CheckTargetHelper>();
        anim = GetComponent<Animator>();
        boss1AttackOrder = GetComponent<Boss1AttackOrder>();
        if (characterImage)
            objMat = characterImage.material;

        direction = IsFacingRight() ? Vector2.right : Vector2.left;
        currentHealth = health;

        ghostSprite = GetComponent<GhostSprites>();
        if (ghostSprite != null)
            ghostSprite.allowGhost = false;

        flyPoints = new List<Vector3>();
        foreach (Transform child in flyPointGroup.GetComponentsInChildren<Transform>())
        {
            if (child.position != flyPointGroup.position)
                flyPoints.Add(child.position);
        }
        flyPointGroup.parent = null;

        disappearPoints = new List<Vector3>();
        foreach (Transform child in disappearPointGroup.GetComponentsInChildren<Transform>())
        {
            if (child.position != disappearPointGroup.position)
                disappearPoints.Add(child.position);
        }
        disappearPointGroup.parent = null;
    }

    public override void Play()
    {
        if (isPlaying)
            return;

        isPlaying = true;
        startupTimer = 1f;

        if (bossType == BOSSTYPE.BOSS)
        {
            bossHealthbarService.Init(bossIcon, health);
        }
        else
        {
            var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
            healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
            healthBar.Init(transform, (Vector3)healthBarOffset);
        }

        audioService.PlaySfx(showupSound);
        anim.SetTrigger("showup");
    }

    private void Update()
    {
        TickTimers();
        anim.SetFloat("speed", Mathf.Abs(velocity.x));

        if (isDead || gameSession.State != GameManager.GameState.Playing || gameSession.Player == null ||
            gameSession.Player.isFinish || disapearing || isSuperAttacking || isFallingObjectAttack || isFlyingAttack ||
            isTornadoAttacking || isBoomerangeAttacking)
        {
            velocity.x = 0f;
            if (isSuperAttacking)
                LookAtPlayer();

            return;
        }

        bool allowChasing = true;
        if (moving)
        {
            GameObject hitTarget = checkTargetHelper.CheckTarget();
            if (hitTarget != null)
            {
                isPlayerInRange = true;
                if (Mathf.Abs(hitTarget.transform.position.x - transform.position.x) < 0.3f && !gameSession.Player.controller.collisions.below)
                {
                    allowChasing = false;
                }
                else if (Mathf.Abs(hitTarget.transform.position.x - transform.position.x) > 0.1f)
                {
                    if ((IsFacingRight() && transform.position.x > gameSession.Player.transform.position.x) ||
                        (!IsFacingRight() && transform.position.x < gameSession.Player.transform.position.x))
                    {
                        Flip();
                    }
                }
                else
                {
                    allowChasing = false;
                }
            }
            else
            {
                allowChasing = false;
                isPlayerInRange = false;
            }
        }

        bool allowAutoPunch = punchType == PUNCHTYPE.Auto && !IsDoingOtherAttacking();
        bool allowSpeedPunch = punchType == PUNCHTYPE.OnlySpeedAttack && isAttackSpeed;
        if (!isMeleeAttacking && Time.time > (ME_LastAttack + MA_rate) && (allowAutoPunch || allowSpeedPunch))
        {
            if (Physics2D.CircleCast(MeleePoint.position, meleeDamageZone, Vector2.zero, 0, gameSession.PlayerLayer))
            {
                MeleeAttack();
                BeginIdleDelay(1f, 3f);
            }
        }

        float targetVelocityX = direction.x * speed * mulSpeed;
        velocity.x = moving ? Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, controller.collisions.below ? 0.1f : 0.2f) : 0f;
        velocity.y += -gravity * Time.deltaTime;
        if (!allowChasing || !moving)
            velocity.x = 0f;
    }

    private void LateUpdate()
    {
        if (isDead)
            return;

        if (isFlyingAttack)
            velocity = Vector2.zero;

        controller.Move(velocity * Time.deltaTime, false);
        if (controller.collisions.above || controller.collisions.below)
            velocity.y = 0f;
    }

    private void TickTimers()
    {
        TickStartup();
        TickIdleDelay();
        TickDisableBoss();
        TickBlink();
        TickMeleeRecovery();
        TickDeathSequence();
        TickDisappear();
        TickSpeedAttack();
        TickSuperAttack();
        TickFallingObjectAttack();
        TickFlyingAttack();
        TickTornadoAttack();
        TickBoomerangAttack();
    }

    private void TickStartup()
    {
        if (startupTimer < 0f)
            return;

        startupTimer -= Time.deltaTime;
        if (startupTimer <= 0f)
        {
            startupTimer = -1f;
            moving = true;
            boss1AttackOrder.Play();
        }
    }

    private void TickIdleDelay()
    {
        if (idleResumeTimer < 0f)
            return;

        idleResumeTimer -= Time.deltaTime;
        if (idleResumeTimer <= 0f)
        {
            idleResumeTimer = -1f;
            moving = true;
        }
    }

    private void TickDisableBoss()
    {
        if (disableBossTimer < 0f)
            return;

        disableBossTimer -= Time.deltaTime;
        if (disableBossTimer <= 0f)
        {
            disableBossTimer = -1f;
            DisableBoss();
        }
    }

    private void TickBlink()
    {
        if (blinkTimer < 0f)
            return;

        blinkTimer -= Time.deltaTime;
        nextBlinkToggle -= Time.deltaTime;
        if (nextBlinkToggle <= 0f)
        {
            blinkUsingWhite = !blinkUsingWhite;
            if (characterImage != null)
                characterImage.material = blinkUsingWhite ? whiteMaterial : objMat;
            nextBlinkToggle = 0.2f;
        }

        if (blinkTimer <= 0f)
        {
            blinkTimer = -1f;
            blinkUsingWhite = false;
            isBlinking = false;
            if (characterImage != null)
                characterImage.material = objMat;
        }
    }

    private void TickMeleeRecovery()
    {
        if (meleeRecoverTimer < 0f)
            return;

        meleeRecoverTimer -= Time.deltaTime;
        if (meleeRecoverTimer <= 0f)
        {
            meleeRecoverTimer = -1f;
            FinishMeleeAttack();
        }
    }

    private void TickDeathSequence()
    {
        if (deathPhase == DeathSequencePhase.None)
            return;

        deathPhaseTimer -= Time.deltaTime;
        if (deathPhaseTimer > 0f)
            return;

        switch (deathPhase)
        {
            case DeathSequencePhase.InitialDelay:
                deathPhase = DeathSequencePhase.FirstWave;
                deathExplosionsRemaining = 3;
                TriggerDeathExplosionWaveStep();
                break;
            case DeathSequencePhase.FirstWave:
                if (TriggerDeathExplosionWaveStep())
                {
                    presentationService.ShowBlackScreen(2, Color.white);
                    deathPhase = DeathSequencePhase.WhiteFlash;
                    deathPhaseTimer = 0f;
                }
                break;
            case DeathSequencePhase.WhiteFlash:
                deathPhase = DeathSequencePhase.SecondWave;
                deathExplosionsRemaining = 4;
                TriggerDeathExplosionWaveStep();
                break;
            case DeathSequencePhase.SecondWave:
                if (TriggerDeathExplosionWaveStep())
                {
                    presentationService.HideBlackScreen(1);
                    deathPhase = DeathSequencePhase.Finish;
                    deathPhaseTimer = 0f;
                }
                break;
            case DeathSequencePhase.Finish:
                if (!finishTriggered)
                {
                    finishTriggered = true;
                    gameSession.GameFinish(1);
                    gameObject.SetActive(false);
                }
                deathPhase = DeathSequencePhase.None;
                break;
        }
    }

    private bool TriggerDeathExplosionWaveStep()
    {
        if (deathExplosionsRemaining <= 0)
            return true;

        Instantiate(dieExplosionFX,
            transform.position + new Vector3(Random.Range(-dieExplosionSize.x, dieExplosionSize.x), Random.Range(0, dieExplosionSize.y), 0),
            Quaternion.identity);
        audioService.PlaySfx(dieExplosionSound);
        CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);
        deathExplosionsRemaining--;
        deathPhaseTimer = deathExplosionsRemaining > 0 ? 0.5f : 0f;
        return deathExplosionsRemaining <= 0;
    }

    private void TickDisappear()
    {
        if (disappearPhase == DisappearPhase.None)
            return;

        disappearTimer -= Time.deltaTime;
        if (disappearTimer > 0f)
            return;

        if (disappearPhase == DisappearPhase.BeforeTeleport)
        {
            transform.position = disappearPoints[Random.Range(0, disappearPoints.Count)];
            audioService.PlaySfx(disappearSound);
            disappearPhase = DisappearPhase.AfterTeleport;
            disappearTimer = 1f;
        }
        else
        {
            disappearPhase = DisappearPhase.None;
            disappearTimer = -1f;
            disapearing = false;
        }
    }

    private void TickSpeedAttack()
    {
        if (!isAttackSpeed || speedAttackTimer < 0f)
            return;

        speedAttackTimer -= Time.deltaTime;
        if (speedAttackTimer <= 0f)
        {
            speedAttackTimer = -1f;
            mulSpeed = 1f;
            isAttackSpeed = false;
            if (ghostSprite != null)
                ghostSprite.allowGhost = false;
        }
    }

    private void TickSuperAttack()
    {
        if (!isSuperAttacking || superAttackTimer < 0f)
            return;

        if (gameSession.State != GameManager.GameState.Playing)
            return;

        superAttackTimer -= Time.deltaTime;
        if (superAttackTimer > 0f)
            return;

        if (superAttackRemaining <= 0)
        {
            isSuperAttacking = false;
            superAttackTimer = -1f;
            return;
        }

        if (SuperAttackBullet != null)
            Instantiate(SuperAttackBullet);

        if (useWaveAnimation)
            anim.SetTrigger("SuperAttack");

        audioService.PlaySfx(superAttackSound);
        superAttackRemaining--;
        superAttackTimer = delayPerAttack;
    }

    private void TickFallingObjectAttack()
    {
        if (fallingObjectPhase == FallingObjectPhase.None)
            return;

        fallingObjectTimer -= Time.deltaTime;
        if (fallingObjectTimer > 0f)
            return;

        switch (fallingObjectPhase)
        {
            case FallingObjectPhase.Windup:
                anim.SetTrigger("fallingAttackB");
                audioService.PlaySfx(soundAttackFA);
                fallingObjectPhase = FallingObjectPhase.Cooldown;
                fallingObjectTimer = 1f;
                break;
            case FallingObjectPhase.Cooldown:
                fallingObjectPhase = FallingObjectPhase.None;
                isFallingObjectAttack = false;
                fallingObjectTimer = -1f;
                break;
        }
    }

    private void TickFlyingAttack()
    {
        if (flyingAttackPhase == FlyingAttackPhase.None)
            return;

        switch (flyingAttackPhase)
        {
            case FlyingAttackPhase.MoveToPoint:
                LookAtPlayer();
                transform.position = Vector2.MoveTowards(transform.position, currentFlyingPoint, fly_speed * Time.deltaTime);
                if (Vector2.Distance(transform.position, currentFlyingPoint) > 0.1f)
                    return;

                if (flyingAttackSpreadBullet || flyingAttackThrowStone)
                {
                    flyingAttackPhase = FlyingAttackPhase.WaitBeforeThrowSpread;
                    flyingAttackTimer = Random.Range(0.5f, 2f);
                }
                else
                {
                    isFlyingAttackPrepare = true;
                    audioService.PlaySfx(soundFlyingAttackPrepare, 0.7f);
                    Instantiate(FlyingAttackBeginFX, centerPoint.position, Quaternion.identity);
                    flyingAttackPhase = FlyingAttackPhase.PrepareCharge;
                    flyingAttackTimer = waitingBeforeAttack;
                }
                break;

            case FlyingAttackPhase.WaitBeforeThrowSpread:
                flyingAttackTimer -= Time.deltaTime;
                if (flyingAttackTimer > 0f)
                    return;

                LookAtPlayer();
                if (flyingAttackSpreadBullet)
                {
                    anim.SetTrigger("SkillSpread");
                }
                else
                {
                    ThrowStoneCoAction();
                }

                flyingAttackPhase = FlyingAttackPhase.WaitAfterThrowSpread;
                flyingAttackTimer = Random.Range(0.5f, 1f);
                break;

            case FlyingAttackPhase.WaitAfterThrowSpread:
                flyingAttackTimer -= Time.deltaTime;
                if (flyingAttackTimer > 0f)
                    return;

                FinishFlyingAttack();
                break;

            case FlyingAttackPhase.PrepareCharge:
                flyingAttackTimer -= Time.deltaTime;
                if (flyingAttackTimer > 0f)
                    return;

                if (gameSession.Player != null && gameSession.Player.isPlaying)
                {
                    LookAtPlayer();
                    RaycastHit2D hit = Physics2D.Raycast(centerPoint.position, gameSession.Player.transform.position + Vector3.up - centerPoint.position, 100, gameSession.GroundLayer);
                    targetFlyingAttack = hit ? hit.point : (Vector2)gameSession.Player.transform.position + Vector2.up;
                    audioService.PlaySfx(airAttackSound);
                    anim.SetBool("isFlyingAttack", true);
                    flyingAttackPhase = FlyingAttackPhase.Charge;
                }
                else
                {
                    FinishFlyingAttack();
                }
                break;

            case FlyingAttackPhase.Charge:
                transform.position = Vector2.MoveTowards(transform.position, targetFlyingAttack, flyAttackSpeed * Time.deltaTime);
                if (Vector2.Distance(transform.position, targetFlyingAttack) > 0.1f)
                    return;

                Instantiate(FlyingAttackEndFX, transform.position, Quaternion.identity);
                audioService.PlaySfx(soundFlyingAttackExplosion);
                RaycastHit2D hitGround = Physics2D.CircleCast(centerPoint.position, 1f, Vector2.zero, 0f, gameSession.PlayerLayer);
                if (hitGround && gameSession.Player != null)
                    gameSession.Player.TakeDamage(flyingAttackDamage, new Vector2(0, 3), gameObject, hitGround.point);

                FinishFlyingAttack();
                break;
        }
    }

    private void FinishFlyingAttack()
    {
        anim.SetBool("isFlying", false);
        anim.SetBool("isFlyingAttack", false);
        velocity = Vector2.zero;
        isFlyingAttack = false;
        isFlyingAttackPrepare = false;
        flyingAttackThrowStone = false;
        flyingAttackSpreadBullet = false;
        flyingAttackPhase = FlyingAttackPhase.None;
        flyingAttackTimer = -1f;
    }

    private void TickTornadoAttack()
    {
        if (tornadoPhase == TornadoPhase.None)
            return;

        if (tornadoPhase == TornadoPhase.WaitingForAnim)
        {
            if (!tornadoSpawnAllowed)
                return;

            tornadoPhase = TornadoPhase.Spawning;
            tornadoRemaining = TA_numberOfTornado;
            tornadoTimer = 0f;
        }

        if (tornadoPhase == TornadoPhase.Spawning)
        {
            tornadoTimer -= Time.deltaTime;
            if (tornadoTimer > 0f)
                return;

            if (tornadoRemaining > 0)
            {
                Instantiate(TA_Tornado, transform.position + Vector3.up * 0.1f, Quaternion.identity).Init(TA_twoDirection, TA_damagePerBullet, TA_bulletSpeed);
                tornadoRemaining--;
                tornadoTimer = TA_timneDelayTornado;
            }
            else
            {
                tornadoPhase = TornadoPhase.Cooldown;
                tornadoTimer = 1f;
            }

            return;
        }

        if (tornadoPhase == TornadoPhase.Cooldown)
        {
            tornadoTimer -= Time.deltaTime;
            if (tornadoTimer > 0f)
                return;

            tornadoPhase = TornadoPhase.None;
            tornadoTimer = -1f;
            tornadoSpawnAllowed = false;
            isTornadoAttacking = false;
        }
    }

    private void TickBoomerangAttack()
    {
        if (boomerangPhase == BoomerangPhase.None)
            return;

        if (boomerangPhase == BoomerangPhase.WaitingForAnim)
        {
            if (!boomerangThrowAllowed)
                return;

            boomerangThrowAllowed = false;
            audioService.PlaySfx(BA_soundBegin);
            _BA_bullet = Instantiate(BA_bullet, BA_startPoint.position, Quaternion.identity);
            boomerangAngle = 0f;
            boomerangPhase = BoomerangPhase.Traveling;
            return;
        }

        boomerangAngle += BA_bulletSpeed * Time.deltaTime;
        Vector3 target = (Vector2)BA_startPoint.position + BA_distance * (IsFacingRight() ? Vector2.right : Vector2.left);
        _BA_bullet.transform.position = Vector2.Lerp(BA_startPoint.position, target, Mathf.Sin(boomerangAngle * Mathf.Deg2Rad));
        if (boomerangAngle < 180f)
            return;

        if (_BA_bullet != null)
            Destroy(_BA_bullet);

        isBoomerangeAttacking = false;
        anim.SetBool("boomerangAttack", false);
        audioService.PlaySfx(BA_soundEnd);
        SetMoving(true);
        boomerangPhase = BoomerangPhase.None;
    }

    private bool IsFacingRight()
    {
        return transform.rotation.y == 0;
    }

    private void Flip()
    {
        direction = -direction;
        transform.rotation = Quaternion.Euler(transform.rotation.x, IsFacingRight() ? 180 : 0, transform.rotation.z);
    }

    private void LookAtPlayer()
    {
        if (gameSession.Player == null)
            return;

        if ((IsFacingRight() && transform.position.x > gameSession.Player.transform.position.x) ||
            (!IsFacingRight() && transform.position.x < gameSession.Player.transform.position.x))
        {
            Flip();
        }
    }

    private void BeginIdleDelay(float min, float max)
    {
        moving = false;
        idleResumeTimer = Random.Range(min, max);
    }

    public bool IsIdleDelayActive => idleResumeTimer >= 0f;

    public void SetMoving(bool move)
    {
        moving = move;
        velocity.x = 0f;
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!isPlaying || isDead || disapearing || isFlyingAttackPrepare)
            return;

        currentHealth -= damage;
        isDead = currentHealth <= 0;

        if (bossType == BOSSTYPE.BOSS)
        {
            bossHealthbarService.UpdateHealth(currentHealth);
        }
        else if (healthBar != null)
        {
            healthBar.UpdateValue(currentHealth / (float)health);
        }

        if (isDead)
        {
            ResetAllBossActions();

            anim.SetBool("isDead", true);
            DisableBossColliders();

            if (deadBehavior == DEAD_BEHAIVIOR.FinishLevel)
            {
                audioService.PauseMusic(true);
                anim.enabled = false;
                gameSession.MissionStarCollected = 3;
                controllerInputService.StopMove();
                presentationService.SetControllerVisible(false);
                presentationService.SetGameplayUiVisible(false);
                deathPhase = DeathSequencePhase.InitialDelay;
                deathPhaseTimer = 1f;
                finishTriggered = false;
            }
            else
            {
                anim.SetTrigger("die");
                disableBossTimer = 2f;
                audioService.PlaySfx(deadSound);

                var spawnItem = GetComponent<EnemySpawnItem>();
                if (spawnItem != null)
                    spawnItem.SpawnItem();
            }
        }
        else
        {
            if (!isMeleeAttacking)
                anim.SetTrigger("hit");

            StartBlink();
            audioService.PlaySfx(hurtSound, 0.7f);
        }
    }

    private void ResetAllBossActions()
    {
        startupTimer = -1f;
        idleResumeTimer = -1f;
        meleeRecoverTimer = -1f;
        disapearing = false;
        disappearPhase = DisappearPhase.None;
        disappearTimer = -1f;
        isAttackSpeed = false;
        speedAttackTimer = -1f;
        isSuperAttacking = false;
        superAttackTimer = -1f;
        superAttackRemaining = 0;
        isFallingObjectAttack = false;
        fallingObjectPhase = FallingObjectPhase.None;
        fallingObjectTimer = -1f;
        isFlyingAttack = false;
        isFlyingAttackPrepare = false;
        flyingAttackPhase = FlyingAttackPhase.None;
        flyingAttackTimer = -1f;
        isTornadoAttacking = false;
        tornadoPhase = TornadoPhase.None;
        tornadoTimer = -1f;
        tornadoSpawnAllowed = false;
        isBoomerangeAttacking = false;
        boomerangPhase = BoomerangPhase.None;
        boomerangThrowAllowed = false;
        if (ghostSprite != null)
            ghostSprite.allowGhost = false;
        if (characterImage != null)
            characterImage.material = objMat;
        if (_BA_bullet != null)
            Destroy(_BA_bullet);
    }

    private void StartBlink()
    {
        isBlinking = true;
        blinkTimer = blinking;
        nextBlinkToggle = 0f;
        blinkUsingWhite = false;
    }

    private void DisableBossColliders()
    {
        foreach (var box in GetComponents<BoxCollider2D>())
            box.enabled = false;

        foreach (var cir in GetComponents<CircleCollider2D>())
            cir.enabled = false;
    }

    [Header("EFFECT WHEN DIE")]
    public GameObject dieExplosionFX;
    public Vector2 dieExplosionSize = new Vector2(2, 3);
    public AudioClip dieExplosionSound;

    [Header("BLINKING")]
    public float blinking = 1.5f;
    bool isBlinking;
    public Material whiteMaterial;

    private void DisableBoss()
    {
        if (deadFX != null)
            Instantiate(deadFX, transform.position, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(MeleePoint.position, meleeDamageZone);
    }

    [Header("MELEE ATTACK")]
    public PUNCHTYPE punchType;
    public float MA_rate = 2f;
    [Range(0.1f, 2f)] public float meleeDamageZone = 0.5f;
    [Range(10, 100)] public int givePlayerDamage = 30;
    public Transform MeleePoint;
    [ReadOnly] public string animMeleeAttackTrigger = "meleeAttack";
    [ReadOnly] public bool isMeleeAttacking;
    float ME_LastAttack;

    private void MeleeAttack()
    {
        isMeleeAttacking = true;
        ME_LastAttack = Time.time;
        anim.SetTrigger(animMeleeAttackTrigger);
        meleeRecoverTimer = 1f;
    }

    private void FinishMeleeAttack()
    {
        isMeleeAttacking = false;
    }

    public void AnimMeleeAttack()
    {
        var hit = Physics2D.CircleCast(MeleePoint.position, meleeDamageZone, Vector2.zero, 0, gameSession.PlayerLayer);
        if (hit && gameSession.Player != null)
            gameSession.Player.TakeDamage(givePlayerDamage, new Vector2(0, 3), gameObject, hit.point);

        audioService.PlaySfx(attackSound);
    }

    private bool IsDoingOtherAttacking()
    {
        return isFlyingAttack || isFallingObjectAttack || isSuperAttacking || disapearing || isTornadoAttacking || isBoomerangeAttacking;
    }

    [Header("Disappear and Show")]
    public AudioClip disappearSound;
    public Transform disappearPointGroup;
    [ReadOnly] public List<Vector3> disappearPoints;
    public SpriteRenderer characterImage;
    [HideInInspector] public bool disapearing;

    public void DisappearShowAction()
    {
        disapearing = true;
        disappearPhase = DisappearPhase.BeforeTeleport;
        disappearTimer = 1f;
        audioService.PlaySfx(disappearSound);
    }

    [Header("Throw Stone")]
    public bool useTrack = false;
    public float ts_timeToLive = 6f;
    public GameObject Stone;
    public Transform attackPoint;
    public int speadBullet = 5;
    public AudioClip throwSound;

    public void ThrowStoneCoAction()
    {
        audioService.PlaySfx(throwSound);
        anim.SetTrigger("Throw");
        BeginIdleDelay(0.2f, 1f);
    }

    public void AnimThrowAttack()
    {
        if (isDead)
            return;

        int j = speadBullet / 2;
        for (int i = 0; i < speadBullet; i++)
        {
            GameObject bullet = Instantiate(Stone, attackPoint.position + Vector3.up * i * (useTrack ? 0.5f : 0f), Quaternion.identity);
            bullet.GetComponent<ChasingStone>().Init(j, 0, useTrack);
            Destroy(bullet, ts_timeToLive);
            j--;
        }
    }

    [Header("Speed Attack")]
    public AudioClip speedAttackSound;
    public GameObject speedAttackHitPlayerFX;
    public float speedAttackTime = 3f;
    public float Speed = 5f;
    public bool speedAttackUseGhostFX = true;
    [HideInInspector] public bool isAttackSpeed;

    public void SpeedAttackCoAction()
    {
        audioService.PlaySfx(speedAttackSound);
        mulSpeed = Speed;
        isAttackSpeed = true;
        speedAttackTimer = speedAttackTime;

        if (ghostSprite != null)
            ghostSprite.allowGhost = speedAttackUseGhostFX;
    }

    [Header("Super Attack")]
    public bool useWaveAnimation = true;
    public GameObject SuperAttackBullet;
    public float delayPerAttack = 1.5f;
    public int attackTimes = 5;
    public AudioClip superAttackSound;
    [HideInInspector] public bool isSuperAttacking;

    public void SuperAttackCoAction()
    {
        superAttackRemaining = attackTimes;
        isSuperAttacking = true;
        superAttackTimer = 0f;
    }

    [Header("FALLING OBJECT ATTACK")]
    [Range(3, 8)] public int numberBulletFA = 4;
    public float delayFA = 1f;
    public float speedFA = 30f;
    public float spaceFA = 2f;
    public float distancePlayerHeadFA = 5f;
    public AudioClip soundBPrepareFA;
    public AudioClip soundAttackFA;
    public bool isFallingObjectAttack { get; set; }
    public FallingProjectileBullet fallingAttackBullet;
    public LayerMask layerGround;

    public void FallingObjectAttackCoAction()
    {
        isFallingObjectAttack = true;

        RaycastHit2D checkAbovePlayer = Physics2D.Raycast(gameSession.Player.transform.position + Vector3.up, Vector2.up, 100f, layerGround);
        float spawnBulletFX = distancePlayerHeadFA;
        if (checkAbovePlayer && Vector2.Distance(gameSession.Player.transform.position, checkAbovePlayer.point) < distancePlayerHeadFA)
            spawnBulletFX = Vector2.Distance(gameSession.Player.transform.position, checkAbovePlayer.point) - 0.6f;

        Instantiate(fallingAttackBullet, gameSession.Player.transform.position + Vector3.up * spawnBulletFX, Quaternion.identity)
            .Init(numberBulletFA, delayFA, speedFA, spaceFA);

        anim.SetTrigger("fallingAttack");
        audioService.PlaySfx(soundBPrepareFA);
        fallingObjectPhase = FallingObjectPhase.Windup;
        fallingObjectTimer = delayFA - 0.1f;
    }

    [Header("Flying")]
    public Transform flyPointGroup;
    [ReadOnly] public List<Vector3> flyPoints;
    public float fly_speed = 3f;
    public AudioClip flyingSound;
    AudioSource flyingAudioScr;

    [Header("FLYING ATTACK")]
    public float waitingBeforeAttack = 1f;
    public float flyAttackSpeed = 5f;
    public int flyingAttackDamage = 30;
    public GameObject FlyingAttackBeginFX;
    public GameObject FlyingAttackEndFX;
    public AudioClip soundFlyingAttackPrepare;
    public AudioClip soundFlyingAttackExplosion;
    public AudioClip airAttackSound;
    bool isFlyingAttackPrepare;

    [Header("FLYING THROW SPREAD BULLET")]
    public GameObject spreadBullet;
    public AudioClip spreadBulletAttackSound;
    [HideInInspector] public bool isFlyingAttack;

    public void FlyingAttackCoAction(bool throwStone = false, bool throwSpreadBullet = false)
    {
        isFlyingAttack = true;
        flyingAttackThrowStone = throwStone;
        flyingAttackSpreadBullet = throwSpreadBullet;
        currentFlyingPoint = flyPoints[Random.Range(0, flyPoints.Count)];
        anim.SetBool("isFlying", true);
        flyingAttackPhase = FlyingAttackPhase.MoveToPoint;
    }

    public void AnimSpreadAttack()
    {
        audioService.PlaySfx(spreadBulletAttackSound, 0.7f);
        var bullet = SpawnSystemHelper.GetNextObject(spreadBullet, false);
        bullet.transform.position = transform.position;
        bullet.SetActive(true);
    }

    [Header("TORNADO ATTACK")]
    [ReadOnly] public string TA_Trigger = "tornadoAttack";
    public bool TA_twoDirection = true;
    public int TA_damagePerBullet = 50;
    public float TA_bulletSpeed = 5f;
    public TornadoBullet TA_Tornado;
    public AudioClip TA_sound;
    public int TA_numberOfTornado = 1;
    public float TA_timneDelayTornado = 1f;
    public bool TA_earthQuakeFX = true;
    [ReadOnly] public bool isTornadoAttacking;

    public void TORNADOAttackCoAction()
    {
        isTornadoAttacking = true;
        tornadoSpawnAllowed = false;
        anim.SetTrigger(TA_Trigger);
        tornadoPhase = TornadoPhase.WaitingForAnim;
    }

    public void AnimTornatoAttack()
    {
        if (isDead)
            return;

        tornadoSpawnAllowed = true;
        if (TA_earthQuakeFX)
            CameraPlay.EarthQuakeShake();

        audioService.PlaySfx(TA_sound);
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
        if (_BA_bullet != null)
            Destroy(_BA_bullet);

        ResetAllBossActions();
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

    [Header("BOOMERANG ATTACK")]
    public GameObject BA_bullet;
    public Transform BA_startPoint;
    public float BA_distance = 8f;
    public float BA_bulletSpeed = 3f;
    public AudioClip BA_soundBegin;
    public AudioClip BA_soundEnd;
    GameObject _BA_bullet;
    [ReadOnly] public bool isBoomerangeAttacking;

    public void BoomerangAttackCoAction()
    {
        isBoomerangeAttacking = true;
        anim.SetBool("boomerangAttack", true);
        SetMoving(false);
        boomerangThrowAllowed = false;
        boomerangPhase = BoomerangPhase.WaitingForAnim;
    }

    public void AnimBoomerang()
    {
        boomerangThrowAllowed = true;
    }
}
