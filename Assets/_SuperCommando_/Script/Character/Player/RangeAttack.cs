using System.Collections;
using UnityEngine;
using VContainer;

public class RangeAttack : MonoBehaviour
{
    public Transform FirePoint;

    public bool standToFire = true;
    public float standTime = 0.1f;
    [Tooltip("fire projectile after this delay, useful to sync with the animation of firing action")]
    public float fireDelay;
    public float fireRate;
    public bool inverseDirection = false;
    public float bulletSpeed = 10;

    [ReadOnly] public int extraDamage = 0;
    [Header("NORMAL BULLET")]
    [Range(1, 6)]
    public int normalSpeadBullet = 1;
    public Projectile Projectile;
    public int normalDamage = 30;

    [Header("POWER BULLET")]
    [HideInInspector] public bool useTrack = false;
    [Range(1, 12)]
    public int speadBullet = 5;
    public Projectile ProjectilePower;
    public int powerDamage = 30;

    public AudioClip soundAttack;

    private float nextFire = 0f;
    private IAudioService audioService;
    private IGameSessionService gameSession;
    private IInventoryService inventoryService;
    private IUpgradeService upgradeService;
    private IDefaultGameConfigService defaultGameConfigService;

    [Inject]
    public void Construct(IAudioService audioService, IGameSessionService gameSession, IInventoryService inventoryService, IUpgradeService upgradeService, IDefaultGameConfigService defaultGameConfigService)
    {
        this.audioService = audioService;
        this.gameSession = gameSession;
        this.inventoryService = inventoryService;
        this.upgradeService = upgradeService;
        this.defaultGameConfigService = defaultGameConfigService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        extraDamage = (int)upgradeService.GetUpgradePower(UPGRADE_ITEM_TYPE.dart.ToString());
    }

    public bool Fire(bool power)
    {
        if (power)
        {
            if (inventoryService.PowerBullets > 0 && Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;
                inventoryService.PowerBullets--;
                StartCoroutine(DelayAttack(fireDelay, true));
                return true;
            }

            return false;
        }

        bool hasUnlimitedBullets = defaultGameConfigService.DefaultBulletMax;
        bool canFireNormal = hasUnlimitedBullets || inventoryService.Darts > 0 || gameSession.HideGui;
        if (!canFireNormal || Time.time <= nextFire)
            return false;

        nextFire = Time.time + fireRate;
        if (!hasUnlimitedBullets)
            inventoryService.Darts--;

        StartCoroutine(DelayAttack(fireDelay, false));
        return true;
    }

    private IEnumerator DelayAttack(float time, bool powerBullet)
    {
        yield return new WaitForSeconds(time);

        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        if (inverseDirection)
            direction *= -1;

        Vector2 firePoint = FirePoint.position;

        if (powerBullet)
        {
            bool spawnUp = false;
            for (int i = 0; i < speadBullet; i++)
            {
                direction.y += i * 0.1f * (spawnUp ? 1 : -1);
                spawnUp = !spawnUp;

                Projectile projectile = SpawnSystemHelper.GetNextObject(ProjectilePower.gameObject, false).GetComponent<Projectile>();
                projectile.transform.position = firePoint;
                projectile.Initialize(gameObject, direction, Vector2.zero, powerBullet, useTrack, powerDamage + extraDamage, bulletSpeed);
                projectile.gameObject.SetActive(true);
            }
        }
        else
        {
            bool spawnUp = false;
            for (int i = 0; i < normalSpeadBullet; i++)
            {
                direction.y += i * 0.1f * (spawnUp ? 1 : -1);
                spawnUp = !spawnUp;

                Projectile projectile = SpawnSystemHelper.GetNextObject(Projectile.gameObject, false).GetComponent<Projectile>();
                projectile.transform.position = firePoint;
                projectile.Initialize(gameObject, direction, Vector2.zero, powerBullet, false, normalDamage + extraDamage, bulletSpeed);
                projectile.gameObject.SetActive(true);
            }
        }

        audioService.PlaySfx(soundAttack);
    }
}
