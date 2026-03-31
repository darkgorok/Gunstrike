using UnityEngine;
using VContainer;

public class FallingProjectileBulletBullet : MonoBehaviour, ICanTakeDamage
{
    public LayerMask targetLayer;
    public float speed = 1;
    public GameObject ExplosionFX;
    public AudioClip soundExplosion;
    public int damageToGive = 100;
    public float timeDestroy = 10;

    private Vector2 direction;
    private GameObject Owner;
    private bool allowMoving;
    private IAudioService audioService;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IAudioService audioService, IGameSessionService gameSession)
    {
        this.audioService = audioService;
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    public void Action(Vector2 direction, float speed, GameObject owner = null)
    {
        this.speed = speed;
        this.direction = direction;
        Owner = owner;
        allowMoving = true;
        transform.right = direction;
    }

    private void Start()
    {
        allowMoving = false;
    }

    private void Update()
    {
        if (!allowMoving)
            return;

        transform.Translate(speed * Time.deltaTime, 0, 0, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!allowMoving || targetLayer != (targetLayer | (1 << other.gameObject.layer)))
            return;

        FallingProjectileBulletBullet otherBullet = other.gameObject.GetComponent<FallingProjectileBulletBullet>();
        if (Owner != null && otherBullet != null && otherBullet.Owner == Owner)
            return;

        audioService?.PlaySfx(soundExplosion);

        ICanTakeDamage damage = other.gameObject.GetComponent(typeof(ICanTakeDamage)) as ICanTakeDamage;
        if (damage != null)
        {
            GameObject instigator = gameSession?.Player != null ? gameSession.Player.gameObject : gameObject;
            damage.TakeDamage(damageToGive, Vector2.zero, instigator, Vector2.zero);
        }

        if (ExplosionFX != null)
            Instantiate(ExplosionFX, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!allowMoving)
            return;

        if (ExplosionFX != null)
            Instantiate(ExplosionFX, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
