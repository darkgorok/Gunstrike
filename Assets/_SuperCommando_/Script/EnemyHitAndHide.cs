using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Controller2D))]
public class EnemyHitAndHide : MonoBehaviour, ICanTakeDamage
{
    public DIEBEHAVIOR dieBehavior;
    public float showRandomMin = 1;
    public float showRandomMax = 2;
    public Projectile projectile;
    public Transform throwPoint;

    public GameObject DestroyEffect;        //spawn object when dead
    public float destroyTime = 1.5f;
    [Header("Health")]
    [Range(0, 100)]
    public float health = 50;
    float currentHealth;
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    [ReadOnly] public HealthBarEnemyNew healthBar;

    public AudioClip soundHit, soundDead, soundThrow;

    CheckTargetHelper checkTargetHelper;
    Animator anim;
    Collider2D col;
    bool isAttacking = false;
    [HideInInspector]
    protected Vector3 velocity;
    [HideInInspector]
    public Controller2D controller;

    public bool isFacingRight()
    {
        return transform.rotation.y == 0 ? true : false;
    }

    void OnEnable()
    {
        isAttacking = false;
    }

    void Start()
    {
        controller = GetComponent<Controller2D>();
        currentHealth = health;
        checkTargetHelper = GetComponent<CheckTargetHelper>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>(); 
        col.enabled = false;

        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);

        healthBar.Init(transform, (Vector3)healthBarOffset);
    }

    bool isDetectPlayer = false;
    void Update()
    {
        isDetectPlayer = checkTargetHelper.CheckTarget();
        //Debug.LogError(isDetectPlayer + "x" + isAttacking);
        if (isDetectPlayer && !isAttacking)
        {
            StartCoroutine(AttacktingCo());
        }
    }

    void LateUpdate()
    {
                velocity.y += -35 * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime, false);
    }

    IEnumerator AttacktingCo()
    {
        isAttacking = true;

        while (!isDetectPlayer) { yield return new WaitForSeconds(0.1f); }

        anim.SetBool("showUp", true);
        col.enabled = true;
        yield return new WaitForSeconds(0.5f);

        if ((isFacingRight() && transform.position.x > GameManager.Instance.Player.transform.position.x) || (!isFacingRight() && transform.position.x < GameManager.Instance.Player.transform.position.x))
            Flip();
        if (isDead)
            yield break;
        anim.SetTrigger("throw");
        
        yield return new WaitForSeconds(1.5f);
        anim.SetBool("showUp", false);
        col.enabled = false;
        yield return new WaitForSeconds(Random.Range(showRandomMin, showRandomMax));
        isAttacking = false;
    }

    public void AnimThrow()
    {
        if (isDead)
            return;
        SoundManager.PlaySfx(soundThrow);
        //Instantiate(throwObj, throwPoint.position, Quaternion.identity);
        var _projectile = (Projectile)Instantiate(projectile, throwPoint.position, Quaternion.identity);
        Vector2 _dir = GameManager.Instance.Player.transform.position + Vector3.up - throwPoint.position;
        _projectile.Initialize(gameObject, _dir, Vector2.zero, false);
    }

    void Flip()
    {
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, isFacingRight() ? 180 : 0, transform.rotation.z));
    }

    bool isDead;
    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (healthBar)
            healthBar.UpdateValue(currentHealth / (float)health);

        if (currentHealth <= 0)
        {
            isDead = true;
        }

        if (isDead)
        {
            SoundManager.PlaySfx(soundDead);
            StopAllCoroutines();
            var rig = gameObject.AddComponent<Rigidbody2D>();
            rig.isKinematic = false;
            rig.gravityScale = 2;
           
            rig.linearVelocity = new Vector2(0, 5);
            //Destroy(gameObject, 1.5f);

            var col = gameObject.GetComponent<Collider2D>();
            if (col) col.enabled = false;

            //enabled = false;
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
        }else
            SoundManager.PlaySfx(soundHit);
    }

    void DestroyObject()
    {
        Destroy(gameObject);
    }
}
