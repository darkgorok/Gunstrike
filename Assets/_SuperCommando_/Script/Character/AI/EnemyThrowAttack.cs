using UnityEngine;
using VContainer;

[AddComponentMenu("ADDP/Enemy AI/Throw Attack")]
public class EnemyThrowAttack : MonoBehaviour
{
    public enum ThrowAction { WaitPlayerInRange, ThrowAuto }

    public ThrowAction throwAction;

    [Header("Grenade")]
    public float angleThrow = 60f;
    public float throwForce = 300f;
    public float addTorque = 100f;
    public float throwRate = 0.5f;
    public Transform throwPosition;
    public Grenade _Grenade;
    public int makeDamage = 100;
    public float radius = 3f;
    public AudioClip soundAttack;
    public GameObject fireFX;

    public LayerMask targetPlayer;
    public Transform checkPoint;
    public float radiusDetectPlayer = 5f;
    public bool isAttacking { get; set; }

    private float lastShoot;
    private IGameSessionService gameSession;
    private IAudioService audioService;

    [Inject]
    public void Construct(IGameSessionService gameSession, IAudioService audioService)
    {
        this.gameSession = gameSession;
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    public bool AllowAction()
    {
        return Time.time - lastShoot > throwRate;
    }

    public void Throw(bool isFacingRight, Vector2 throwDirection)
    {
        Vector3 throwPos = throwPosition.position;
        var obj = Instantiate(_Grenade, throwPos, Quaternion.identity);
        obj.Init(makeDamage, radius, false, false, gameSession.Player.transform.position.y + 2f);

        if (throwDirection == Vector2.zero)
        {
            float angle = isFacingRight ? angleThrow : 135f;
            obj.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            obj.transform.right = throwDirection;
        }

        var rigidbody = obj.GetComponent<Rigidbody2D>();
        rigidbody.AddRelativeForce(obj.transform.right * throwForce);
        rigidbody.AddTorque(obj.transform.right.x * addTorque);

        if (fireFX)
            SpawnSystemHelper.GetNextObject(fireFX, true).transform.position = throwPos;

        audioService.PlaySfx(soundAttack);
    }

    public bool CheckPlayer()
    {
        if (throwAction == ThrowAction.ThrowAuto)
            return true;

        return Physics2D.CircleCast(checkPoint.position, radiusDetectPlayer, Vector2.zero, 0, targetPlayer);
    }

    public void Action()
    {
        if (_Grenade == null)
            return;

        lastShoot = Time.time;
        isAttacking = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (throwAction != ThrowAction.WaitPlayerInRange)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(checkPoint.position, radiusDetectPlayer);
    }
}
