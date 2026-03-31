using UnityEngine;
using VContainer;

[RequireComponent(typeof(CheckTargetHelper))]
public class Turret : MonoBehaviour, ICanTakeDamage
{
    public bool fixedCamera = false;
    public bool aimPlayer = false;
    public int health = 200;
    public Transform gunObj;
    public GameObject explosionFX;

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

    private CheckTargetHelper checkTargetHelper;
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
        anim = GetComponent<Animator>();
        burstFireController = new BurstFireController(normalNumberBulletsRound, normalGunRate, normalBulletRate2Bullets);
    }

    private void Update()
    {
        burstFireController.Tick(Time.deltaTime, FireNormalShot);

        if (isWorking && aimPlayer)
        {
            gunObj.transform.right = TargetDirection(Vector3.up * 0.5f);
            return;
        }

        if (isWorking)
        {
            if (burstFireController.CanStartBurst)
                burstFireController.StartBurst(FireNormalShot);
            return;
        }

        if (!checkTargetHelper.CheckTarget(transform.position.x > gameSession.Player.transform.position.x ? 1 : -1))
            return;

        isWorking = true;
        if (fixedCamera)
            gameSession.PauseCamera(true);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!isWorking)
            return;

        health -= damage;
        if (health > 0)
            return;

        burstFireController.CancelBurst();
        if (explosionFX)
            Instantiate(explosionFX, transform.position, Quaternion.identity);

        if (fixedCamera)
            gameSession.PauseCamera(false);

        GetComponent<Collider2D>().enabled = false;
        anim.SetBool("destroyed", true);
        enabled = false;
    }

    private void FireNormalShot()
    {
        var projectile = SpawnSystemHelper.GetNextObject(normalBullet.gameObject, false).GetComponent<Projectile>();

        Vector3 direction = aimPlayer ? TargetDirection(Vector3.up * 0.5f) : normalPoint.right;
        if (aimPlayer)
            gunObj.transform.right = direction;

        projectile.transform.position = normalPoint.position;
        projectile.transform.right = direction;
        projectile.Initialize(gameObject, direction, Vector2.zero, false, false, normalDamage, noralBulletSpeed);
        projectile.gameObject.SetActive(true);

        audioService.PlaySfx(normalSound);
        anim.SetTrigger("shot");
    }

    private Vector2 TargetDirection(Vector3 offset)
    {
        var lookAtPlayerDirection = (gameSession.Player.transform.position + offset) - transform.position;
        lookAtPlayerDirection.Normalize();
        return lookAtPlayerDirection;
    }
}
