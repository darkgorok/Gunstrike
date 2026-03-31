using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public enum DoggeType { OverObject, HitObject }
public enum GunHandlerState { AVAILABLE, SWAPPING, RELOADING, EMPTY }
public enum ShootingMethob { SingleShoot, AutoShoot }

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour, ICanTakeDamage, IListener {
    public Animator anim;

    public Collider2D standUpCollider;      //enable this collider 2d if the player lie down on the ground

    [Header("Moving")]
	public float moveSpeed = 3;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;

	[Header("Jump")]
	public float maxJumpHeight = 3;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	public int numberOfJumpMax = 1;
	int numberOfJumpLeft;
	public GameObject JumpEffect;
    public GameObject landingFX;

    [Header("Health")]
	public int maxHealth;
	public int Health{ get; private set;}
	public GameObject HurtEffect;
    public GameObject respawnFX;

    [Header("TAKE DAMAGE")]
    public float rateGetDmg = 0.5f;
    public Color blinkingColor = Color.green;
    [ReadOnly] public bool isBlinking = false;
    public float knockbackForce = 10f;

    [Header("Sound")]
    public AudioClip respawnSound;
	public AudioClip[] jumpSound;
	[Range(0,1)]
	public float jumpSoundVolume = 0.5f;
	public AudioClip landSound;
	[Range(0,1)]
	public float landSoundVolume = 0.5f;
	public AudioClip[] hurtSound;
	[Range(0,1)]
	public float hurtSoundVolume = 0.5f;
	public AudioClip[] deadSound;
	[Range(0,1)]
	public float deadSoundVolume = 0.5f;
    bool isPlayedLandSound;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	[HideInInspector]
	public Vector3 velocity;
	float velocityXSmoothing;

	[ReadOnly] public bool isFacingRight;

    [ReadOnly] public Vector2 input;
    bool isDead = false;

    [HideInInspector]
	public Controller2D controller;

	public bool isPlaying { get; private set;}
	public bool isFinish { get; set;}
    public bool isGrounded { get { return controller.collisions.below; } }
    bool forceStannding = false;

    void Awake()
    {
        controller = GetComponent<Controller2D>();
        if (anim == null)
            anim = GetComponent<Animator>();
    }

	void Start() {

        CameraFollow.Instance.manualControl = true;
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);

		isFacingRight = transform.localScale.x > 0;
		Health = maxHealth;
		numberOfJumpLeft = numberOfJumpMax;

        gunTypeID = GunManager.Instance.getGunID();
        SetGun(gunTypeID);
        GunManager.Instance.ResetGunBullet();

        grenadeRemaining = maxGrenade;

        if (GlobalValue.currentGunTypeID != null)
            GunManager.Instance.SetNewGunDuringGameplay(GlobalValue.currentGunTypeID);
    }

    void Update() {
        
        if (isFrozen)
            return;

		HandleAnimation ();
        GetInput();

        standUpCollider.enabled = !isLieDown;

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

        if (Mathf.Abs(input.x) < 0.3f)
            velocity.x = 0;

        velocity.y += gravity * Time.deltaTime;

        if (controller.collisions.below && !isPlayedLandSound) {
            isPlayedLandSound = true;
			SoundManager.PlaySfx (landSound, landSoundVolume);
            if (landingFX)
                SpawnSystemHelper.GetNextObject(landingFX, true).transform.position = transform.position;
		} else if (!controller.collisions.below && isPlayedLandSound)
			isPlayedLandSound = false;

        if (isBlinking || !isPlaying)
        {
            ;
        }
        else
        {
           
                gameObject.layer = LayerMask.NameToLayer("Player");
        }

        if (controller.collisions.above)
        {
            CheckBlock();
        }

        if ((transform.position.y + 2) < Camera.main.ViewportToWorldPoint(Vector3.zero).y)
            GameManager.Instance.GameOver();
    }

   void CheckBelow()
    {
        if (controller.collisions.ClosestHit.collider != null)
        {
            var standObj = (IStandOnEvent)controller.collisions.ClosestHit.collider.gameObject.GetComponent(typeof(IStandOnEvent));
            if (standObj != null)
                standObj.StandOnEvent(gameObject);
        }
    }

    void CheckBlock()
    {
        Block isBlock;
        BrokenTreasure isTreasureBlock;
        var bound = controller.boxcollider.bounds;

        var hit = Physics2D.Raycast(new Vector2((bound.min.x + bound.max.x) / 2f, bound.max.y), Vector2.up, 0.5f, 1 << LayerMask.NameToLayer("Platform"));

        if (hit)
        {
            isBlock = hit.collider.gameObject.GetComponent<Block>();
            if (isBlock)
            {
                isBlock.BoxHit();
            }

            isTreasureBlock = hit.collider.gameObject.GetComponent<BrokenTreasure>();
            if (isTreasureBlock)
            {
                isTreasureBlock.BoxHit();
            }
        }

        //check left
        hit = Physics2D.Raycast(new Vector2(bound.min.x, bound.max.y), Vector2.up, 0.5f, 1 << LayerMask.NameToLayer("Platform"));
        if (hit)
        {
            isBlock = hit.collider.gameObject.GetComponent<Block>();
            if (isBlock)
            {
                isBlock.BoxHit();
                //return;
            }

            isTreasureBlock = hit.collider.gameObject.GetComponent<BrokenTreasure>();
            if (isTreasureBlock)
            {
                isTreasureBlock.BoxHit();
                //return;
            }
        }

        hit = Physics2D.Raycast(new Vector2(bound.max.x, bound.max.y), Vector2.up, 0.5f, 1 << LayerMask.NameToLayer("Platform"));
        if (hit)
        {
            isBlock = hit.collider.gameObject.GetComponent<Block>();
            if (isBlock)
            {
                isBlock.BoxHit();
                //return;
            }

            isTreasureBlock = hit.collider.gameObject.GetComponent<BrokenTreasure>();
            if (isTreasureBlock)
            {
                isTreasureBlock.BoxHit();
                //return;
            }
        }
    }
    void GetInput()
    {
        input = new Vector2(ControllerInput.Instance.Horizontak + Input.GetAxis("Horizontal"), ControllerInput.Instance.Vertical + Input.GetAxis("Vertical"));

        if ((input.x < -0.2f && isFacingRight) || (input.x > 0.2f && !isFacingRight))
            Flip();
    }

    void LateUpdate(){
        if (isFrozen)
            return;
        
        if ((controller.raycastOrigins.bottomLeft.x < CameraFollow.Instance._min.x && velocity.x<0) || (controller.raycastOrigins.bottomRight.x > CameraFollow.Instance._max.x && velocity.x > 0))
            velocity.x = 0;
        
        if (forceStannding)
            velocity.x = 0;

        if (GameManager.Instance.State != GameManager.GameState.Playing)
            velocity.x = 0;

        if (controller.raycastOrigins.bottomLeft.y < CameraFollow.Instance._min.y)
            GameManager.Instance.GameOver();

        controller.Move (velocity * Time.deltaTime, input);
        if(!isDead)
        CameraFollow.Instance.DoFollowPlayer();


        if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}
        
        if (controller.collisions.below)
        { 
            numberOfJumpLeft = 0;
            CheckBelow();       //check the object below if it have Stand on event
        }

        lastPosX = transform.position.x;
        
    }

    public void PausePlayer(bool pause)
    {
        StopMove();
        isPlaying = !pause;
    }

    public bool isFrozen { get; set; }  //player will be frozen
    public void Frozen(bool is_enable)
    {
        input = Vector2.zero;
        velocity = Vector2.zero;
        isFrozen = is_enable;
        anim.enabled = !is_enable;
    }

	private void Flip(){
        if (forceStannding)
            return;
        
        transform.localScale = new Vector3 (transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
		isFacingRight = transform.localScale.x > 0;
	}


	public void MoveLeft(){
		if (isPlaying) {
			input = new Vector2 (-1, 0);
			if (isFacingRight)
				Flip ();
		}
	}


	public void MoveRight(){
		if (isPlaying) {
			input = new Vector2 (1, 0);
			if (!isFacingRight)
				Flip ();
		}
	}


	public void StopMove(){
		input = Vector2.zero;
	}

	public void FallDown(){
		input = new Vector2 (0, -1);
	}


    public void Jump()
    {
        if (!isPlaying)
            return;

        if (forceStannding)
            return;

         if (controller.collisions.below)
        {
            if(input.y == -1)
            {
                controller.JumpDown();
                return;
            }

            velocity.y = maxJumpVelocity;

            if (JumpEffect)
                SpawnSystemHelper.GetNextObject(JumpEffect, true).transform.position = transform.position;

            SoundManager.PlaySfx(jumpSound, jumpSoundVolume);
            numberOfJumpLeft = numberOfJumpMax;
        }
        else
        {
            numberOfJumpLeft--;
            if (numberOfJumpLeft > 0)
            {
                anim.SetTrigger("doubleJump");
                velocity.y = minJumpVelocity;

                if (JumpEffect)
                    SpawnSystemHelper.GetNextObject(JumpEffect, true).transform.position = transform.position;
                SoundManager.PlaySfx(jumpSound, jumpSoundVolume);
            }
        }
    }
    
	public void JumpOff(){
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}

    #region FORCE STANDING
     
    public void ForceStanding(float delay)
    {
        if (ForceStandingCoDo != null)
            StopCoroutine(ForceStandingCoDo);

        ForceStandingCoDo = ForceStandingCo(delay);
        StartCoroutine(ForceStandingCoDo);
    }

    IEnumerator ForceStandingCoDo;

    IEnumerator ForceStandingCo(float delay)
    {
        forceStannding = true;
        input.x = 0;
        velocity.x = 0;
        yield return new WaitForSeconds(delay);
        forceStannding = false;
    }

    #endregion


    public void SetForce(Vector2 force, bool springPush = false)
    {
        if (!springPush && isBlinking)
            return;

        if (springPush)
        {
            numberOfJumpLeft = numberOfJumpMax;
        }

        velocity = (Vector3)force;
    }

	public void AddForce(Vector2 force){
		velocity += (Vector3) force;
	}


	public void RespawnAt(Vector2 pos){
		transform.position = pos;
        if (respawnFX)
            Instantiate(respawnFX, pos, respawnFX.transform.rotation);
		isPlaying = true;
        isDead = false;
        Health = maxHealth;

        SoundManager.PlaySfx(respawnSound, 0.8f);

        ResetAnimation ();

        controller.HandlePhysic = true;

        StartCoroutine(BlinkingCo(1.5f));
	}

    int aim_angle = 0;
    bool isLieDown = false;
    void HandleAnimation()
    {

        //set animation state
        anim.SetFloat("speed", Mathf.Abs(velocity.x));
        anim.SetFloat("height_speed", velocity.y);
        anim.SetBool("isGrounded", controller.collisions.below);
        anim.SetFloat("inputY", input.y);
        anim.SetFloat("inputX", input.x);

        isLieDown = Mathf.Abs(velocity.x) < 0.1f && input.x < 0.3f && input.y < -0.7f && isGrounded;
        anim.SetBool("isLieDown", isLieDown);
        if (GameManager.Instance.State == GameManager.GameState.Playing)
        {
            //isLieDown = Mathf.Abs(velocity.x) < 0.1f && input.x < 0.3f && input.y < -0.7f && isGrounded;
            //anim.SetBool("isLieDown", isLieDown);

            var _tempInput = input;
            _tempInput.Normalize();
            if (_tempInput == Vector2.zero)
                aim_angle = 0;
            else if (_tempInput.y > 0.9f)
            {
                aim_angle = 90;
            }
            else if (_tempInput.y < -0.9f)
            {
                aim_angle = -90;
            }
            else
            {
                if (_tempInput.y < 0.5f && _tempInput.y > -0.5f)
                {
                    if (_tempInput.x > 0.5f || _tempInput.x < -0.5f)
                    {
                        aim_angle = 0;
                    }
                }
                else if (_tempInput.y > 0.5f)
                {
                    if (_tempInput.x > 0.5f || _tempInput.x < -0.5f)
                    {
                        aim_angle = 45;
                    }
                }
                else if (_tempInput.y < -0.5f)
                {
                    if (_tempInput.x > 0.5f || _tempInput.x < -0.5f)
                    {
                        aim_angle = -45;
                    }
                }
            }
            anim.SetInteger("lookAngle", aim_angle);
        }
    }

	void ResetAnimation(){
		anim.SetFloat ("speed", 0);
		anim.SetFloat ("height_speed", 0);
		anim.SetBool ("isGrounded", true);
		anim.SetBool ("isWall", false);
		anim.SetTrigger ("reset");
	}

	public void GameFinish(){
		StopMove ();
		isPlaying = false;
		anim.SetTrigger ("finish");
	}

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!isPlaying || isBlinking)
            return;

		SoundManager.PlaySfx (hurtSound, hurtSoundVolume);
        if (HurtEffect)
            SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = hitPoint == Vector3.zero ? instigator.transform.position : hitPoint;

        

        if (Health <= 0)
            GameManager.Instance.GameOver();
        else
        {
            anim.SetTrigger("hurt");
            StartCoroutine(BlinkingCo(rateGetDmg));
        }

        
        if (instigator != null)
        {
            int dirKnockBack = (instigator.transform.position.x > transform.position.x) ? -1 : 1;
            SetForce(new Vector2(knockbackForce * dirKnockBack, 0));
        }
    }

    IEnumerator BlinkingCo(float time)
    {
        isBlinking = true;
        int blink = (int)(time * 0.5f / 0.1f);
        for (int i = 0; i < blink; i++)
        {
            yield return new WaitForSeconds(0.1f);
            yield return new WaitForSeconds(0.1f);
        }

        isBlinking = false;
    }

	public void GiveHealth(int hearthToGive, GameObject instigator){
		Health = Mathf.Min (Health + hearthToGive, maxHealth);
	}
    
    public void Kill()
    {
        if (isPlaying)
        {
            isPlaying = false;
            forceStannding = false;
            isDead = true;
            StopAllCoroutines();
            StopMove();
            SoundManager.PlaySfx(deadSound, deadSoundVolume);
            anim.SetTrigger("dead");
            SetForce(new Vector2(0, 7f));
            Health = 0;
        }
    }

    [Space]
    [Header("HIT EFFECT")]
    public bool playEarthQuakeOnHitDogge = true;
    public float _eqTime = 0.1f;
    public float _eqSpeed = 60;
    public float _eqSize = 1;
    [Space]
    
    float lastPosX;

    #region TELEPORT
    public void Teleport(Transform newPos, float timer)
    {
        StartCoroutine(TeleportCo(newPos, timer));
    }


    IEnumerator TeleportCo(Transform newPos, float timer)
    {
        StopMove();
        isPlaying = false;
        Color color = Color.white;

        float transparentSpeed = 3;
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= (Time.deltaTime * transparentSpeed);
            
            color.a = Mathf.Clamp01(alpha);
            yield return null;
        }

        transform.position = newPos.position;
        yield return new WaitForSeconds(timer);

        isPlaying = true;
        yield return null;
        isPlaying = false;

        alpha = 0;
        while (alpha < 1)
        {
            alpha += (Time.deltaTime * transparentSpeed);
            color.a = Mathf.Clamp01(alpha);
            yield return null;
        }

        color.a = 1;

        isPlaying = true;
    }
    #endregion
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPlaying)
            return;

        var isTriggerEvent = collision.GetComponent<TriggerEvent>();
        if (isTriggerEvent != null)
            isTriggerEvent.OnContactPlayer();

        if (collision.CompareTag("Checkpoint"))
        {
            var hitGround = Physics2D.Raycast(collision.transform.position, Vector2.down, 100, GameManager.Instance.groundLayer);
            
            if (hitGround)
                GameManager.Instance.SaveCheckPoint(hitGround.point);
            else
                GameManager.Instance.SaveCheckPoint(collision.transform.position);

        }

        if (collision.CompareTag("DeadZone"))
            GameManager.Instance.GameOver();

        var isCollectItem = (ICanCollect)collision.GetComponent(typeof(ICanCollect));
        if (isCollectItem != null)
        {
            isCollectItem.Collect();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isPlaying)
            return;

        var itemType = collision.GetComponent<ItemType>();
        if (itemType)
            itemType.Collect();
    }

    public void IPlay()
    {
        isPlaying = true;
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
        Kill();
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

    [Header("WEAPONS")]
    [ReadOnly] public GunHandlerState GunState;
    [ReadOnly] public GunTypeID gunTypeID;
    public Transform firePoint;
    float lastTimeShooting = -999;
    bool allowShooting = true;
    protected HealthBarEnemyNew healthBar;

    public void AnimSetTrigger(string name)
    {
        anim.SetTrigger(name);
    }

    public void AnimSetSpeed(float value)
    {
        if (anim)
            anim.speed = value;
    }

    public void AnimSetFloat(string name, float value)
    {
        anim.SetFloat(name, value);
    }

    public void AnimSetBool(string name, bool value)
    {
        anim.SetBool(name, value);
    }

    public void SetState(GunHandlerState state)
    {
        GunState = state;

    }

    bool isShooting = false;

    public void Shoot(bool hold)
    {
        if (!hold)
        {
            isShooting = false;
            return;
        }
        else if (isShooting && gunTypeID.shootingMethob == ShootingMethob.SingleShoot)
            return;

        isShooting = true;

        if (!allowShooting || gunTypeID.bullet <= 0)
            return;

        if (Time.time < (lastTimeShooting + gunTypeID.rate))
            return;

        if (GunState != GunHandlerState.AVAILABLE)
            return;

        lastTimeShooting = Time.time;
        if (!gunTypeID.unlimitedBullet)
            gunTypeID.bullet--;

        AnimSetTrigger("shot");

        StartCoroutine(FireCo(true));

        //for spread bullet
        int _right = 0;
        int _left = 0;

        for (int i = 0; i < gunTypeID.maxBulletPerShoot; i++)
        {
            var direction = isFacingRight ? Vector2.right : Vector2.left;

            if (aim_angle == 45)
                direction = new Vector2(isFacingRight ? 1 : -1, 1);
            else if (aim_angle == 90)
                direction = Vector2.up;
            else if (aim_angle == -45)
                direction = new Vector2(isFacingRight ? 1 : -1, -1);
            else if (aim_angle == -90 && !isGrounded)
                direction = Vector2.down;

            var projectile = SpawnSystemHelper.GetNextObject(gunTypeID.bulletPrefab.gameObject, false).GetComponent<Projectile>();
            projectile.transform.position = firePoint.position;
            var localRandom = new Vector3(0, Random.Range(-0.1f, 0.1f), 0);
            projectile.transform.localPosition += localRandom;
            projectile.transform.right = direction;

            if (gunTypeID.isSpreadBullet)
            {
                if (i != 0)
                {
                    if (i % 2 == 1)
                    {
                        _right++;
                        projectile.transform.Rotate(Vector3.forward, 10 * _right);
                    }
                    else
                    {
                        _left++;
                        //direction -= Vector2.up * _right * 0.2f;
                        projectile.transform.Rotate(Vector3.forward, -10 * _left);
                    }
                }

                direction = projectile.transform.right;
            }

            projectile.Initialize(gameObject, direction, Vector2.zero, false, gunTypeID.useTrack, gunTypeID.damage, gunTypeID.bulletSpeed);
            projectile.gameObject.SetActive(true);

            if (gunTypeID.muzzleFX)
            {
                var _muzzle = SpawnSystemHelper.GetNextObject(gunTypeID.muzzleFX, firePoint.position, true);
                _muzzle.transform.right = direction;
                _muzzle.transform.localPosition += localRandom;
            }
        }

        SoundManager.PlaySfx(gunTypeID.soundFire, gunTypeID.soundFireVolume);

        CancelInvoke("CheckBulletRemain");
        Invoke("CheckBulletRemain", gunTypeID.rate);
    }

    public IEnumerator FireCo(bool isFirstShot)
    {
        yield return null;

        if (gunTypeID.reloadPerShoot)
        {
            StartCoroutine(ReloadGunSub());
        }
    }

    void CheckBulletRemain()
    {
        if (gunTypeID.bullet <= 0)
        {
            GlobalValue.currentGunTypeID = null;
            GunManager.Instance.BackToDefaultGun();
        }
    }

    public void ReloadGun()
    {
        SetState(GunHandlerState.RELOADING);
        //SoundManager.PlaySfx (soundReload, soundReloadVolume);
        AnimSetTrigger("reload");
        AnimSetBool("reloading", true);
        Invoke("ReloadComplete", gunTypeID.reloadTime);

        SoundManager.PlaySfx(gunTypeID.reloadSound, gunTypeID.reloadSoundVolume);
    }

    IEnumerator ReloadGunSub()
    {
        SetState(GunHandlerState.RELOADING);
        AnimSetBool("isReloadPerShootNeeded", true);

        yield return new WaitForSeconds(gunTypeID.reloadTime);

        SetState(GunHandlerState.AVAILABLE);
        AnimSetBool("isReloadPerShootNeeded", false);
    }

    public void ReloadComplete()
    {
        lastTimeShooting = Time.time;
        AnimSetBool("reloading", false);
        SetState(GunHandlerState.AVAILABLE);
    }

    public void SetGun(GunTypeID gunID)
    {
        anim.runtimeAnimatorController = gunID.animatorOverride;
        gunTypeID = gunID;
        AnimSetTrigger("swap-gun");
        allowShooting = false;
        SoundManager.PlaySfx(SoundManager.Instance.swapGun);
        Invoke("AllowShooting", 0.3f);
    }
   

    void AllowShooting()
    {
        allowShooting = true;
    }

    [Header("GRENADE")]
    public int maxGrenade = 6;
    [ReadOnly] public int grenadeRemaining = 0;
    public Grenade grenade;
    public Transform throwPoint;
    public float throwForce = 600;
    public int grenade_damage = 100;
    public float grenade_radius = 2;

    public void ThrowGrenade()
    {
        if (grenadeRemaining > 0)
        {
            grenadeRemaining--;
            anim.SetTrigger("throw");
            Vector3 throwPos = throwPoint.position;
            var obj = (Grenade)Instantiate(grenade, throwPos, Quaternion.identity);
            obj.Init(grenade_damage, grenade_radius);

            obj.transform.right = new Vector2(isFacingRight ? 1 : -1, 0.75f);

            obj.GetComponent<Rigidbody2D>().AddRelativeForce(obj.transform.right * (throwForce + (Mathf.Abs(velocity.x / moveSpeed)) * throwForce * 0.3f));
            obj.GetComponent<Rigidbody2D>().AddTorque(obj.transform.right.x * 10);
        }
    }

    public void AddGrenade(int amount)
    {
        grenadeRemaining += amount;
        grenadeRemaining = Mathf.Min(grenadeRemaining, maxGrenade);
    }
}
