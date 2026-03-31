using UnityEngine;
using VContainer;

[AddComponentMenu("ADDP/Enemy AI/[ENEMY] Melee Attack")]
public class EnemyMeleeAttack : MonoBehaviour
{
    public LayerMask targetPlayer;
    public Transform checkPoint;
    public Transform meleePoint;
    public float detectDistance = 1f;
    public float meleeRate = 1f;
    public bool isAttacking { get; set; }
    public GameObject MeleeObj;
    public float meleeAttackZone = 0.7f;
    public float meleeAttackCheckPlayer = 0.1f;
    public int meleeDamage = 20;
    public AudioClip[] soundAttacks;

    private float lastShoot;
    private float attackEndTimer = -1f;
    private IAudioService audioService;

    [Inject]
    public void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Update()
    {
        if (attackEndTimer < 0f)
            return;

        attackEndTimer -= Time.deltaTime;
        if (attackEndTimer <= 0f)
        {
            isAttacking = false;
            attackEndTimer = -1f;
        }
    }

    public bool AllowAction()
    {
        return Time.time - lastShoot > meleeRate;
    }

    public bool CheckPlayer(bool isFacingRight)
    {
        return Physics2D.Raycast(checkPoint.position, isFacingRight ? Vector2.right : Vector2.left, detectDistance, targetPlayer);
    }

    public void Action()
    {
        lastShoot = Time.time;
        isAttacking = true;
    }

    public void Check4Hit()
    {
        var hit = Physics2D.CircleCast(meleePoint.position, meleeAttackZone, Vector2.zero, 0, targetPlayer);
        if (hit)
        {
            var damage = (ICanTakeDamage)hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage));
            damage?.TakeDamage(meleeDamage, Vector2.zero, gameObject, hit.point);
        }

        if (soundAttacks.Length > 0)
            audioService.PlaySfx(soundAttacks[Random.Range(0, soundAttacks.Length)]);
    }

    public void EndCheck4Hit()
    {
        attackEndTimer = 1f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(checkPoint.position, checkPoint.position + Vector3.right * detectDistance);
        Gizmos.DrawSphere(checkPoint.position + Vector3.right * detectDistance, 0.1f);

        if (meleePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(meleePoint.position, meleeAttackZone);
        }
    }
}
