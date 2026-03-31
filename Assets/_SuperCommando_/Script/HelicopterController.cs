using UnityEngine;
using VContainer;

[RequireComponent(typeof(CheckTargetHelper))]
public class HelicopterController : MonoBehaviour, ICanTakeDamage
{
    public int health = 200;
    public Grenade bomb;
    public float dropRate = 1.5f;
    public AudioClip soundDestroy;
    public float speed = 2;
    public GameObject explosionFX;

    [ReadOnly] public bool allowMoving = false;

    private float lastDropTime = -999f;
    private CheckTargetHelper checkTargetHelper;
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
        checkTargetHelper = GetComponent<CheckTargetHelper>();
    }

    private void Update()
    {
        Player player = gameSession?.Player;
        if (player == null)
            return;

        if (!allowMoving)
        {
            if (checkTargetHelper.CheckTarget(transform.position.x > player.transform.position.x ? 1 : -1))
            {
                allowMoving = true;
                gameSession.PauseCamera(true);
            }
        }

        if (!allowMoving)
            return;

        if (Mathf.Abs(transform.position.x - player.transform.position.x) > 0.1f)
        {
            transform.Translate(speed * Time.deltaTime * (transform.position.x > player.transform.position.x ? -1 : 1), 0, 0);
        }

        if (Time.time <= lastDropTime + dropRate)
            return;

        lastDropTime = Time.time;
        Grenade obj = Instantiate(bomb, transform.position, Quaternion.identity);
        obj.Init(100, 0.75f, false, true);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        health -= damage;
        if (health > 0)
            return;

        Instantiate(explosionFX, transform.position, Quaternion.identity);
        gameSession?.PauseCamera(false);
        audioService?.PlaySfx(soundDestroy);
        Destroy(gameObject);
    }
}
