using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CheckTargetHelper))]

public class EnemyHideAndShow : MonoBehaviour, ICanTakeDamage
{
    [Range(0, 1000)]
    public int health = 100;
    int currentHealth;
    [Space]
    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    public GameObject hitFX;
    protected EnemyThrowAttack throwAttack;
    [ReadOnlyAttribute] public CheckTargetHelper checkTarget;
    Animator anim;
    bool isDead;
    protected HealthBarEnemyNew healthBar;
    bool isDetectedPlayer = false;

    public bool isFacingRight()
    {
        return transform.rotation.y == 0 ? true : false;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = health;
           throwAttack = GetComponent<EnemyThrowAttack>();
        checkTarget = GetComponent<CheckTargetHelper>();
        anim = GetComponent<Animator>();

        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead || isDetectedPlayer)
            return;

        if (checkTarget.CheckTarget(isFacingRight() ? 1 : -1))
        {
            isDetectedPlayer = true;
            StartCoroutine(ThrowCo());
        }
    }

    IEnumerator ThrowCo()
    {
        anim.SetTrigger("show");
        yield return new WaitForSeconds(1);
        while (true)
        {
            CheckTargetAndFlip();
            if (throwAttack.AllowAction())
            {
                if (throwAttack.CheckPlayer())
                {
                    throwAttack.Action();
                    anim.SetTrigger("throw");
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    void CheckTargetAndFlip()
    {
        if (Mathf.Abs(transform.position.x - GameManager.Instance.Player.transform.position.x) > 0.1f && ((isFacingRight() && transform.position.x > GameManager.Instance.Player.transform.position.x) || (!isFacingRight() && transform.position.x < GameManager.Instance.Player.transform.position.x)))
        {
            Flip();
        }
    }

    void Flip()
    {
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, isFacingRight() ? 180 : 0, transform.rotation.z));
    }

    public void AnimThrow()
    {
        CheckTargetDirection();

        throwAttack.Throw(isFacingRight(), lookAtPlayerDirection);
    }

    Vector2 lookAtPlayerDirection;
    void CheckTargetDirection()
    {
        var hitPlayer = Physics2D.CircleCast(transform.position, 8, Vector2.zero, 0, 1 << (LayerMask.NameToLayer("Player")));
        if (hitPlayer)
        {

            lookAtPlayerDirection = (hitPlayer.transform.position - transform.position);

            lookAtPlayerDirection.Normalize();
        }
    }

    public virtual void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (isDead)
            return;

        Vector2 hitPos = hitPoint;
        currentHealth -= (int)damage;
        if (hitFX)
            SpawnSystemHelper.GetNextObject(hitFX, true).transform.position =
                hitPos + new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));

        if (healthBar)
            healthBar.UpdateValue(currentHealth / (float)health);

        if (currentHealth <= 0)
        {
            StopAllCoroutines();
            anim.SetTrigger("die");
            isDead = true;
            Destroy(gameObject, 3);
        }
        else
            anim.SetTrigger("hurt");
    }
}
