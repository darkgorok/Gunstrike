using UnityEngine;
using VContainer;

public class TornadoBullet : MonoBehaviour
{
    public bool TA_twoDirection = true;
    public int damagePerBullet = 50;
    public Projectile projectile;
    public float bulletSpeed = 5;
    public AudioClip sound;

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

    public void Init(bool taTwoDirection, int damagePerBullet, float bulletSpeed)
    {
        TA_twoDirection = taTwoDirection;
        this.damagePerBullet = damagePerBullet;
        this.bulletSpeed = bulletSpeed;
    }

    private void OnEnable()
    {
        audioService?.PlaySfx(sound);
        for (int i = 0; i < (TA_twoDirection ? 2 : 1); i++)
        {
            float angle = 180 * i;
            GameObject nextProjectile = SpawnSystemHelper.GetNextObject(projectile.gameObject, false);
            nextProjectile.transform.position = transform.position;
            nextProjectile.GetComponent<Projectile>().Initialize(gameObject, UltiHelper.AngleToVector2(angle), Vector2.zero, false, false, damagePerBullet, bulletSpeed);
            nextProjectile.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}
