using UnityEngine;
using VContainer;

public class ArrowProjectile : Projectile, IListener
{
    public bool parentToHitObject = true;
    public bool destroyWhenContact = false;
    public GameObject DestroyEffect;
    public int pointToGivePlayer;
    public float timeToLive = 3f;
    public AudioClip soundHitEnemy;
    [Range(0, 1)] public float soundHitEnemyVolume = 0.5f;
    public AudioClip soundHitNothing;
    [Range(0, 1)] public float soundHitNothingVolume = 0.5f;

    [Header("EXPLOSTION")]
    public bool doExplosionDamage = false;
    public Grenade _grenade;
    public int explosionDamage = 100;
    public float explosionRadius = 1f;

    private Rigidbody2D rig;
    private bool isStop;
    private bool destroyScheduled;
    private float destroyDelay;

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

    private void OnEnable()
    {
        if (rig == null)
            rig = GetComponent<Rigidbody2D>();

        destroyScheduled = false;
        destroyDelay = -1f;
        rig.isKinematic = false;
    }

    public void Init(Vector2 velocityForce, float gravityScale)
    {
        if (rig == null)
            rig = GetComponent<Rigidbody2D>();

        rig.gravityScale = gravityScale;
        rig.linearVelocity = velocityForce;
    }

    public override void Update()
    {
        base.Update();

        if (isStop || !destroyScheduled)
            return;

        if (!destroyWhenContact)
        {
            destroyDelay -= Time.deltaTime;
            if (destroyDelay > 0f)
                return;
        }

        FinalizeDestroy();
    }

    public void TakeDamage(float damage, Vector2 force, Vector2 hitPoint, GameObject instigator, Vector3 point)
    {
        audioService.PlaySfx(soundHitNothing, soundHitNothingVolume);
        ScheduleDestroy(1f);
    }

    public void TakeDamage(float damage, Vector2 force, Vector2 hitPoint, GameObject instigator, Vector2 point)
    {
        ScheduleDestroy(0f);
    }

    protected override void OnCollideOther(RaycastHit2D other)
    {
        audioService.PlaySfx(soundHitNothing, soundHitNothingVolume);
        ScheduleDestroy(3f);
        if (parentToHitObject)
            transform.parent = other.collider.gameObject.transform;
    }

    protected override void OnCollideTakeDamage(RaycastHit2D other, ICanTakeDamage takeDamage)
    {
        base.OnCollideTakeDamage(other, takeDamage);
        takeDamage.TakeDamage(int.MaxValue, Vector2.zero, Owner, other.point);
        audioService.PlaySfx(soundHitEnemy, soundHitEnemyVolume);
        ScheduleDestroy(0f);
    }

    private void ScheduleDestroy(float delay)
    {
        if (destroyScheduled)
            return;

        destroyScheduled = true;
        destroyDelay = delay;
        if (rig == null)
            rig = GetComponent<Rigidbody2D>();

        rig.linearVelocity = Vector2.zero;
        rig.isKinematic = true;
    }

    private void FinalizeDestroy()
    {
        destroyScheduled = false;
        destroyDelay = -1f;

        if (DestroyEffect != null)
            SpawnSystemHelper.GetNextObject(DestroyEffect, true).transform.position = transform.position;

        if (_grenade != null)
        {
            var grenade = Instantiate(_grenade, transform.position, Quaternion.identity);
            grenade.Init(explosionDamage, explosionRadius, true);
        }

        gameObject.SetActive(false);
    }

    public void IPlay()
    {
    }

    public void ISuccess()
    {
    }

    public void IPause()
    {
    }

    public void IUnPause()
    {
    }

    public void IGameOver()
    {
    }

    public void IOnRespawn()
    {
    }

    public void IOnStopMovingOn()
    {
        isStop = true;
    }

    public void IOnStopMovingOff()
    {
        isStop = false;
    }
}
