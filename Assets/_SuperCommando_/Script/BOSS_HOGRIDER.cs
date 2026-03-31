using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOSS_HOGRIDER : BossManager, ICanTakeDamage, IListener
{
    public float walkSpeed = 1f;
    public float runningSpeed = 3;
    [Range(1, 1000)]
    public int health = 350;
    [ReadOnly] public int currentHealth;
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    protected HealthBarEnemyNew healthBar;
    public float gravity = 35f;

    [Header("EARTH QUAKE")]
    public float _eqTime = 0.3f;
    public float _eqSpeed = 60;
    public float _eqSize = 1.5f;

    [Header("SOUND")]
    public AudioClip attackSound;
    public AudioClip deadSound;
    public AudioClip hurtSound;
    public AudioClip detectSound;
    public AudioClip[] hitWallSound;

    [HideInInspector]
    public bool isDead = false;

    [HideInInspector]
    protected Vector3 velocity;
    protected float velocityXSmoothing = 0;
    Controller2D controller;
    Animator anim;
    [ReadOnly] public bool moving = false;
    bool isRunning = false;

    [ReadOnly] public bool isPlaying = false;
    public bool isFacingRight()
    {
        return transform.rotation.y == 0 ? true : false;
    }
    //HODRIDER ACTION
    [Header("*** HODRIDER ***")]
    public float minDelay = 2;
    public float maxDelay = 4;
    public float stunningTime = 3;
    public GameObject hitWallFX;
    public GameObject stunningFX;
    bool isWaitingAttack = false;

    IEnumerator BOSS_ACTION_CO()
    {
        float delay = Random.Range(0.5f, 1f);
        while (true)
        {
            isWaitingAttack = true;
            while (isWaitingAttack)
            {
                delay -= Time.deltaTime;
                if (delay <= 0)
                    isWaitingAttack = false;

                yield return null;
            }

            //perform attacking
            LookAtPlayer();
            isRunning = true;
            moving = true;
            SoundManager.PlaySfx(attackSound);

            while((_direction.x > 0 && !controller.collisions.right) || (_direction.x < 0 && !controller.collisions.left)) { yield return null; }
            CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);
            anim.SetBool("isStunning", true);

            foreach (var sound in hitWallSound)
                SoundManager.PlaySfx(sound);
            stunningFX.SetActive(true);
            if (hitWallFX)
                Instantiate(hitWallFX, transform.position + Vector3.up * 1.5f, Quaternion.identity);

            yield return new WaitForSeconds(stunningTime);
            SoundManager.PlaySfx(detectSound);
            anim.SetBool("isStunning", false);
            stunningFX.SetActive(false);
            LookAtPlayer();
            moving = false;
            delay = Random.Range(minDelay, maxDelay);
        }
    }

    void LookAtPlayer()
    {
        if ((isFacingRight() && transform.position.x > GameManager.Instance.Player.transform.position.x) || (!isFacingRight() && transform.position.x < GameManager.Instance.Player.transform.position.x))
        {
            Flip();
        }
    }

    void Start()
    {
        controller = GetComponent<Controller2D>();
        anim = GetComponent<Animator>();
        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);

        _direction = isFacingRight() ? Vector2.right : Vector2.left;
        currentHealth = health;

        stunningFX.SetActive(false);
    }

    public override void Play()
    {
        if (isPlaying)
            return;

        isPlaying = true;
        StartCoroutine(BOSS_ACTION_CO());
    }

    void Update()
    {
        anim.SetFloat("speed", Mathf.Abs(velocity.x));

        if (isDead || GameManager.Instance.State != GameManager.GameState.Playing || GameManager.Instance.Player.isFinish)
        {
            velocity.x = 0;

            return;
        }

        float targetVelocityX = _direction.x * (isRunning? runningSpeed: walkSpeed);
        velocity.x = moving ? Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? 0.1f : 0.2f) : 0;
        velocity.y += -gravity * Time.deltaTime;
        if (!moving)
            velocity.x = 0;
    }

    void LateUpdate()
    {
        if (isDead)
            return;

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

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!isPlaying || isDead)
            return;

        currentHealth -= (int)damage;
        isDead = currentHealth <= 0 ? true : false;

        if (healthBar)
            healthBar.UpdateValue(currentHealth / (float)health);

        if (isDead)
        {
            StopAllCoroutines();
            CancelInvoke();

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

            StartCoroutine(BossDieBehavior());

        }
        else
        {
            anim.SetTrigger("hurt");
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
        SoundManager.PlaySfx(deadSound);
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 3; i++)
        {
            if (dieExplosionFX)
                Instantiate(dieExplosionFX, transform.position + new Vector3(Random.Range(-dieExplosionSize.x, dieExplosionSize.x), Random.Range(0, dieExplosionSize.y), 0), Quaternion.identity);
            SoundManager.PlaySfx(dieExplosionSound);
            CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);
            yield return new WaitForSeconds(0.5f);
        }

        BlackScreenUI.instance.Show(2, Color.white);
        for (int i = 0; i < 4; i++)
        {
            if (dieExplosionFX)
                Instantiate(dieExplosionFX, transform.position + new Vector3(Random.Range(-dieExplosionSize.x, dieExplosionSize.x), Random.Range(0, dieExplosionSize.y), 0), Quaternion.identity);
            SoundManager.PlaySfx(dieExplosionSound);
            CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);
            yield return new WaitForSeconds(0.5f);
        }

        BlackScreenUI.instance.Hide(1);
        GameManager.Instance.GameFinish(1);
        gameObject.SetActive(false);
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
}
