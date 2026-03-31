using UnityEngine;

[AddComponentMenu("ADDP/Enemy AI/[ENEMY] Bomber Attack")]
public class EnemyBomberAttack : MonoBehaviour
{
    public LayerMask targetPlayer;
    public Transform checkPoint;
    public float checkRadius = 2f;
    public Miner enemyBomb;
    [Tooltip("Damage depend on the distance of the bomb with the player")]
    public int damageMax = 100;
    public float damageRadius = 5f;
    public float delayBlowUp = 1f;
    public AudioClip soundActiveBomb;

    [HideInInspector] public bool isBlowingUp = false;

    private bool allowCheckTarget;
    private bool isBlewUp;
    private float blowUpTimer = -1f;
    private Enemy ownerEnemy;

    private void Start()
    {
        ownerEnemy = GetComponent<Enemy>();
    }

    private void Update()
    {
        if (blowUpTimer >= 0f)
        {
            blowUpTimer -= Time.deltaTime;
            if (blowUpTimer <= 0f && this && !isBlewUp)
            {
                BlowUp();
                ownerEnemy.TakeDamage(int.MaxValue, Vector2.zero, gameObject, Vector3.zero, BulletFeature.Explosion);
            }
        }

        if (!allowCheckTarget || isBlowingUp)
            return;

        var hit = Physics2D.CircleCast(checkPoint.position, checkRadius, Vector2.zero, 0, targetPlayer);
        if (!hit)
            return;

        allowCheckTarget = false;
        if (!enemyBomb)
        {
            Debug.LogError("MUST PLACE THE EXPLOSION BOMB");
            return;
        }

        BeginBlowUp();
    }

    public void Attack()
    {
        allowCheckTarget = true;
    }

    public void BlowUp()
    {
        if (isBlewUp)
            return;

        isBlewUp = true;
        blowUpTimer = -1f;
        Instantiate(enemyBomb, transform.position, Quaternion.identity).Init(true, damageMax, damageRadius);

        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
            spawnItem.SpawnItem();
    }

    private void BeginBlowUp()
    {
        isBlowingUp = true;
        ownerEnemy.SetEnemyState(ENEMYSTATE.IDLE);
        ownerEnemy.AnimSetTrigger("bomberBlinking");

        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = soundActiveBomb;
        audioSource.Play();

        blowUpTimer = delayBlowUp;
    }

    private void OnDrawGizmos()
    {
        if (checkPoint == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(checkPoint.position, checkRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(checkPoint.position, damageRadius);
    }
}
