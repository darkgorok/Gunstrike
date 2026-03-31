using UnityEngine;
using VContainer;

public class Boss_SuperAttackFlame : MonoBehaviour
{
    public LayerMask layerGround;
    public float timeOn = 1f;
    public float timeOff = 1.5f;
    public int damage = 20;
    public ParticleSystem beginParticSys;
    public EffectType effectChoose;
    public ParticleSystem[] FX1;
    public ParticleSystem[] FX2;
    public ParticleSystem[] FX3;
    public AudioClip sound;

    private ParticleSystem[] particleSystems;
    private bool hitPlayer;
    private BoxCollider2D box2D;
    private float turnOnTimer = -1f;
    private float turnOffTimer = -1f;
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
        switch (effectChoose)
        {
            case EffectType.Effect1:
                particleSystems = FX1;
                break;
            case EffectType.Effect2:
                particleSystems = FX2;
                break;
            case EffectType.Effect3:
                particleSystems = FX3;
                break;
        }

        SetParticleArrayActive(FX1, false);
        SetParticleArrayActive(FX2, false);
        SetParticleArrayActive(FX3, false);
        SetParticleArrayActive(particleSystems, false);

        box2D = GetComponent<BoxCollider2D>();
        box2D.enabled = false;

        if (gameSession?.Player == null)
            return;

        RaycastHit2D hit = Physics2D.Raycast(gameSession.Player.transform.position + Vector3.up, Vector2.down, 10, layerGround);
        if (hit)
        {
            transform.position = hit.point;
            turnOnTimer = timeOn;
        }
    }

    private void Update()
    {
        if (turnOnTimer >= 0f)
        {
            turnOnTimer -= Time.deltaTime;
            if (turnOnTimer <= 0f)
            {
                turnOnTimer = -1f;
                TurnOn();
            }
        }

        if (turnOffTimer >= 0f)
        {
            turnOffTimer -= Time.deltaTime;
            if (turnOffTimer <= 0f)
            {
                turnOffTimer = -1f;
                TurnOff();
            }
        }
    }

    public void TurnOn()
    {
        box2D.enabled = true;
        foreach (ParticleSystem child in particleSystems)
        {
            child.gameObject.SetActive(true);
            var emission = child.emission;
            emission.enabled = true;
        }

        audioService?.PlaySfx(sound);
        turnOffTimer = timeOn;
    }

    public void TurnOff()
    {
        foreach (ParticleSystem child in particleSystems)
        {
            var emission = child.emission;
            emission.enabled = false;
        }

        var beginEmission = beginParticSys.emission;
        beginEmission.enabled = false;
        box2D.enabled = false;
        beginParticSys.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hitPlayer || gameSession?.Player == null)
            return;

        if (gameSession.Player.gameObject.layer == LayerMask.NameToLayer("HidingZone"))
            return;

        Player player = other.GetComponent<Player>();
        if (player == null)
            return;

        player.TakeDamage(damage, Vector2.zero, gameObject, Vector2.zero);
        hitPlayer = true;
        box2D.enabled = false;
    }

    private static void SetParticleArrayActive(ParticleSystem[] systems, bool active)
    {
        if (systems == null)
            return;

        foreach (ParticleSystem child in systems)
            child.gameObject.SetActive(active);
    }
}
