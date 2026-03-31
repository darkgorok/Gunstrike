using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Boss1AttackOrder))]
[RequireComponent(typeof(Controller2D))]
public class BOSS_1 : BossManager, ICanTakeDamage, IListener
{
    public enum BOSSTYPE { BOSS, SUPER_ENEMY }
    public enum PUNCHTYPE { Auto, OnlySpeedAttack, None }
    public enum DEAD_BEHAIVIOR { FinishLevel, None }
    [Header("SETUP")]
    public BOSSTYPE bossType = BOSSTYPE.BOSS;
    public Sprite bossIcon;
    public DEAD_BEHAIVIOR deadBehavior;
    public float speed = 1f;
    [Range(50, 1000)]
    public int health = 500;
    [ReadOnly] public int currentHealth;
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    protected HealthBarEnemyNew healthBar;
    public float gravity = 35f;

    [Header("EARTH QUAKE")]
    public float _eqTime = 0.3f;
    public float _eqSpeed = 60;
    public float _eqSize = 1.5f;

    [Space]
    public Transform centerPoint;
    public GameObject deadFX;

    [Header("SOUND")]
    public AudioClip showupSound;
    public AudioClip attackSound;
    public AudioClip deadSound;
    public AudioClip hurtSound;

    [HideInInspector]
    public bool isDead = false;

    //Private variable
    [HideInInspector]
    protected Vector3 velocity;
    protected float velocityXSmoothing = 0;
    Boss1AttackOrder boss1AttackOrder;
    Controller2D controller;
    Animator anim;
    bool moving;
    float mulSpeed = 1;
    CheckTargetHelper checkTargetHelper;
    [ReadOnly] public bool isPlaying = false;
    public bool isFacingRight()
    {
        return transform.rotation.y == 0 ? true : false;
    }

    void Start()
    {
        controller = GetComponent<Controller2D>();
        checkTargetHelper = GetComponent<CheckTargetHelper>();
        anim = GetComponent<Animator>();
        boss1AttackOrder = FindObjectOfType<Boss1AttackOrder>();
        if (characterImage)
            objMat = characterImage.material;

        _direction = isFacingRight() ? Vector2.right : Vector2.left;

        currentHealth = health;

      
        ghostSprite = GetComponent<GhostSprites>();
        if (ghostSprite)
            ghostSprite.allowGhost = false;

        //get list fly point
        flyPoints = new List<Vector3>();
        var child = flyPointGroup.GetComponentsInChildren<Transform>();
        foreach (var c in child)
        {
            if (c.position != flyPointGroup.position)
                flyPoints.Add(c.position);
        }
        flyPointGroup.parent = null;

        //get list disappear point
        disappearPoints = new List<Vector3>();
        var child2 = disappearPointGroup.GetComponentsInChildren<Transform>();
        foreach (var c in child2)
        {
            if (c.position != disappearPointGroup.position)
                disappearPoints.Add(c.position);
        }
        disappearPointGroup.parent = null;
    }
    
    public override void Play()
    {
        if (isPlaying)
            return;

        isPlaying = true;
        StartCoroutine(PlayCo());
    }

    IEnumerator PlayCo()
    {
        if (bossType == BOSSTYPE.BOSS)
        {
            BossHealthbar.Instance.Init(bossIcon, health);
        }
        else
        {
            var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
            healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
            healthBar.Init(transform, (Vector3)healthBarOffset);
        }

        SoundManager.PlaySfx(showupSound);
        anim.SetTrigger("showup");
        yield return new WaitForSeconds(1);
        moving = true;
        boss1AttackOrder.Play();
    }

    [HideInInspector] public bool isPlayerInRange = false;
    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("speed", Mathf.Abs(velocity.x));

        if (isDead || GameManager.Instance.State != GameManager.GameState.Playing || GameManager.Instance.Player.isFinish || disapearing || isSuperAttacking || isFallingObjectAttack || isFlyingAttack || isTornadoAttacking || isBoomerangeAttacking)
        {
            //anim.SetBool("walk", false);
            velocity.x = 0;

            if (isSuperAttacking)
                LookAtPlayer();

            return;
        }

        bool allowChasing = true;
        if (moving)
        {
            //hit = Physics2D.BoxCast(transform.position + Vector3.up * (detectZoneHeight * 0.5f), new Vector2(detectZoneWidth, detectZoneHeight), 0, Vector2.zero, 0, GameManager.Instance.playerLayer);
            var hitTarget = checkTargetHelper.CheckTarget();
            if (hitTarget)
            {
                isPlayerInRange = true;
                if (Mathf.Abs(hitTarget.transform.position.x - transform.position.x) < 0.3f && !GameManager.Instance.Player.controller.collisions.below)
                {
                    allowChasing = false;
                }
                else if (Mathf.Abs(hitTarget.transform.position.x - transform.position.x) > 0.1f)
                {
                    if ((isFacingRight() && transform.position.x > GameManager.Instance.Player.transform.position.x) || (!isFacingRight() && transform.position.x < GameManager.Instance.Player.transform.position.x))
                    {
                        Flip();
                    }
                }
                else
                    allowChasing = false;
            }
            else
            {
                allowChasing = false;
                isPlayerInRange = false;
            }
        }

        if (!isMeleeAttacking && (Time.time > (ME_LastAttack + MA_rate)) && (punchType == PUNCHTYPE.Auto && !isDoingOtherAttacking()) || (punchType == PUNCHTYPE.OnlySpeedAttack && isAttackSpeed))
        {
            if (Physics2D.CircleCast(MeleePoint.position, meleeDamageZone, Vector2.zero, 0, GameManager.Instance.playerLayer))
            {
                MeleeAttack();
                StartCoroutine(IdleDelay(1, 3));
            }
        }

        float targetVelocityX = _direction.x * speed * mulSpeed;
        velocity.x = moving ? Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? 0.1f : 0.2f) : 0;
        velocity.y += -gravity * Time.deltaTime;
        if (!allowChasing || !moving)
            velocity.x = 0;
    }

    

    void LateUpdate()
    {
        if (isDead)
            return;

        if (isFlyingAttack)
            velocity = Vector2.zero;
        
        controller.Move(velocity * Time.deltaTime, false);

        if (controller.collisions.above || controller.collisions.below)
            velocity.y = 0;
    }

    private Vector2 _direction;
    
    void Flip()
    {
        _direction = -_direction;
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, isFacingRight() ? 180 : 0, transform.rotation.z));
    }


    void LookAtPlayer()
    {
        if ((isFacingRight() && transform.position.x > GameManager.Instance.Player.transform.position.x) || (!isFacingRight() && transform.position.x < GameManager.Instance.Player.transform.position.x))
        {
            Flip();
        }
    }

    IEnumerator IdleDelay(float min, float max)
    {
        moving = false;
        var delay = Random.Range(min, max);
        yield return new WaitForSeconds(delay);
        moving = true;
    }

    public void SetMoving(bool move)
    {
        //anim.SetBool("walk", move);
        velocity.x = 0;
    }
    
    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!isPlaying || isDead || disapearing || isFlyingAttackPrepare)
            return;

        
        //Debug.LogError(damage);
        currentHealth -= (int)damage;
        isDead = currentHealth <= 0 ? true : false;
        
        if(bossType == BOSSTYPE.BOSS)
        {
            BossHealthbar.Instance.UpdateHealth(currentHealth);
        }
        else if(healthBar)
            healthBar.UpdateValue(currentHealth / (float)health);

        if (isDead)
        {
            StopAllCoroutines();
            CancelInvoke();

            disapearing = false;
            isAttackSpeed = false;
            isSuperAttacking = false;
            isFallingObjectAttack = false;
            isFlyingAttack = false;
            isFlyingAttackPrepare = false;
            if(ghostSprite)
            ghostSprite.allowGhost = false;
            if(characterImage)
            characterImage.material = objMat;

            if (_BA_bullet != null)
                Destroy(_BA_bullet);

            anim.SetBool("isDead", true);
            var boxCo = GetComponents<BoxCollider2D>();
            foreach (var box in boxCo)
            {
                box.enabled = false;
            }
            var CirCo = GetComponents<CircleCollider2D>();
            foreach (var cir in CirCo)
            {
                cir.enabled = false;
            }

            if (deadBehavior == DEAD_BEHAIVIOR.FinishLevel)
            {
                StartCoroutine(BossDieBehavior());
            }
            else
            {
                anim.SetTrigger("die");
                Invoke("DisableBoss", 2);
                SoundManager.PlaySfx(deadSound);

                //try spawn random item
                var spawnItem = GetComponent<EnemySpawnItem>();
                if (spawnItem != null)
                {
                    spawnItem.SpawnItem();
                }
            }

        }
        else
        {
            if (!isMeleeAttacking)
                anim.SetTrigger("hit");
            StartCoroutine(BlinkEffecrCo());		//begin the blink effect
            SoundManager.PlaySfx(hurtSound, 0.7f);
        }
    }

    [Header("EFFECT WHEN DIE")]
    public GameObject dieExplosionFX;
    public Vector2 dieExplosionSize = new Vector2(2, 3);
    public AudioClip dieExplosionSound;

    IEnumerator BossDieBehavior()
    {
        SoundManager.Instance.PauseMusic(true);
        anim.enabled = false;
        GameManager.Instance.MissionStarCollected = 3;
        ControllerInput.Instance.StopMove();
        MenuManager.Instance.TurnController(false);
        MenuManager.Instance.TurnGUI(false);

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 3; i++)
        {
            Instantiate(dieExplosionFX, transform.position + new Vector3(Random.Range(-dieExplosionSize.x, dieExplosionSize.x), Random.Range(0, dieExplosionSize.y), 0), Quaternion.identity);
            SoundManager.PlaySfx(dieExplosionSound);
            CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);
            yield return new WaitForSeconds(0.5f);
        }

        BlackScreenUI.instance.Show(2, Color.white);
        for (int i = 0; i < 4; i++)
        {
            Instantiate(dieExplosionFX, transform.position + new Vector3(Random.Range(-dieExplosionSize.x, dieExplosionSize.x), Random.Range(0, dieExplosionSize.y), 0), Quaternion.identity);
            SoundManager.PlaySfx(dieExplosionSound);
            CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);
            yield return new WaitForSeconds(0.5f);
        }

        BlackScreenUI.instance.Hide(1);

        //anim.enabled = true;
        //anim.SetTrigger("die");
        //SoundManager.PlaySfx(deadSound);
        GameManager.Instance.GameFinish(1);
        gameObject.SetActive(false);

    }

    [Header("BLINKING")]
    public float blinking = 1.5f;       //blinking time allowed
    bool isBlinking = false;
    //public SpriteRenderer characterImage;
    public Material whiteMaterial;
    Material objMat;

    IEnumerator BlinkEffecrCo()
    {
        isBlinking = true;
        int blink = (int)(blinking * 0.5f / 0.2f);

        for (int i = 0; i < blink; i++)
        {

            if (characterImage)
                characterImage.material = whiteMaterial;
            yield return new WaitForSeconds(0.2f);

            if (characterImage)
                characterImage.material = objMat;
            yield return new WaitForSeconds(0.2f);
        }
        if (characterImage)
            characterImage.material = objMat;
        isBlinking = false;
    }


    void DisableBoss()
    {
        if (deadFX)
            Instantiate(deadFX, transform.position, Quaternion.identity);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(MeleePoint.position, meleeDamageZone);
    }

    #region MELEE ATTACK
    [Header("MELEE ATTACK")]
    public PUNCHTYPE punchType;
    public float MA_rate = 2;
    [Range(0.1f, 2)]
    public float meleeDamageZone = 0.5f;
    [Range(10, 100)]
    public int givePlayerDamage = 30;
    public Transform MeleePoint;
    [ReadOnly] public string animMeleeAttackTrigger = "meleeAttack";
   [ReadOnly] public bool isMeleeAttacking = false;
    float ME_LastAttack = 0;

    void MeleeAttack()
    {
        isMeleeAttacking = true;
        ME_LastAttack = Time.time;
        anim.SetTrigger(animMeleeAttackTrigger);
        Invoke("FinishMeleeAttack", 1);
    }

    void FinishMeleeAttack()
    {
        isMeleeAttacking = false;
    }

    public void AnimMeleeAttack()
    {
        var hit = Physics2D.CircleCast(MeleePoint.position, meleeDamageZone, Vector2.zero, 0, GameManager.Instance.playerLayer);
        if (hit)
            GameManager.Instance.Player.TakeDamage(givePlayerDamage, new Vector2(0, 3), gameObject, hit.point);
        
        SoundManager.PlaySfx(attackSound);
    }

    bool isDoingOtherAttacking()
    {
        return isFlyingAttack || isFallingObjectAttack || isSuperAttacking || disapearing || isTornadoAttacking || isBoomerangeAttacking;
    }
    
    #endregion

    #region DISAPPEAR
    [Header("Disappear and Show")]
    public AudioClip disappearSound;
    public Transform disappearPointGroup;
    [ReadOnly] public List<Vector3> disappearPoints;
    public SpriteRenderer characterImage;

    [HideInInspector] public bool disapearing = false;

    public void DisappearShowAction()
    {
        StartCoroutine(DisappearShowCo());
    }

    IEnumerator DisappearShowCo()
    {
        //anim.SetBool("walk", false);
        disapearing = true;
        SoundManager.PlaySfx(disappearSound);
        yield return new WaitForSeconds(1);
        transform.position = disappearPoints[Random.Range(0, disappearPoints.Count)];
        SoundManager.PlaySfx(disappearSound);
        yield return new WaitForSeconds(1);

        disapearing = false;
    }

    #endregion

    #region THROWN STONE
    [Header("Throw Stone")]
    public bool useTrack = false;
    public float ts_timeToLive = 6;
    public GameObject Stone;
    public Transform attackPoint;
    public int speadBullet = 5;
    public AudioClip throwSound;

    public void ThrowStoneCoAction()
    {
        StartCoroutine(ThrowStoneCo());
    }

    IEnumerator ThrowStoneCo()
    {
        SoundManager.PlaySfx(throwSound);
        anim.SetTrigger("Throw");
        StartCoroutine(IdleDelay(0.2f, 1));
        yield return null;
    }

    public void AnimThrowAttack()
    {
        if (isDead)
            return;
        int j = speadBullet / 2;
        for (int i = 0; i < speadBullet; i++)
        {
            GameObject bullet = Instantiate(Stone, attackPoint.position + Vector3.up * i * (useTrack ? 0.5f : 0), Quaternion.identity) as GameObject;
            bullet.GetComponent<ChasingStone>().Init(j, 0, useTrack);
            Destroy(bullet, ts_timeToLive);
            j--;
        }
    }
    #endregion

    #region SPEED ATTACK
    [Header("Speed Attack")]
    public AudioClip speedAttackSound;
    public GameObject speedAttackHitPlayerFX;
    public float speedAttackTime = 3;
    public float Speed = 5f;
    public bool speedAttackUseGhostFX = true;

    private GhostSprites ghostSprite;
    [HideInInspector] public bool isAttackSpeed = false;

    public void SpeedAttackCoAction()
    {
        StartCoroutine(SpeedAttackCo());
    }

    IEnumerator SpeedAttackCo()
    {
        SoundManager.PlaySfx(speedAttackSound);
        mulSpeed = Speed;
        isAttackSpeed = true;

        if (ghostSprite)
            ghostSprite.allowGhost = speedAttackUseGhostFX;

        yield return new WaitForSeconds(speedAttackTime);

        mulSpeed = 1;
        isAttackSpeed = false;

        if (ghostSprite)
            ghostSprite.allowGhost = false;
    }

    #endregion

    #region SUPER ATTACK
    [Header("Super Attack")]
    public bool useWaveAnimation = true;
    public GameObject SuperAttackBullet;
    public float delayPerAttack = 1.5f;
    public int attackTimes = 5;
    public AudioClip superAttackSound;

    int superAttackCurrentBulletNumber = 0;
    [HideInInspector] public bool isSuperAttacking = false;

    public void SuperAttackCoAction()
    {
        StartCoroutine(SuperAttackCo());
    }

    IEnumerator SuperAttackCo()
    {
        superAttackCurrentBulletNumber = 0;
        isSuperAttacking = true;

        while (superAttackCurrentBulletNumber < attackTimes && GameManager.Instance.State == GameManager.GameState.Playing)
        {
            if (SuperAttackBullet)
                Instantiate(SuperAttackBullet);

            if (useWaveAnimation)
                anim.SetTrigger("SuperAttack");

            SoundManager.PlaySfx(superAttackSound);

            yield return new WaitForSeconds(delayPerAttack);
            superAttackCurrentBulletNumber++;
        }

        isSuperAttacking = false;
    }
    #endregion

    #region FALLING OBJECT ATTACK
    [Header("FALLING OBJECT ATTACK")]
    [Range(3, 8)]
    public int numberBulletFA = 4;
    public float delayFA = 1;
    public float speedFA = 30;
    public float spaceFA = 2;
    public float distancePlayerHeadFA = 5;
    public AudioClip soundBPrepareFA, soundAttackFA;
    public bool isFallingObjectAttack { get; set; }
    public FallingProjectileBullet fallingAttackBullet;
    public LayerMask layerGround;

    public void FallingObjectAttackCoAction()
    {
        StartCoroutine(FallingObjectAttackCo());
    }

    IEnumerator FallingObjectAttackCo()
    {
        isFallingObjectAttack = true;

        RaycastHit2D checkAbovePlayer = Physics2D.Raycast(GameManager.Instance.Player.transform.position + Vector3.up, /*(GameManager.Instance.Player.inverseGravity ? Vector2.down : */Vector2.up/*)*/, 100, layerGround);

        float spawnBulletFX = distancePlayerHeadFA;
        if (checkAbovePlayer)
        {
            if (Vector2.Distance(GameManager.Instance.Player.transform.position, checkAbovePlayer.point) < distancePlayerHeadFA)
                spawnBulletFX = Vector2.Distance(GameManager.Instance.Player.transform.position, checkAbovePlayer.point) - 0.6f;
        }

        //Debug.LogError(Vector3.up * spawnBulletFX);
        Instantiate(fallingAttackBullet, GameManager.Instance.Player.transform.position + Vector3.up * spawnBulletFX /** (GameManager.Instance.Player.inverseGravity ? -1 : 1)*/, Quaternion.identity).Init(numberBulletFA, delayFA, speedFA, spaceFA);
        anim.SetTrigger("fallingAttack");
        SoundManager.PlaySfx(soundBPrepareFA);
        //allowMoving = false;
        yield return new WaitForSeconds(delayFA - 0.1f);
        anim.SetTrigger("fallingAttackB");
        SoundManager.PlaySfx(soundAttackFA);
        yield return new WaitForSeconds(1);
        isFallingObjectAttack = false;
    }
    #endregion

    #region FLYING ATTACK
    [Header("Flying")]
    public Transform flyPointGroup;
   [ReadOnly] public List<Vector3> flyPoints;
    public float fly_speed = 3;
    public AudioClip flyingSound;
    AudioSource flyingAudioScr;

    [Header("FLYING ATTACK")]
    public float waitingBeforeAttack = 1;
    public float flyAttackSpeed = 5;
    public int flyingAttackDamage = 30;
    public GameObject FlyingAttackBeginFX;
    public GameObject FlyingAttackEndFX;
    //public bool dropBonusItem = true;
    //public GameObject dropItem;
    public AudioClip soundFlyingAttackPrepare;
    public AudioClip soundFlyingAttackExplosion;
    public AudioClip airAttackSound;

    bool isFlyingAttackPrepare = false;

    [Header("FLYING THROW SPREAD BULLET")]
    public GameObject spreadBullet;
    public AudioClip spreadBulletAttackSound;

    Vector2 targetFlyingAttack;
    [HideInInspector] public bool isFlyingAttack = false;

    public void FlyingAttackCoAction(bool throwStone = false, bool throwSpreadBullet = false)
    {
        StartCoroutine(FlyingAttackCo(throwStone, throwSpreadBullet));
    }

    IEnumerator FlyingAttackCo(bool throwStone = false, bool throwSpreadBullet = false)
    {
        isFlyingAttack = true;

        int randomPoint = Random.Range(0, flyPoints.Count);
        anim.SetBool("isFlying", true);
        while (Vector2.Distance(transform.position, flyPoints[randomPoint]) > 0.1f)
        {
            LookAtPlayer();
            transform.position = Vector2.MoveTowards(transform.position, flyPoints[randomPoint], fly_speed * Time.deltaTime);
            yield return null;
        }

        if (throwSpreadBullet)
        {
            yield return new WaitForSeconds(Random.Range(.5f, 2f));
            LookAtPlayer();

            anim.SetTrigger("SkillSpread");

            yield return new WaitForSeconds(Random.Range(.5f, 1f));
        }
        else if (throwStone)
        {
            yield return new WaitForSeconds(Random.Range(.5f, 2f));
            LookAtPlayer();
            ThrowStoneCoAction();
            yield return new WaitForSeconds(Random.Range(.5f, 1f));
        }
        else
        {
            isFlyingAttackPrepare = true;
            SoundManager.PlaySfx(soundFlyingAttackPrepare, 0.7f);
            Instantiate(FlyingAttackBeginFX, centerPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(waitingBeforeAttack);

            if (GameManager.Instance.Player.isPlaying)
            {

                LookAtPlayer();
                var hit = Physics2D.Raycast(centerPoint.position, (GameManager.Instance.Player.transform.position + Vector3.up - centerPoint.position), 100, GameManager.Instance.groundLayer);
                if (hit)
                {
                    targetFlyingAttack = hit.point;
                }
                else
                    targetFlyingAttack = GameManager.Instance.Player.transform.position + Vector3.up;

                SoundManager.PlaySfx(airAttackSound);
                anim.SetBool("isFlyingAttack", true);

                while (Vector2.Distance(transform.position, targetFlyingAttack) > 0.1f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, targetFlyingAttack, flyAttackSpeed * Time.deltaTime);
                    yield return null;
                }

                Instantiate(FlyingAttackEndFX, transform.position, Quaternion.identity);
                SoundManager.PlaySfx(soundFlyingAttackExplosion);
                //check player in range and deal a damage
                var hitGround = Physics2D.CircleCast(centerPoint.position, 1, Vector2.zero, 0, GameManager.Instance.playerLayer);
                if (hitGround)
                    GameManager.Instance.Player.TakeDamage(flyingAttackDamage, new Vector2(0, 3), gameObject, hitGround.point);
            }
        }

        anim.SetBool("isFlying", false);
        anim.SetBool("isFlyingAttack", false);
        velocity = Vector2.zero;

        isFlyingAttack = false;
        isFlyingAttackPrepare = false;
    }

    public void AnimSpreadAttack()
    {
        SoundManager.PlaySfx(spreadBulletAttackSound, 0.7f);
        var bullet = SpawnSystemHelper.GetNextObject(spreadBullet, false);
        bullet.transform.position = transform.position;
        bullet.SetActive(true);
    }
    #endregion

    #region TORNADO ATTACK
    [Header("TORNADO ATTACK")]
    [ReadOnly] public string TA_Trigger = "tornadoAttack";
    public bool TA_twoDirection = true;
    public int TA_damagePerBullet = 50;
    public float TA_bulletSpeed = 5;
    public TornadoBullet TA_Tornado;
    public AudioClip TA_sound;
    public int TA_numberOfTornado = 1;
    public float TA_timneDelayTornado = 1;
    public bool TA_earthQuakeFX = true;
    [ReadOnly] public bool isTornadoAttacking = false;
    bool TA_allowSpawn = false;

    public void TORNADOAttackCoAction()
    {
        StartCoroutine(TORNADOAttackCo());
    }

    //call by Animation event
    public void AnimTornatoAttack()
    {
        if (isDead)
            return;

        TA_allowSpawn = true;
        if (TA_earthQuakeFX)
            CameraPlay.EarthQuakeShake();

        SoundManager.PlaySfx(TA_sound);
    }

    IEnumerator TORNADOAttackCo()
    {
        isTornadoAttacking = true;
        anim.SetTrigger(TA_Trigger);

        while (!TA_allowSpawn) { yield return null; }
        
        for (int i = 0; i < TA_numberOfTornado; i++)
        {
            Instantiate(TA_Tornado, transform.position + Vector3.up * 0.1f, Quaternion.identity).Init(TA_twoDirection, TA_damagePerBullet, TA_bulletSpeed);
            yield return new WaitForSeconds(TA_timneDelayTornado);
        }
       
       
        yield return new WaitForSeconds(1);
        TA_allowSpawn = false;
        isTornadoAttacking = false;
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

        StopAllCoroutines();
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
    #endregion

    #region BOOMERANG ATTACK
    [Header("BOOMERANG ATTACK")]
    public GameObject BA_bullet;
    public Transform BA_startPoint;
    public float BA_distance = 8;
    public float BA_bulletSpeed = 3;
    public AudioClip BA_soundBegin, BA_soundEnd;
    GameObject _BA_bullet;
    [ReadOnly] public bool isBoomerangeAttacking = false;
    bool allowThrowObject = false;
    public void BoomerangAttackCoAction()
    {
        StartCoroutine(BoomerangAttackCo());
    }

    IEnumerator BoomerangAttackCo()
    {
        isBoomerangeAttacking = true;
        anim.SetBool("boomerangAttack", true);
        
        SetMoving(false);

        allowThrowObject = false;
        while (!allowThrowObject) { yield return null; }        //wait Anim event
        allowThrowObject = false;
        SoundManager.PlaySfx(BA_soundBegin);
        //float startTime = Time.time;
        _BA_bullet = Instantiate(BA_bullet, BA_startPoint.position, Quaternion.identity) as GameObject;
        Vector3 BA_target = (Vector2)BA_startPoint.position + BA_distance * (isFacingRight() ? Vector2.right : Vector2.left);

        float angle = 0;
        while (angle < 180)
        {
            angle += BA_bulletSpeed * Time.deltaTime;
            _BA_bullet.transform.position = Vector2.Lerp(BA_startPoint.position, BA_target, Mathf.Sin(angle * Mathf.Deg2Rad));
            yield return null;
        }
        
        Destroy(_BA_bullet);
        isBoomerangeAttacking = false;
        anim.SetBool("boomerangAttack", false);
        SoundManager.PlaySfx(BA_soundEnd);
        SetMoving(true);
    }

    public void AnimBoomerang()
    {
        allowThrowObject = true;
    }
    #endregion
}


