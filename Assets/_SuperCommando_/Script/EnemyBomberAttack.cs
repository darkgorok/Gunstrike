using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("ADDP/Enemy AI/[ENEMY] Bomber Attack")]
public class EnemyBomberAttack : MonoBehaviour
{
    public LayerMask targetPlayer;
    public Transform checkPoint;
    public float checkRadius = 2;
    public Miner enemyBomb;
    [Tooltip("Damage depend on the distance of the bomb with the player")]
    public int damageMax = 100;
    public float damageRadius = 5;
    //public AudioClip[] soundAttacks;
    bool allowCheckTarget = false;
    public float delayBlowUp = 1f;
    public AudioClip soundActiveBomb;

   [HideInInspector] public bool isBlowingUp = false;
    bool isBlewUp = false;
    Enemy ownerEnemy;

    private void Start()
    {
        ownerEnemy = GetComponent<Enemy>();
    }

    //called by the owner
    public void Attack()
    {
        allowCheckTarget = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!allowCheckTarget || isBlowingUp)
            return;

        var hit = Physics2D.CircleCast(checkPoint.position, checkRadius, Vector2.zero, 0, targetPlayer);
        if (hit)
        {
            allowCheckTarget = false;
            if (enemyBomb)
            {
                //SpawnSystemHelper.GetNextObject(EnemyBomb, true).transform.position = transform.position;
                StartCoroutine(BlowUpCo());
            }
            else
                Debug.LogError("MUST PLACE THE EXPLOSION BOMB");
        }
    }

    IEnumerator BlowUpCo()
    {
        isBlowingUp = true;
        ownerEnemy.SetEnemyState(ENEMYSTATE.IDLE);
        ownerEnemy.AnimSetTrigger("bomberBlinking");

        var ascr = gameObject.AddComponent<AudioSource>();
        ascr.clip = soundActiveBomb;
        ascr.Play();

        yield return new WaitForSeconds(delayBlowUp);

        if (this && !isBlewUp)
        {
            BlowUp();
            ownerEnemy.TakeDamage(int.MaxValue, Vector2.zero, gameObject, Vector3.zero, BulletFeature.Explosion);     //destroy the owner
        }
    }

    public void BlowUp()
    {
        if (isBlewUp)
            return;
        isBlewUp = true;

        StopAllCoroutines();
        Instantiate(enemyBomb, transform.position, Quaternion.identity).Init(true, damageMax, damageRadius);

        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
        {
            spawnItem.SpawnItem();
        }
    }

    void OnDrawGizmos()
    {
        if (checkPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(checkPoint.position, checkRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(checkPoint.position, damageRadius);
        }
    }
}
