using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOSS_FLYING_BOMBER : BossManager, ICanTakeDamage, IListener
{
    public float flyingSpeed = 3;
    [Range(1, 1000)]
    public int health = 500;
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

    [HideInInspector]
    public bool isDead = false;

    Animator anim;
    [ReadOnly] public bool moving = false;
    [ReadOnly] public bool isPlaying = false;

    public bool isFacingRight()
    {
        return transform.rotation.y == 0 ? true : false;
    }

    // ACTION
    [Header("*** FLYING BOMBER ***")]
    public float localLeftPosX = -5f;
    public float localRightPosX = 5f;

    public GameObject bombObj;
    public int totalBombInARow = 8;
    public Transform throwPoint;


    Vector2 leftPos, rightPos;
    [ReadOnly] public float distance2bombs;
    float lastThrowPosX;
    bool isThrowing = false;
    int bombCounter = 0;
    int misspoint = 0;  //set the point that the boss no throw the bomb

    [ReadOnly] public Vector2 _direction;

    void Flip()
    {
        _direction = -_direction;
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, isFacingRight() ? 180 : 0, transform.rotation.z));
    }

    IEnumerator BOSS_ACTION_CO()
    {
        if (isThrowing)
            yield break;

        ResetThrowBomb();

        isThrowing = true;
        distance2bombs = Vector2.Distance(leftPos, rightPos)/ (totalBombInARow - 1);

        while (true)
        {
            if (bombCounter != misspoint)
            {
                SoundManager.PlaySfx(attackSound);
                Instantiate(bombObj, throwPoint.position, Quaternion.identity).transform.right = transform.right + Vector3.down * 0.3f;
            }

            bombCounter++;
            lastThrowPosX = transform.position.x;

            while(Mathf.Abs(lastThrowPosX - transform.position.x)< distance2bombs) { yield return null; }
        }
    }

    void ResetThrowBomb()
    {
        bombCounter = 0;
        misspoint = Random.Range(0, totalBombInARow);
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);

        currentHealth = health;

        leftPos = transform.position + Vector3.right * localLeftPosX;
        rightPos = transform.position + Vector3.right * localRightPosX;

        _direction = isFacingRight() ? Vector2.right : Vector2.left;
    }

    public override void Play()
    {
        if (isPlaying)
            return;

        isPlaying = true;
    }

    void Update()
    {
        if (!isPlaying || isDead || GameManager.Instance.State != GameManager.GameState.Playing || GameManager.Instance.Player.isFinish)
        {
            return;
        }

        transform.Translate(flyingSpeed * Time.deltaTime * _direction.x, 0, 0, Space.World);

        if (isFacingRight())
        {
            if (transform.position.x >= rightPos.x)
            {
                Flip();
                ResetThrowBomb();
                StartCoroutine(BOSS_ACTION_CO());
            }
        }
        else
        {
            if (transform.position.x <= leftPos.x)
            {
                Flip();
                ResetThrowBomb();
                StartCoroutine(BOSS_ACTION_CO());
            }
        }
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

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(leftPos, 0.3f);
            Gizmos.DrawWireSphere(rightPos, 0.3f);
            Gizmos.DrawLine(leftPos, rightPos);

            var distance = Vector2.Distance(leftPos, rightPos) / (totalBombInARow - 1);
            for (int i = 0; i < totalBombInARow; i++)
            {
                Gizmos.DrawSphere(transform.position + Vector3.right * localLeftPosX + Vector3.right * i * distance, 0.2f);
            }
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position + Vector3.right * localLeftPosX, 0.3f);
            Gizmos.DrawWireSphere(transform.position + Vector3.right * localRightPosX, 0.3f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * localRightPosX);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * localLeftPosX);

            var distance = Vector2.Distance(transform.position + Vector3.right * localLeftPosX, transform.position + Vector3.right * localRightPosX) / (totalBombInARow - 1);

            for(int i = 0; i < totalBombInARow; i++)
            {
                Gizmos.DrawSphere(transform.position + Vector3.right * localLeftPosX + Vector3.right * i * distance, 0.2f);
            }
        }
    }
}
