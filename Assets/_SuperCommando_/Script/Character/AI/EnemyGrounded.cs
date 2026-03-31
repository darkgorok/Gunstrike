/*
 * ENEMY GROUNDED dung Enemy binh thuong, co nhieu skill tan cong, nhung khong truy duoi Player
 * neu muon su dung Enemy co kha nang truy duoi Player, chon su dung SmartEnemyGrounded
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class EnemyGrounded : MonoBehaviour, ICanTakeDamage, IListener
{

    public enum WeaponsType { None, Melee, Fire, Throw, FireProjectileObj, Spawn }

    [Header("Setup")]
    public float gravity = 35f;
    public float moveSpeed = 3;
    public float waitingTurn = 0.5f;
    bool isWaiting = false;
    float waitingTime = 0;
    public float sockingTime = 0.5f;
    public GameObject BonusItem;    //spawn bonus item when it dead
    public GameObject HurtEffect;
    public GameObject DestroyEffect;        //spawn object when dead
    public float destroyTime = 1.5f;
    public int pointToGivePlayer = 100;

    [Header("Behavior")]
    public bool isAllowChasingPlayer = false;
    public bool isChasing { get; set; }     //set by another script
    public bool canBeFallDown = false;  //if true, the enemy will be fall from the higher platform
    public DIEBEHAVIOR dieBehavior;

    [Header("Health")]
    [Range(0, 100)]
    public float health = 50;
    float currentHealth;
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    protected HealthBarEnemyNew healthBar;

    [Header("Patrol")]
    public bool doPatrol = true;
    public float localLimitLeft = 3;
    public float localLimitRight = 3;
    float limitLeft, limitRight;

   

    [Header("Attack")]
    public WeaponsType attackType;
    public bool attackOnStart = false;
    public CheckTargetHelper checkTargetHelper;
    public float attackRate = 2f;
    float attackRateCounter;
    bool isDetectingPlayer;

    [Header("Melee Attack")]
    public Transform meleePoint;
    public float meleeAttackZone = .7f;
    public float meleeAttackCheckPlayer = 0.1f;
    public float meleeRate = 2;
    public int meleeDamage = 20;  //give damage to player

    [Header("Range Attack")]
    public Transform rangePoint;
    public Projectile rangeprojectile;
    public int bulletDamage = 50;
    public float bulletSpeed = 10;

    [Header("Grenade")]
    public float angleThrow = 60;       //the angle to throw the bomb
    public float throwForce = 300;      //how strong?
    public Transform throwPosition;     //throw the bomb at this position
    public GameObject _Grenade;     //the bomb prefab object

    [Header("Spawn Obj")]
    public Transform spawnPoint;
    public GameObject spawnObj;
    public float spawnRate = 2;
    public int maxSpawnActive = 3;
    List<GameObject> activeSpawnedObj = new List<GameObject>();
    bool isSpawningObject = false;
    
    //[Header("Fire Projectile Obj")]
    //public float 
    
    [Header("Chasing")]
    public float chaseSpeed = 3;
    public float offsetPlayerY = 0.5f;
    public float finishDistance = 0.5f;
    
    [Header("Sound")]
    public AudioClip soundMeleeAttack;
    public AudioClip soundRangAttack;
    public AudioClip soundSpawnttack;
    public AudioClip hurtSound;
    [Range(0, 1)]
    public float hurtSoundVolume = 0.5f;
    public AudioClip deadSound;
    [Range(0, 1)]
    public float deadSoundVolume = 0.5f;

    public bool isPlaying { get; set; }
    public bool isSocking { get; set; }
    public bool isDead { get; set; }

    [HideInInspector]
    public Vector3 velocity;
    private Vector2 _direction;

    public bool isFacingRight()
    {
        return transform.rotation.y == 0 ? true : false;
    }

    [HideInInspector]
    public Controller2D controller;
    Animator animator;

    float velocityXSmoothing = 0;
    Vector2 pushForce;
    private float _directionFace;
    bool isStop = false;
    bool detectToWaiting = false;       //mean when detect player, player go to hiding zone, wait there and attack player when player go out hiding zone
    Vector3 waitingAfterPlayerGoHidingZone;
    SmartProjectileAttack smartProjectileAttack;

    void Flip()
    {
        _direction = -_direction;
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, isFacingRight() ? 180 : 0, transform.rotation.z));
    }

    IEnumerator Start()
    {
        controller = GetComponent<Controller2D>();
        animator = GetComponent<Animator>();
        if (checkTargetHelper == null)
            checkTargetHelper = GetComponent<CheckTargetHelper>();
        //_direction = Vector2.left;
        if (attackType == WeaponsType.FireProjectileObj)
            smartProjectileAttack = GetComponent<SmartProjectileAttack>();

        currentHealth = health;
        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);

        if (moveSpeed == 0)
        {
            _direction = isFacingRight() ? Vector2.right : Vector2.left;
        }

        attackRateCounter = attackRate;

        limitLeft = transform.position.x - localLimitLeft;
        limitRight = transform.position.x + localLimitRight;

        isPlaying = true;
        isSocking = false;
        isChasing = false;

        yield return new WaitForEndOfFrame();
        controller.collisions.faceDir = -1;
    }

    bool SpawnObject()
    {
        if (activeSpawnedObj.Count < maxSpawnActive)
        {
            animator.SetTrigger("spawn");
            return true;
        }
        else
        {
            for (int i = 0; i < activeSpawnedObj.Count; i++)
            {
                if (activeSpawnedObj[i] ==null || activeSpawnedObj[i].activeInHierarchy == false)
                    activeSpawnedObj.Remove(activeSpawnedObj[i]);
            }

            return false;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!isPlaying)
            return;

        if (isStop)
            return;

        HandleAnimation();
        healthBar.transform.localScale = new Vector2(transform.localScale.x > 0 ? Mathf.Abs(healthBar.transform.localScale.x) : -Mathf.Abs(healthBar.transform.localScale.x), healthBar.transform.localScale.y);

        attackRateCounter -= Time.deltaTime;

        if (!isPlaying || isSocking || !GameManager.Instance.Player.isPlaying)
        {
            velocity.x = 0;
            return;
        }

        if (isWaiting)
        {
            waitingTime += Time.deltaTime;
            if (waitingTime >= waitingTurn)
            {
                isWaiting = false;
                waitingTime = 0;

                //_direction = -_direction;
                //transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                Flip();
            }
        }
        else
        {

            if ((_direction.x > 0 && controller.collisions.right) || (_direction.x < 0 && controller.collisions.left)
               || (!canBeFallDown && !controller.isGrounedAhead(velocity.x > 0) && controller.collisions.below)
               || (doPatrol && ((_direction.x > 0 && transform.position.x > limitRight) || (_direction.x < 0 && transform.position.x < limitLeft))))
            {
                isWaiting = true;
            }
        }

        if (!attackOnStart)
        {
            if (attackType != WeaponsType.None && !isSocking)
            {
                var hit = checkTargetHelper.CheckTarget((int)_direction.x);
                isDetectingPlayer = hit!=null;
                if (hit)
                {
                    DoAttack(hit);
                }
            }
        }else
            DoAttack();
    }

    void DoAttack(GameObject targetHit = null)
    {
        if (attackRateCounter <= 0)
        {

            if (attackType == WeaponsType.Fire)
            {
                FireProjectile();
                attackRateCounter = attackRate;
            }
            else if (attackType == WeaponsType.Throw)
            {
                ThrowGrenade();
                attackRateCounter = attackRate;
            }

            else if (attackType == WeaponsType.Spawn)
            {
                if (SpawnObject())
                    attackRateCounter = attackRate;
            }
            else if(attackType == WeaponsType.FireProjectileObj)
            {
                smartProjectileAttack.Shoot(targetHit);
            }
            else if (attackType == WeaponsType.Melee)
            {
                StartCoroutine(CheckMeleeAttack(meleeAttackCheckPlayer));
                attackRateCounter = attackRate;
            }
        }
    }


    public virtual void LateUpdate()
    {
        if (!isPlaying)
        {
            if (isDead && dieBehavior == DIEBEHAVIOR.FALLOUT)
            {
                velocity.y += -35 * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime, false);
            }
            return;
        }

        if (isStop)
            return;

        if (GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        if (!isPlaying || isSocking || isDetectingPlayer)
        {
            velocity = Vector2.zero;
            return;
        }

        if (!GameManager.Instance.Player.isPlaying)
            return;

        //		if (isPlaying && !isSocking) {

        //		}

        //		Debug.LogError (isChasing + "/" + isAllowChasingPlayer);

        if (isChasing && isAllowChasingPlayer)
        {
            if (GameManager.Instance.Player.gameObject.layer != LayerMask.NameToLayer("HidingZone"))
            {
                if (Mathf.Abs(Vector3.Distance(transform.position, GameManager.Instance.Player.transform.position)) > finishDistance)
                {
                    transform.position = Vector3.MoveTowards(transform.position, GameManager.Instance.Player.transform.position + new Vector3(0, offsetPlayerY, 0), chaseSpeed * Time.deltaTime);
                    _directionFace = transform.position.x > GameManager.Instance.Player.transform.position.x ? -1 : 1;
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * _directionFace, transform.localScale.y, transform.localScale.z);
                }
            }
            else
            {
                isChasing = false;
                if (gravity == 0)
                {
                    detectToWaiting = true; //mean this enemy is flying kind
                    waitingAfterPlayerGoHidingZone = GameManager.Instance.Player.transform.position + Vector3.up * 5;
                }
                else
                    transform.localScale = new Vector3((velocity.x > 0 ? -1 : 1) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

        }
        else if (detectToWaiting)
        {
            transform.position = Vector3.MoveTowards(transform.position, waitingAfterPlayerGoHidingZone, chaseSpeed * Time.deltaTime);
            if (GameManager.Instance.Player.gameObject.layer != LayerMask.NameToLayer("HidingZone"))
            {
                isChasing = true;
                detectToWaiting = false;

            }
        }

        else
        {
            float targetVelocityX = _direction.x * moveSpeed;
            velocity.x = isWaiting ? 0 : Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? 0.1f : 0.2f);


            velocity.y += -gravity * Time.deltaTime;
            if (isStop)
                velocity = Vector2.zero;
            controller.Move(velocity * Time.deltaTime, false);

            if (controller.collisions.above || controller.collisions.below)
                velocity.y = 0;
        }
    }

    void HandleAnimation()
    {
        animator.SetFloat("speed", Mathf.Abs(velocity.x));
    }


    IEnumerator CheckMeleeAttack(float delay)
    {
        animator.SetTrigger("attack");
        isPlaying = false;

        yield return new WaitForSeconds(delay);

        if (isSocking)
        {
            isPlaying = true;
            yield break;
        }

        var hit = Physics2D.CircleCast(meleePoint.position, meleeAttackZone, Vector2.zero, 0, GameManager.Instance.playerLayer);

        if (!hit)
        {
            isPlaying = true;
            yield break;
        }
        
        var damage = (ICanTakeDamage)hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage));
        if (damage == null)
        {
            isPlaying = true;
            yield break;
        }
        damage.TakeDamage(meleeDamage, Vector2.zero, gameObject, Vector2.zero);

        yield return new WaitForSeconds(attackRate);
        isPlaying = true;
    }

    public void SetForce(float x, float y)
    {
        velocity = new Vector3(x, y, 0);
    }
    
    private void FireProjectile()
    {
        animator.SetTrigger("throw");
        //Invoke("FireBulletDelay", fireDelay);
    }
    
    //called by animation event
    void AnimFire()
    {
        var _projectile = (Projectile)Instantiate(rangeprojectile, rangePoint.position, Quaternion.identity);
        _projectile.Initialize(gameObject, _direction, Vector2.zero, false,false,bulletDamage,bulletSpeed);
        SoundManager.PlaySfx(soundRangAttack);
    }

    //called by animation event
    void AnimSpawn()
    {
        //yield return new WaitForSeconds (fireDelay);
        activeSpawnedObj.Add(Instantiate(spawnObj, spawnPoint.position, spawnObj.transform.rotation));
        SoundManager.PlaySfx(soundSpawnttack);
    }

    //This action is called by the Input/ControllerInput
    public void ThrowGrenade()
    {
        if (_Grenade == null)
            return;
        
        GameObject obj = Instantiate(_Grenade, throwPosition.position, Quaternion.identity) as GameObject;
        obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, transform.localScale.x < 1 ? angleThrow : 180 - angleThrow));
        
        obj.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(throwForce, 0));

        animator.SetTrigger("throw");
    }


    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        //Debug.LogError(damage);
        if (isDead)
            return;

        pushForce = force;

        //if (HurtEffect != null)
        //    Instantiate(HurtEffect, instigator.transform.position, Quaternion.identity);
        if (HurtEffect)
            SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = instigator.transform.position;

        currentHealth -= damage;

        if (healthBar)
            healthBar.UpdateValue(currentHealth / (float)health);

        if (currentHealth <= 0)
        {
            isDead = true;
        }


        if (instigator && instigator.GetComponent<Block>() != null)
            isDead = true;

        if (isDead)
        {
            Dead();
        }
        else
            HitEvent();

    }

    protected virtual void HitEvent()
    {

        SoundManager.PlaySfx(hurtSound, hurtSoundVolume);
        //if (HurtEffect != null)
        //    Instantiate(HurtEffect, transform.position, transform.rotation);
        if (HurtEffect)
            SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = transform.position;
        //		StopAllCoroutines ();
        animator.SetTrigger("hurt");
        StartCoroutine(PushBack(sockingTime));
    }

    protected virtual void Dead()
    {
        isPlaying = false;

        StopAllCoroutines();
        SoundManager.PlaySfx(deadSound, deadSoundVolume);

        if (BonusItem != null)
            Instantiate(BonusItem, transform.position, transform.rotation);

        animator.SetTrigger("die");

        velocity.x = 0;
        GetComponent<BoxCollider2D>().enabled = false;
        
        //try spawn random item
        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
        {
            spawnItem.SpawnItem();
        }

        if (dieBehavior == DIEBEHAVIOR.BLOWUP)
        {
            if (DestroyEffect)
            {
                SpawnSystemHelper.GetNextObject(DestroyEffect, true).transform.position = transform.position + Vector3.up * 0.5f;
            }
            DestroyObject();
        }
        else if (dieBehavior == DIEBEHAVIOR.FALLOUT)
        {
            controller.HandlePhysic = false;
            velocity = new Vector2(0, 8);
            Invoke("DestroyObject", 1);
        }
        else
        {
            Invoke("DestroyObject", 1);
        }

        //try remove path moving
        var havePathMoving = GetComponent<SimplePathedMoving>();
        if (havePathMoving)
            Destroy(havePathMoving);
    }

    void DestroyObject()
    {
       
        Destroy(gameObject);
    }

    protected virtual void OnRespawn()
    {

    }
    
    public IEnumerator PushBack(float delay)
    {
        isSocking = true;
        SetForce(GameManager.Instance.Player.transform.localScale.x * pushForce.x, pushForce.y);

        if (isDead)
        {
            Dead();
            yield break;
        }

        yield return new WaitForSeconds(delay);

        SetForce(0, 0);
        isSocking = false;
        isPlaying = true;
    }

    void OnDrawGizmos()
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
            DestroyImmediate(GetComponent<SmartProjectileAttack>());
    }

    #region IListener implementation

    public void IPlay()
    {
        //		throw new System.NotImplementedException ();
    }

    public void ISuccess()
    {
        //		throw new System.NotImplementedException ();
    }

    public void IPause()
    {
        //		throw new System.NotImplementedException ();
    }

    public void IUnPause()
    {
        //		throw new System.NotImplementedException ();
    }

    public void IGameOver()
    {
        //		throw new System.NotImplementedException ();
    }

    public void IOnRespawn()
    {
        //		throw new System.NotImplementedException ();
    }

    public void IOnStopMovingOn()
    {
        if (this)
        {
           
            animator.enabled = false;
            isStop = true;
        }
    }

    public void IOnStopMovingOff()
    {
        if (this)
        {
            animator.enabled = true;
            isStop = false;
        }
    }

    #endregion
}
