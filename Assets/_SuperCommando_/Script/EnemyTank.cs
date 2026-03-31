using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CheckTargetHelper))]

public class EnemyTank : MonoBehaviour, ICanTakeDamage
{
    public int health = 200;
    [ReadOnly] public bool allowMoving = false;
    public float speed = 2;
    public float moveToLocalPointX = -8f;
    Vector2 moveToTarget;
    public GameObject explosionFX;
    public AudioClip soundHit, soundDestroy;

    [Header("---Normal Bullet---")]
    public int normalDamage = 50;
    public Transform normalPoint;
    public Projectile normalBullet;
    public int noralBulletSpeed = 6;
    public float normalGunRate = 2;
    [Range(1,10)]
    public int normalNumberBulletsRound = 3;
    public float normalBulletRate2Bullets = 0.3f;
    float lastTimeFireNormal = -999;
    public AudioClip normalSound;

    [Header("---Special Bullet---")]
    public Transform specialGunPoint;
    public GameObject specialBullet;
    public float specialBulletRate = 3;
    float lastTimeFireSpecial = -999;
    public AudioClip specialSound;
    CheckTargetHelper checkTargetHelper;
    bool finishMoving = false;

    Animator anim;
    bool isWorking = false;

    private void Start()
    {
        checkTargetHelper = GetComponent<CheckTargetHelper>();
        moveToTarget = transform.position + Vector3.right * moveToLocalPointX;
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (finishMoving)
            return;

        if (!allowMoving)
        {
            if (checkTargetHelper.CheckTarget(transform.position.x > GameManager.Instance.Player.transform.position.x ? 1 : -1))
            {
                isWorking = true;
                allowMoving = true;
                GameManager.Instance.PauseCamera(true);
            }
        }

        if (allowMoving)
        {
            moveToTarget.y = transform.position.y;
            transform.position = Vector2.MoveTowards(transform.position, moveToTarget, speed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - moveToTarget.x) < 0.1f)
            {
                finishMoving = true;
                allowMoving = false;
                StartCoroutine(FireCo());
            }
        }

        anim.SetBool("moving", allowMoving);
    }

    IEnumerator FireCo()
    {
        while (true)
        {
            for (int i = 0; i < normalNumberBulletsRound; i++)
            {
                var projectile = SpawnSystemHelper.GetNextObject(normalBullet.gameObject, false).GetComponent<Projectile>();
                projectile.transform.position = normalPoint.position;
                projectile.transform.right = targetDirection(Vector3.up * 0.5f);
                //projectile.transform.rotation = Quaternion.Euler (0, 0, shootAngle);
                projectile.Initialize(gameObject, targetDirection(Vector3.up * 0.5f), Vector2.zero, false, false, normalDamage, noralBulletSpeed);

                projectile.gameObject.SetActive(true);
                SoundManager.PlaySfx(normalSound);
                yield return new WaitForSeconds(normalBulletRate2Bullets);
            }

            yield return new WaitForSeconds(normalGunRate);
        }
    }

    Vector2 targetDirection(Vector3 offset)
    {
            var lookAtPlayerDirection = (GameManager.Instance.Player.transform.position + offset) - normalPoint.position;

            lookAtPlayerDirection.Normalize();
            return lookAtPlayerDirection;
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!isWorking)
            return;

        health -= damage;
        if (health <= 0)
        {
            Instantiate(explosionFX, transform.position, Quaternion.identity);
            GameManager.Instance.PauseCamera(false);
            SoundManager.PlaySfx(soundDestroy);
            Destroy(gameObject);
        }else
            SoundManager.PlaySfx(soundHit);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + Vector3.right * moveToLocalPointX);
        Gizmos.DrawWireCube(transform.position + Vector3.up + Vector3.right * moveToLocalPointX, Vector3.one * 0.2f);
    }
}
