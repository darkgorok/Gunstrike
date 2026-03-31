using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class EnemyAI : MonoBehaviour, ICanTakeDamage {
	[Header("Behavior")]
    public DIEBEHAVIOR dieBehavior;
    public float gravity = 35f;
	[Tooltip("allow push the enemy back when hit by player")]
	public bool pushEnemyBack = true;
	Vector2 pushForce;
	//public GameObject spawnItemWhenDead;

	[Header("Moving")]
	public float moveSpeed = 3;
	public bool ignoreCheckGroundAhead = false;
	public GameObject DestroyEffect;
    public bool moveFastWhenDetectPlayer = false;
    public float movingDetectPlayerDistance = 5;
    public float moveFastMultiple = 2;
    Vector2 originalPos;

    public enum HealthType{HitToKill, HealthAmount, Immortal}
	[Header("Health")]

	public HealthType healthType;
	public int maxHitToKill = 1;
	[HideInInspector]
	public int currentHitLeft;

	public float health;
	float currentHealth;
	public int pointToGivePlayer;
	public GameObject HurtEffect;

	[Header("Sound")]
	public AudioClip hurtSound;
	[Range(0,1)]
	public float hurtSoundVolume = 0.5f;
	public AudioClip deadSound;
	[Range(0,1)]
	public float deadSoundVolume = 0.5f;

	[Header("Projectile")]
	public bool isUseProjectile;
	public LayerMask shootableLayer;
	public Transform PointSpawn;
	public Projectile projectile;
    public AudioClip fireSound;
	public float fireRate = 1f;
	public float detectDistance = 10f;
	float _fireIn;

	public bool isPlaying{ get; set; }
	public bool isSocking{ get; set; }
	public bool isDead{ get; set; }

	private Vector3 velocity;
	private Vector2 _direction;
	private Vector2 _startPosition;	//set this enemy back to the first position when Player spawn to check point
	private Vector2 _startScale;	//set this enemy back to the first position when Player spawn to check point
	[HideInInspector]
	public Controller2D controller;
    protected float velocityXSmoothing = 0;

    // Use this for initialization
    public virtual void Start () {
		controller = GetComponent<Controller2D> ();
		_direction = Vector2.left;
		_startPosition = transform.position;
		_startScale = transform.localScale;
		_fireIn = fireRate;
		currentHealth = health;
		currentHitLeft = maxHitToKill;
        originalPos = transform.position;

        isPlaying = true;
		isSocking = false;
	}
	
	// Update is called once per frame
	public virtual void Update () {
		if (GameManager.Instance.State == GameManager.GameState.Finish)
			enabled = false;
		
		if (!isPlaying || isSocking)
			return;

		_fireIn -= Time.deltaTime;

        if ((_direction.x > 0 && controller.collisions.right) || (_direction.x < 0 && controller.collisions.left)
            || (!ignoreCheckGroundAhead && !controller.isGrounedAhead(_direction.x > 0) && controller.collisions.below))
        {

            _direction = -_direction;
            velocity.x = 0;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }



		if (isUseProjectile) {
			var position = PointSpawn != null ? PointSpawn.position : transform.position;
			var hit = Physics2D.Raycast (position, _direction, detectDistance, shootableLayer);
			if (hit) {
				if (hit.collider.gameObject.GetComponent<Player> () != null)
					FireProjectile ();
			}
		}
	}

	public virtual void LateUpdate(){
        if (GameManager.Instance.State != GameManager.GameState.Playing)
            return;
         
        if (!isPlaying)
        {
            if (isDead && dieBehavior == DIEBEHAVIOR.FALLOUT)
            {
                velocity.y += -35 * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime, false);
            }
            return;
        }

        float targetVelocityX = _direction.x * moveSpeed;
        if (!isPlaying || isSocking)
        {
            targetVelocityX = 0;
        }else if (moveFastWhenDetectPlayer)
        {
            //Gizmos.DrawWireCube(transform.position + Vector3.up * detectPlayerSize.y * 0.5f, detectPlayerSize);
            if (Physics2D.Linecast(transform.position + Vector3.left * movingDetectPlayerDistance + Vector3.up * 0.5f, transform.position + Vector3.right * movingDetectPlayerDistance + Vector3.up * 0.5f, GameManager.Instance.playerLayer))
            {
                targetVelocityX *= moveFastMultiple;
            }
        }

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? 0.1f : 0.2f);
        
        velocity.y += -gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime, false);

		if (controller.collisions.above || controller.collisions.below)
			velocity.y = 0;
	}

	public void SetForce(float x, float y){
		velocity = new Vector3 (x, y, 0);
	}

	private void FireProjectile(){
		if (_fireIn > 0)
			return;

		_fireIn = fireRate;
		var _projectile = (Projectile) Instantiate (projectile, PointSpawn.position, Quaternion.identity);
		_projectile.Initialize (gameObject, _direction, Vector2.zero,false);
        SoundManager.PlaySfx(fireSound);
	}


	/// <summary>
	/// Takes the damage.
	/// </summary>
	/// <param name="damage">Damage.</param>
	/// <param name="instigator">Instigator.</param>
	public void TakeDamage (int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
	{
        if (healthType == HealthType.Immortal)
            return;

        //Debug.LogError(damage);
		if (isDead)
			return;

		if (instigator.GetComponent<Grenade> ()) {
            //if (HurtEffect != null)
            //	Instantiate (HurtEffect, instigator.transform.position, Quaternion.identity);
            if (HurtEffect)
                SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = instigator.transform.position;

            isDead = true;
			HitEvent ();
			return;
		}
		
		pushForce = force;

        //if (HurtEffect != null)
        //	Instantiate (HurtEffect, instigator.transform.position, Quaternion.identity);
        if (HurtEffect)
            SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = instigator.transform.position;

        if (healthType == HealthType.HitToKill) {
			currentHitLeft--;
			if (currentHitLeft <= 0) {
				isDead = true;
			}
		} else if (healthType == HealthType.HealthAmount) {
			currentHealth -= damage;
			if (currentHealth <= 0) {
				isDead = true;
			}
		}

		if (instigator.GetComponent<Block> () != null)
			isDead = true;

		HitEvent ();

	}

	protected virtual void HitEvent(){
		
		SoundManager.PlaySfx (hurtSound, hurtSoundVolume);
        //if (HurtEffect != null)
        //	Instantiate (HurtEffect, transform.position, transform.rotation);
        if (HurtEffect)
            SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = transform.position;

        StopAllCoroutines ();
		StartCoroutine(PushBack (0.35f));
	}


	protected virtual void Dead(){
		
		isPlaying = false;

		StopAllCoroutines ();
		SoundManager.PlaySfx (deadSound, deadSoundVolume);
		//if (pointToGivePlayer != 0) {
		//	GameManager.Instance.AddPoint (pointToGivePlayer);
		//	GameManager.Instance.ShowFloatingText ("+" + pointToGivePlayer, transform.position, Color.yellow);
		//}

		
        //try spawn random item
        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
        {
            spawnItem.SpawnItem();
        }

        //turn off all colliders if the enemy have
        var boxCo = GetComponents<BoxCollider2D> ();
		foreach (var box in boxCo) {
			box.enabled = false;
		}
		var CirCo = GetComponents<CircleCollider2D> ();
		foreach (var cir in CirCo) {
			cir.enabled = false;
		}

        if (dieBehavior == DIEBEHAVIOR.BLOWUP)
        {
            if (DestroyEffect != null)
                SpawnSystemHelper.GetNextObject(DestroyEffect, true).transform.position = transform.position;
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
    }

    void DestroyObject()
    {
        
        Destroy(gameObject);
    }

    protected virtual void OnRespawn(){

	}

	

	public IEnumerator PushBack(float delay){
		
		isPlaying = false;
		SetForce (GameManager.Instance.Player.transform.localScale.x * pushForce.x, pushForce.y);

		yield return new WaitForSeconds (delay);
		SetForce (0, 0);

		if (isDead)
			Dead ();
		else
			isPlaying = true;
	}

    public void OnDrawGizmosSelected()
    {
        if (isUseProjectile)
        {
            Gizmos.color = Color.blue;
            if (_direction.magnitude != 0)
                Gizmos.DrawRay(PointSpawn.position, _direction * detectDistance);
            else
                Gizmos.DrawRay(PointSpawn.position, Vector2.left * detectDistance);
        }
        if (moveFastWhenDetectPlayer)
        {
            Gizmos.DrawLine(transform.position + Vector3.left * movingDetectPlayerDistance + Vector3.up * 0.5f, transform.position + Vector3.right * movingDetectPlayerDistance + Vector3.up * 0.5f);
        }

    }
}
