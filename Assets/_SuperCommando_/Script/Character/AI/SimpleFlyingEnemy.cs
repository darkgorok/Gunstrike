using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFlyingEnemy : MonoBehaviour, ICanTakeDamage, IListener{
    public DIEBEHAVIOR dieBehavior;
    public float minX = 3;
	public float maxX = 3;
	public float minY = 3;
	public float maxY = 3;

	public float speedY = 3;
	public float speedX = 5;

	[ReadOnly]public bool isMovingRight = false;
	[ReadOnly]public bool isMovingTop = false;
    [Range(0, 1000)]
    public int health = 100;
    int currentHealth;

    public Vector2 healthBarOffset = new Vector2(0, 1.5f);

    public GameObject DestroyEffect;

	float targetR,targetL,targetT,targetB;
    
	public AudioClip soundHit, soundDead;
    protected HealthBarEnemyNew healthBar;
    [HideInInspector]
    protected Vector3 velocity;
    [HideInInspector]
    public Controller2D controller;
    bool isPlaying = true;
    bool isDead = false;

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.DrawWireCube(new Vector2((transform.position.x - minX + transform.position.x + maxX) * 0.5f, (transform.position.y - minY + transform.position.y + maxY) * 0.5f), new Vector2(minX + maxX, minY + maxY));
        }
    }

    // Use this for initialization
    void Start () {
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

    public bool isFacingRight()
    {
        //		return transform.localScale.x <0 ? true : false;
        return transform.rotation.y == 0 ? true : false;
    }

    void Flip()
    {
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, isFacingRight() ? 180 : 0, transform.rotation.z));
    }

    // Update is called once per frame
    void Update () {
        if (isDead)
        {
            if (dieBehavior == DIEBEHAVIOR.FALLOUT)
            {
                velocity.y += -35 * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime, false);
            }
            return;
        }

        if (!isPlaying || isStop)
			return;
		
		float x, y;
		x = transform.position.x;
		y = transform.position.y;
		//moving horizontal
		if (isMovingRight) {
			transform.Translate (speedX * Time.deltaTime, 0, 0, Space.World);
			if (transform.position.x >= targetR)
				isMovingRight = false;
		} else {
			transform.Translate (-speedX * Time.deltaTime, 0, 0, Space.World);
			if (transform.position.x <= targetL)
				isMovingRight = true;
		}

        //transform.localScale = new Vector3(isMovingRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        if ((isFacingRight() && !isMovingRight) || (!isFacingRight() && isMovingRight))
            Flip();

		if (isMovingTop) {
			y = Mathf.Lerp (y, targetT, speedY * Time.deltaTime);
			if (Mathf.Abs (y - targetT) < 0.1f)
				isMovingTop = false;
		} else {
			y = Mathf.Lerp (y, targetB, speedY * Time.deltaTime);
			if (Mathf.Abs (y - targetB) < 0.1f)
				isMovingTop = true;
		}
		transform.position = new Vector2 (transform.position.x, y);
        healthBar.transform.localScale = new Vector2(transform.localScale.x > 0 ? Mathf.Abs(healthBar.transform.localScale.x) : -Mathf.Abs(healthBar.transform.localScale.x), healthBar.transform.localScale.y);
    }

    [Header("Contact Player")]
    public int makeDamage = 30;
    [Tooltip("delay a moment before give next damage to Player")]
    public float rateDamage = 0.2f;
    //public Vector2 pushPlayer = new Vector2(15, 10);
    float nextDamage;

    IEnumerator OnTriggerStay2D(Collider2D other)
    {
        if (isDead)
            yield break;

        if (this)
        {
            var Player = other.GetComponent<Player>();
            if (Player == null)
                yield break;

            if (!Player.isPlaying)
                yield break;

            if (Player.gameObject.layer == LayerMask.NameToLayer("HidingZone"))
                yield break;

            if (Time.time < nextDamage + rateDamage)
                yield break;

            nextDamage = Time.time;

            if (makeDamage == 0)
                yield break;

            Player.TakeDamage(makeDamage, Vector2.zero, gameObject, transform.position);
        }
    }

    #region ICanTakeDamage implementation


    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (isDead)
            return;

        currentHealth -= (int)damage;
        if (healthBar)
            healthBar.UpdateValue(currentHealth / (float)health);
        if (currentHealth <= 0)
        {
            isDead = true;
            isPlaying = false;
            SoundManager.PlaySfx(soundDead);

            //try spawn random item
            var spawnItem = GetComponent<EnemySpawnItem>();
            if (spawnItem != null)
            {
                spawnItem.SpawnItem();
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
        else
            SoundManager.PlaySfx(soundHit);
    }

    void DestroyObject()
    {
        Destroy(gameObject);
    }

    #endregion

    #region IListener implementation

    public void IPlay ()
	{
		//		throw new System.NotImplementedException ();
	}

	public void ISuccess ()
	{
		//		throw new System.NotImplementedException ();
	}

	public void IPause ()
	{
		//		throw new System.NotImplementedException ();
	}

	public void IUnPause ()
	{
		//		throw new System.NotImplementedException ();
	}

	public void IGameOver ()
	{
		//		throw new System.NotImplementedException ();
	}

	public void IOnRespawn ()
	{
		//		throw new System.NotImplementedException ();
	}
	bool isStop = false;
	public void IOnStopMovingOn ()
	{
		GetComponent<Animator>().enabled =  false;
		isStop = true;
	}

	public void IOnStopMovingOff ()
	{
		GetComponent<Animator>().enabled =  true;
		isStop = false;
	}

	#endregion
}
