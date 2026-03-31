using UnityEngine;
using VContainer;

[AddComponentMenu("ADDP/Enemy AI/Enemy Range Attack")]
public class EnemyRangeAttack : MonoBehaviour
{
    public bool allowAimPlayer = true;
    public Transform firePoint;
    public float bulletSpeed = 10;
    public int damage = 30;
    public Projectile bullet;
    public GameObject muzzleFX;
    public float shootingRate = 2;
    public int multiShoot = 1;
    public float multiShootRate = 0.2f;
    public AudioClip soundAttack;

    private BurstFireController burstFireController;
    private Vector2 queuedDirection;
    private IAudioService audioService;

    [Inject]
    public void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        burstFireController = new BurstFireController(multiShoot, shootingRate, multiShootRate);
    }

    private void Update()
    {
        burstFireController.Tick(Time.deltaTime, FireQueuedShot);
    }

    public bool AllowAction()
    {
        return burstFireController.CanStartBurst;
    }

    public void Shoot(Vector2 bulletDirection)
    {
        queuedDirection = bulletDirection;
        burstFireController.StartBurst(FireQueuedShot);
    }

    private void FireQueuedShot()
    {
        var projectile = SpawnSystemHelper.GetNextObject(bullet.gameObject, false).GetComponent<Projectile>();
        projectile.transform.position = firePoint.position;
        projectile.transform.right = queuedDirection;
        projectile.Initialize(gameObject, queuedDirection, Vector2.zero, false, false, damage, bulletSpeed);

        projectile.gameObject.SetActive(true);
        audioService.PlaySfx(soundAttack);

        if (muzzleFX)
        {
            var muzzle = SpawnSystemHelper.GetNextObject(muzzleFX, firePoint.position, true);
            muzzle.transform.right = queuedDirection;
        }
    }
}
