using UnityEngine;
using VContainer;

public class ChasingStone : MonoBehaviour, ICanTakeDamage
{
    public float speedMin = 2f;
    public float speedMax = 3f;
    public Vector2 direction = Vector2.zero;
    public GameObject ExplosionFx;
    public AudioClip destroySound;

    [Header("RADAR")]
    public float radarRadius = 3;

    private float speed;
    private bool isUseRadar;
    private bool isChasingPlayer = true;
    private Player player;
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

    public void Init(float yOffset = 0.5f, float xOffset = 0, bool isUseRadar = false)
    {
        if (gameSession?.Player != null)
        {
            Vector3 targetPosition = new Vector3(
                gameSession.Player.transform.position.x + xOffset,
                gameSession.Player.transform.position.y + yOffset,
                gameSession.Player.transform.position.z);
            direction = (targetPosition - transform.position).normalized;
        }

        this.isUseRadar = isUseRadar;
    }

    private void Start()
    {
        player = gameSession != null ? gameSession.Player : FindObjectOfType<Player>();
        if (direction == Vector2.zero && player != null)
            direction = (player.transform.position - transform.position).normalized;

        speed = Random.Range(speedMin, speedMax);
    }

    private void Update()
    {
        if (player == null)
            return;

        if (isChasingPlayer && isUseRadar)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
            if (Vector2.Distance(player.transform.position, transform.position) < 0.1f)
                isChasingPlayer = false;

            return;
        }

        transform.Translate(speed * direction * Time.deltaTime);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (ExplosionFx)
            Instantiate(ExplosionFx, transform.position, Quaternion.identity);

        audioService?.PlaySfx(destroySound, 0.7f);
        gameObject.SetActive(false);
    }
}
