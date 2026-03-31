using UnityEngine;
using VContainer;

public class MonsterFish : MonoBehaviour, ICanTakeDamage
{
    public Vector2 Attackdirection;
    public float AttackForce = 750f;
    public float delayAttack = 0.35f;
    public AudioClip soundAttack;
    public AudioClip soundDead;
    public GameObject deadFx;
    public int scoreRewarded = 200;

    private Rigidbody2D rig;
    private float rotation;
    private float attackTimer = -1f;
    private bool attackPending;

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
        rig = GetComponent<Rigidbody2D>();
        rotation = -Vector2.Angle(Attackdirection, Vector2.left);
    }

    private void Update()
    {
        if (!attackPending)
            return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f)
            return;

        attackPending = false;
        attackTimer = -1f;
        audioService.PlaySfx(soundAttack);
        rig.isKinematic = false;
        rig.AddRelativeForce(new Vector2(-AttackForce, 0f));
    }

    public void Attack()
    {
        transform.Rotate(Vector3.forward, rotation);
        attackPending = true;
        attackTimer = delayAttack;
    }

    public void Dead()
    {
        audioService.PlaySfx(soundDead);

        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
            spawnItem.SpawnItem();

        gameSession.AddPoint(scoreRewarded);
        if (deadFx != null)
            Instantiate(deadFx, transform.position, Quaternion.identity);

        rig.linearVelocity = Vector2.zero;

        foreach (var box in GetComponents<BoxCollider2D>())
            box.enabled = false;

        foreach (var circle in GetComponents<CircleCollider2D>())
            circle.enabled = false;
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        Dead();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Attackdirection);
    }
}
