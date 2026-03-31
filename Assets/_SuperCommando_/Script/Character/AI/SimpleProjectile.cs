using UnityEngine;
using VContainer;

public class SimpleProjectile : Projectile, ICanTakeDamage
{
    [Header("Bullet")]
    public GameObject DestroyEffect;
    public int pointToGivePlayer;
    public float timeToLive = 2f;
    public GameObject newBulletEffect;

    public AudioClip soundHitEnemy;
    [Range(0, 1)] public float soundHitEnemyVolume = 0.5f;
    public AudioClip soundHitNothing;
    [Range(0, 1)] public float soundHitNothingVolume = 0.5f;

    private float timeLiveCounter;
    private bool isDestroy;
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

    private void OnDisable()
    {
        ResetProjectileState();
    }

    private void OnEnable()
    {
        ResetProjectileState();
    }

    public override void Update()
    {
        if ((timeLiveCounter -= Time.deltaTime) <= 0f)
        {
            DestroyProjectile();
            return;
        }

        if (!isDetect && isUseRadar)
        {
            RaycastHit2D hit = Physics2D.CircleCast((Vector2)transform.position + Direction * radarRadius, radarRadius, Vector2.zero, 0, LayerCollision);
            if (hit && hit.collider.gameObject.layer != LayerMask.NameToLayer("Platform"))
            {
                var anotherSimpleProjectile = hit.collider.gameObject.GetComponent<SimpleProjectile>();
                if (anotherSimpleProjectile == null || Owner != anotherSimpleProjectile.Owner)
                {
                    isDetect = true;
                    target = hit.collider.gameObject.transform;
                }
            }

            transform.Translate((Direction + new Vector2(InitialVelocity.x, 0f)) * Speed * Time.deltaTime, Space.World);
        }
        else if (target)
        {
            var aimPos = target.GetComponent<Collider2D>().bounds.center;
            transform.position = Vector2.MoveTowards(transform.position, aimPos, Speed * Time.deltaTime);

            Vector3 dir = aimPos - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            angle = Mathf.Lerp(transform.eulerAngles.z > 180 ? transform.eulerAngles.z - 360 : transform.eulerAngles.z, angle, 1f);

            float finalAngle = angle < 0 ? angle - 360 : angle;
            transform.rotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);
        }
        else
        {
            if (isDetect)
            {
                if (DestroyEffect != null)
                    Instantiate(DestroyEffect, transform.position, Quaternion.identity);

                gameObject.SetActive(false);
            }

            transform.Translate(Speed * Time.deltaTime, 0f, 0f, Space.Self);
        }

        base.Update();
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (pointToGivePlayer != 0)
        {
            var projectile = instigator.GetComponent<Projectile>();
            if (projectile != null && projectile.Owner != null && projectile.Owner.GetComponent<Player>() != null)
                gameSession.AddPoint(pointToGivePlayer);
        }

        audioService.PlaySfx(soundHitNothing, soundHitNothingVolume);
        DestroyProjectile();
    }

    protected override void OnCollideOther(RaycastHit2D other)
    {
        audioService.PlaySfx(soundHitNothing, soundHitNothingVolume);
        DestroyProjectile();
    }

    protected override void OnCollideTakeDamage(RaycastHit2D other, ICanTakeDamage takeDamage)
    {
        if (isDestroy)
            return;

        takeDamage.TakeDamage(Damage, Vector2.zero, Owner, other.point);
        if (gameSession.Player != null && Owner == gameSession.Player.gameObject)
            CameraPlay.EarthQuakeShake(0.1f, 60, 1.5f);

        audioService.PlaySfx(soundHitEnemy, soundHitEnemyVolume);
        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        if (isDestroy)
            return;

        isDestroy = true;
        if (DestroyEffect != null)
            SpawnSystemHelper.GetNextObject(DestroyEffect, true).transform.position = transform.position;

        if (isPower && newBulletEffect != null)
        {
            var bullet = Instantiate(newBulletEffect, transform.position, Quaternion.identity);
            bullet.GetComponent<Grenade>().DoExplosion();
        }

        gameObject.SetActive(false);
    }

    private void ResetProjectileState()
    {
        isDestroy = false;
        isDetect = false;
        target = null;
        isUseRadar = false;
        timeLiveCounter = timeToLive;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.right * radarRadius, radarRadius);
    }
}
