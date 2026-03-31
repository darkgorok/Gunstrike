using UnityEngine;
using VContainer;

public class Grenade : MonoBehaviour
{
    public bool onlyBlowWhenContactCollision;
    public float delayBlowUp = 0.7f;

    [Header("Explosion Damage")]
    public AudioClip soundDestroy;
    public GameObject[] DestroyFX;
    public LayerMask collisionLayer;
    public int makeDamage = 100;
    public float radius = 3;

    [ReadOnly] public float collideWithTheGroundUnderPosY = 1000;

    private bool isBlowingUp;
    private float blowUpDelayRemaining = -1f;
    private Rigidbody2D rig;
    private Animator anim;
    private Collider2D cachedCollider;
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
        rig = GetComponent<Rigidbody2D>();
        cachedCollider = GetComponent<Collider2D>();
    }

    public void Init(int damage, float radius, bool blowImmediately = false, bool blowOnContactCollision = false, float collideWithTheGroundUnderPosY = -1)
    {
        makeDamage = damage;
        this.radius = radius;
        onlyBlowWhenContactCollision = blowOnContactCollision;
        GetComponent<Collider2D>().enabled = false;
        this.collideWithTheGroundUnderPosY = collideWithTheGroundUnderPosY != -1 ? collideWithTheGroundUnderPosY : 1000;

        if (blowImmediately)
            DoExplosion();
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!cachedCollider.enabled && transform.position.y < collideWithTheGroundUnderPosY)
            cachedCollider.enabled = true;

        if (blowUpDelayRemaining < 0f)
            return;

        blowUpDelayRemaining -= Time.deltaTime;
        if (blowUpDelayRemaining <= 0f)
        {
            blowUpDelayRemaining = -1f;
            DoExplosion();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isBlowingUp)
            return;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 0.2f, Vector2.zero, 0, gameSession.GroundLayer);
        if (hits == null)
            return;

        isBlowingUp = true;
        blowUpDelayRemaining = onlyBlowWhenContactCollision ? 0f : delayBlowUp;
    }

    public void DoExplosion()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero, 0, collisionLayer);
        if (hits == null)
            return;

        foreach (RaycastHit2D hit in hits)
        {
            ICanTakeDamage damage = hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage)) as ICanTakeDamage;
            if (damage != null)
                damage.TakeDamage(makeDamage, Vector2.zero, gameObject, Vector2.zero);
        }

        foreach (GameObject fx in DestroyFX)
        {
            if (!fx)
                continue;

            RaycastHit2D hitGround = Physics2D.Raycast(transform.position, Vector2.down, 100, gameSession.GroundLayer);
            SpawnSystemHelper.GetNextObject(fx, true).transform.position = hitGround ? (Vector3)hitGround.point : transform.position;
        }

        audioService?.PlaySfx(soundDestroy);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
