using UnityEngine;
using VContainer;

public class PathedProjectileSpawner : MonoBehaviour
{
    public Transform Destination;
    public PathedProjectile Projectile;
    public GameObject SpawnEffect;
    public AudioClip spawnSound;
    [Range(0, 1)]
    public float spawnSoundVolume = 0.5f;
    public float speed;
    public float fireRate;

    private float nextFireRate;
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
    }

    private void Start()
    {
        nextFireRate = Time.time;
    }

    private void Update()
    {
        if (gameSession == null || gameSession.State != GameManager.GameState.Playing)
            return;

        if (Time.time < nextFireRate + fireRate)
            return;

        nextFireRate = Time.time;
        PathedProjectile projectile = Instantiate(Projectile, transform.position, Quaternion.identity);
        projectile.Initalize(Destination, speed);

        if (SpawnEffect != null)
            Instantiate(SpawnEffect, transform.position, Quaternion.identity);

        audioService?.PlaySfx(spawnSound, spawnSoundVolume);
    }

    public void OnDrawGizmos()
    {
        if (Destination == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, Destination.position);
    }
}
