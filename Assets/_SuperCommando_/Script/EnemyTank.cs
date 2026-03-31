using UnityEngine;
using VContainer;

[RequireComponent(typeof(CheckTargetHelper))]
public class EnemyTank : MonoBehaviour, ICanTakeDamage
{
    public int health = 200;
    [ReadOnly] public bool allowMoving = false;
    public float speed = 2;
    public float moveToLocalPointX = -8f;
    public GameObject explosionFX;
    public AudioClip soundHit;
    public AudioClip soundDestroy;

    [Header("---Normal Bullet---")]
    public int normalDamage = 50;
    public Transform normalPoint;
    public Projectile normalBullet;
    public int noralBulletSpeed = 6;
    public float normalGunRate = 2;
    [Range(1, 10)]
    public int normalNumberBulletsRound = 3;
    public float normalBulletRate2Bullets = 0.3f;
    public AudioClip normalSound;

    [Header("---Special Bullet---")]
    public Transform specialGunPoint;
    public GameObject specialBullet;
    public float specialBulletRate = 3;
    public AudioClip specialSound;

    private Vector2 moveToTarget;
    private CheckTargetHelper checkTargetHelper;
    private bool finishMoving;
    private Animator anim;
    private bool isWorking;
    private BurstFireController burstFireController;

    private IAudioService audioService;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IAudioService audioService, IGameSessionService gameSession)
    {
        this.audioService = audioService;
        this.gameSession = gameSession;
    }

    private void Start()
    {
        ProjectScope.Inject(this);
        checkTargetHelper = GetComponent<CheckTargetHelper>();
        moveToTarget = transform.position + Vector3.right * moveToLocalPointX;
        anim = GetComponent<Animator>();
        burstFireController = new BurstFireController(normalNumberBulletsRound, normalGunRate, normalBulletRate2Bullets);
    }

    private void Update()
    {
        burstFireController.Tick(Time.deltaTime, FireNormalShot);

        if (finishMoving)
            return;

        if (!allowMoving)
        {
            if (checkTargetHelper.CheckTarget(transform.position.x > gameSession.Player.transform.position.x ? 1 : -1))
            {
                isWorking = true;
                allowMoving = true;
                gameSession.PauseCamera(true);
            }
        }

        if (allowMoving)
        {
            moveToTarget.y = transform.position.y;
            transform.position = Vector2.MoveTowards(transform.position, moveToTarget, speed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - moveToTarget.x) < 0.1f)
            {
                finishMoving = true;
                allowMoving = false;
            }
        }

        if (finishMoving && burstFireController.CanStartBurst)
            burstFireController.StartBurst(FireNormalShot);

        anim.SetBool("moving", allowMoving);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!isWorking)
            return;

        health -= damage;
        if (health <= 0)
        {
            Instantiate(explosionFX, transform.position, Quaternion.identity);
            gameSession.PauseCamera(false);
            audioService.PlaySfx(soundDestroy);
            Destroy(gameObject);
            return;
        }

        audioService.PlaySfx(soundHit);
    }

    private void FireNormalShot()
    {
        var projectile = SpawnSystemHelper.GetNextObject(normalBullet.gameObject, false).GetComponent<Projectile>();
        Vector2 direction = TargetDirection(Vector3.up * 0.5f);

        projectile.transform.position = normalPoint.position;
        projectile.transform.right = direction;
        projectile.Initialize(gameObject, direction, Vector2.zero, false, false, normalDamage, noralBulletSpeed);
        projectile.gameObject.SetActive(true);

        audioService.PlaySfx(normalSound);
    }

    private Vector2 TargetDirection(Vector3 offset)
    {
        var lookAtPlayerDirection = (gameSession.Player.transform.position + offset) - normalPoint.position;
        lookAtPlayerDirection.Normalize();
        return lookAtPlayerDirection;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + Vector3.right * moveToLocalPointX);
        Gizmos.DrawWireCube(transform.position + Vector3.up + Vector3.right * moveToLocalPointX, Vector3.one * 0.2f);
    }
}
