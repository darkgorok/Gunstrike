using UnityEngine;
using VContainer;

public class GiveDamageToTarget : MonoBehaviour
{
    public enum DEALDAMAGETOPLAYER { Contact, FallTo }

    public DEALDAMAGETOPLAYER deadDamageToPlayer;

    [Header("GiveDamageToTarget")]
    public GameObject Owner;
    public int Damage = 20;
    public LayerMask targetLayer;
    public Vector2 pushTargetForce = new Vector2(0, 0);

    [Header("Repeat Damage")]
    public bool repeatDamage = true;
    public float damageRate = 0.1f;

    private bool isWorked;
    private float repeatDamageTimer;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IGameSessionService gameSession)
    {
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void OnEnable()
    {
        isWorked = false;
        repeatDamageTimer = 0f;
    }

    private void Update()
    {
        if (!isWorked || !repeatDamage)
            return;

        repeatDamageTimer -= Time.deltaTime;
        if (repeatDamageTimer <= 0f)
            isWorked = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isWorked || Damage == 0)
            return;

        if (targetLayer != (targetLayer | (1 << other.gameObject.layer)))
            return;

        Player player = gameSession?.Player;
        if (deadDamageToPlayer == DEALDAMAGETOPLAYER.FallTo && player != null && player.velocity.y > 0)
            return;

        ICanTakeDamage takeDamage = other.gameObject.GetComponent(typeof(ICanTakeDamage)) as ICanTakeDamage;
        if (takeDamage == null)
            return;

        isWorked = true;
        takeDamage.TakeDamage(Damage, Vector2.zero, Owner == null ? gameObject : Owner, pushTargetForce);

        if (player != null && other.gameObject == player.gameObject)
        {
            float facingDirectionX = Mathf.Sign(player.transform.position.x - transform.position.x);
            float facingDirectionY = Mathf.Sign(player.velocity.y);

            player.SetForce(new Vector2(
                pushTargetForce.x * facingDirectionX,
                pushTargetForce.y * facingDirectionY * -1));
        }

        if (repeatDamage)
            repeatDamageTimer = Mathf.Max(0.01f, damageRate);
    }
}
