using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpider : MonoBehaviour, ICanTakeDamage
{
    public enum AttackType { Auto, DetectPlayer}
    public AttackType attackType;
    public float speedDown = 5;
    public float speedUp = 2;
    public float waitAtBottom = 1;
    public float waitAtTop = 0.5f;
    public GameObject DestroyEffect;        //spawn object when dead
    public float destroyTime = 1.5f;
    [Header("Health")]
    [Range(0, 100)]
    public float health = 50;
    float currentHealth;
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    protected HealthBarEnemyNew healthBar;
    [Header("Patrol")]
    public Vector2 localUpPoint;
    public Vector2 localDownPoint = new Vector2(0,-3);

    public AudioClip soundAttack, soundHit, soundDie;

    Vector2 upPoint, downPoint;

    public LineRenderer lineRen;
    CheckTargetHelper checkTargetHelper;
    bool isMoving = false;
  
    void OnEnable()
    {
        if (upPoint != Vector2.zero)
            transform.position = upPoint;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = health;
        upPoint = (Vector2) transform.position + localUpPoint;
        downPoint = (Vector2) transform.position + localDownPoint;
        if (attackType == AttackType.Auto)
            StartCoroutine(MovingCo());

        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        
        healthBar.Init(transform, (Vector3)healthBarOffset);

        if (attackType == AttackType.DetectPlayer)
            checkTargetHelper = GetComponent<CheckTargetHelper>();
    }

    IEnumerator MovingCo()
    {
        float percent = 0;
        isMoving = true;
        while (true)
        {
            SoundManager.PlaySfx(soundAttack);
            percent = 0;
            while (percent < 1)
            {
                percent += Time.deltaTime * speedDown;
                transform.position = Vector2.Lerp(upPoint, downPoint, percent);
                yield return null;
            }

            yield return new WaitForSeconds(waitAtBottom);
            percent = 0;
            while (percent < 1)
            {
                percent += Time.deltaTime * speedUp;
                transform.position = Vector2.Lerp(downPoint, upPoint, percent);
                yield return null;
            }


            yield return new WaitForSeconds(waitAtTop);
        }
    }

    private void Update()
    {
        if (!isDead)
        {
            lineRen.positionCount = 2;
            lineRen.SetPosition(0, upPoint);
            lineRen.SetPosition(1, transform.position);
        }
        else
            lineRen.positionCount = 0;

        if (!isMoving && attackType == AttackType.DetectPlayer)
        {
            if (checkTargetHelper.CheckTarget())
                StartCoroutine(MovingCo());
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isMoving = false;
    }

    bool isDead;
    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        //Debug.LogError(damage);
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
            StopAllCoroutines();
            var rig = gameObject.AddComponent<Rigidbody2D>();
            rig.isKinematic = false;
            rig.gravityScale = 2;
            lineRen.positionCount = 0;
            rig.linearVelocity = new Vector2(0, 5);
            Destroy(gameObject, 1.5f);

            var boxCol = gameObject.GetComponent<BoxCollider2D>();
            if (boxCol) boxCol.enabled = false;
            var cirCol = gameObject.GetComponent<CircleCollider2D>();
            if (cirCol) cirCol.enabled = false;

            SoundManager.PlaySfx(soundDie);
            enabled = false;
        }
        else
            SoundManager.PlaySfx(soundHit);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.DrawSphere((Vector2)transform.position + localUpPoint, 0.2f);
            Gizmos.DrawSphere((Vector2)transform.position + localDownPoint, 0.2f);
            Gizmos.DrawLine((Vector2)transform.position + localUpPoint, (Vector2)transform.position + localDownPoint);

            if (attackType == AttackType.DetectPlayer)
            {
                checkTargetHelper = GetComponent<CheckTargetHelper>();
                if (checkTargetHelper == null)
                    checkTargetHelper = gameObject.AddComponent<CheckTargetHelper>();
            }
            else
            {
                if (checkTargetHelper != null)
                    Destroy(checkTargetHelper);
            }
        }
        else
        {
            Gizmos.DrawSphere((Vector2)transform.position + upPoint, 0.2f);
            Gizmos.DrawSphere((Vector2)transform.position + downPoint, 0.2f);
            Gizmos.DrawLine((Vector2)transform.position + upPoint, (Vector2)transform.position + downPoint);
        }
    }
}
