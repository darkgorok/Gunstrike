using UnityEngine;
using VContainer;

public class FireEvil : MonoBehaviour, ICanTakeDamage
{
    public float speed = 5f;
    public AudioClip showUpSound;
    public AudioClip deadSound;
    public float timeLive = 5f;

    private Vector3 oldPosition;
    private float timeLiveTimer;
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

    private void Start()
    {
        oldPosition = transform.position;
        timeLiveTimer = timeLive;
    }

    private void OnEnable()
    {
        timeLiveTimer = timeLive;
        if (gameSession != null && gameSession.State == GameManager.GameState.Playing)
            audioService.PlaySfx(showUpSound);
    }

    private void Update()
    {
        timeLiveTimer -= Time.deltaTime;
        if (timeLiveTimer <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.Translate(speed * Time.deltaTime * transform.right.x, 0f, 0f, Space.World);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        transform.position = oldPosition;
        audioService.PlaySfx(deadSound);

        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
            spawnItem.SpawnItem();

        gameObject.SetActive(false);
    }
}
