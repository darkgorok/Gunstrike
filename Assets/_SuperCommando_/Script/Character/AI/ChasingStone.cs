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

    private Player CurrentPlayer
    {
        get
        {
            if (player == null && gameSession != null)
                player = gameSession.Player;

            return player;
        }
    }

    public void Init(float yOffset = 0.5f, float xOffset = 0, bool isUseRadar = false)
    {
        if (CurrentPlayer != null)
        {
            Vector3 targetPosition = new Vector3(
                CurrentPlayer.transform.position.x + xOffset,
                CurrentPlayer.transform.position.y + yOffset,
                CurrentPlayer.transform.position.z);
            direction = (targetPosition - transform.position).normalized;
        }

        this.isUseRadar = isUseRadar;
    }

    private void Start()
    {
        if (direction == Vector2.zero && CurrentPlayer != null)
            direction = (CurrentPlayer.transform.position - transform.position).normalized;

        speed = Random.Range(speedMin, speedMax);
    }

    private void Update()
    {
        if (CurrentPlayer == null)
            return;

        if (isChasingPlayer && isUseRadar)
        {
            transform.position = Vector2.MoveTowards(transform.position, CurrentPlayer.transform.position, speed * Time.deltaTime);
            if (Vector2.Distance(CurrentPlayer.transform.position, transform.position) < 0.1f)
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
