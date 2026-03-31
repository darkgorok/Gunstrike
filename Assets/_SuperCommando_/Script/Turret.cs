using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CheckTargetHelper))]

public class Turret : MonoBehaviour, ICanTakeDamage
{
    public bool fixedCamera = false;
    public bool aimPlayer = false;
    public int health = 200;
    public Transform gunObj;
    public GameObject explosionFX;

    [Header("---Normal Bullet---")]
    public int normalDamage = 50;
    public Transform normalPoint;
    public Projectile normalBullet;
    public int noralBulletSpeed = 6;
    public float normalGunRate = 2;
    [Range(1, 10)]
    public int normalNumberBulletsRound = 3;
    public float normalBulletRate2Bullets = 0.3f;
    float lastTimeFireNormal = -999;
    public AudioClip normalSound;

    CheckTargetHelper checkTargetHelper;
    Animator anim;
    bool isWorking = false;

    private void Start()
    {
        checkTargetHelper = GetComponent<CheckTargetHelper>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {

        if (isWorking && aimPlayer)
        {
            var direction = targetDirection(Vector3.up * 0.5f);
            gunObj.transform.right = direction;
            return;
        }
        else if (isWorking)
            return;

        if (checkTargetHelper.CheckTarget(transform.position.x > GameManager.Instance.Player.transform.position.x ? 1 : -1))
        {
            isWorking = true;
            StartCoroutine(FireCo());
            if (fixedCamera)
                GameManager.Instance.PauseCamera(true);
        }
    }

    IEnumerator FireCo()
    {
        while (true)
        {
            for (int i = 0; i < normalNumberBulletsRound; i++)
            {
                var projectile = SpawnSystemHelper.GetNextObject(normalBullet.gameObject, false).GetComponent<Projectile>();

                Vector3 direction;
                if (aimPlayer)
                {
                    direction = targetDirection(Vector3.up * 0.5f);
                    gunObj.transform.right = direction;
                    yield return null;
                }
                else
                    direction = normalPoint.right;

                projectile.transform.position = normalPoint.position;
                projectile.transform.right = direction;
                //projectile.transform.rotation = Quaternion.Euler (0, 0, shootAngle);
                projectile.Initialize(gameObject, direction, Vector2.zero, false, false, normalDamage, noralBulletSpeed);

                projectile.gameObject.SetActive(true);
                SoundManager.PlaySfx(normalSound);
                anim.SetTrigger("shot");
                yield return new WaitForSeconds(normalBulletRate2Bullets);
            }

            yield return new WaitForSeconds(normalGunRate);
        }
    }

    Vector2 targetDirection(Vector3 offset)
    {
        var lookAtPlayerDirection = (GameManager.Instance.Player.transform.position + offset) - transform.position;

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
            StopAllCoroutines();
            if (explosionFX)
                Instantiate(explosionFX, transform.position, Quaternion.identity);

            if (fixedCamera)
                GameManager.Instance.PauseCamera(false);

            GetComponent<Collider2D>().enabled = false;
            anim.SetBool("destroyed", true);
            enabled = false;
        }
    }
}
