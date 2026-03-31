using UnityEngine;
using VContainer;

public class PathedProjectile : MonoBehaviour, ICanTakeDamage
{
    public bool canBeKill;
    public GameObject DestroyEffect;
    public int pointToGivePlayer;
    public AudioClip soundDestroy;
    [Range(0, 1)] public float soundDestroyVolume = 0.5f;

    private Transform destination;
    private float speed;
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

    public void Initalize(Transform destination, float speed)
    {
        this.destination = destination;
        this.speed = speed;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, destination.position, Time.deltaTime * speed);
        float distance = (destination.position - transform.position).sqrMagnitude;
        if (distance > 0.01f)
            return;

        if (DestroyEffect != null)
            Instantiate(DestroyEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    void ICanTakeDamage.TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!canBeKill)
            return;

        if (DestroyEffect != null)
            Instantiate(DestroyEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
        audioService.PlaySfx(soundDestroy, soundDestroyVolume);

        var projectile = instigator.GetComponent<Projectile>();
        if (projectile != null && projectile.Owner.GetComponent<Player>() != null && pointToGivePlayer != 0)
            gameSession.AddPoint(pointToGivePlayer);
    }
}
