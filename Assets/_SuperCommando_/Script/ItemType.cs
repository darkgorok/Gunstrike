using System.Collections;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Collider2D))]
public class ItemType : MonoBehaviour
{
    public enum Type { coin, dart, hearth }
    public Type itemType;

    [Header("VALUES")]
    public int amount = 1;
    [Range(0f, 1f)] public float soundVol = 0.8f;
    public AudioClip sound;

    [Header("OPTION")]
    public bool gravity = false;
    public float timeLiveAfterSpawned = 6f;
    public Vector2 forceSpawn = new Vector2(-5f, 5f);
    public GameObject effect;

    private Rigidbody2D rig;
    private Collider2D col;
    private bool isCollected = false;
    private bool allowCollect = false;
    private IAudioService audioService;
    private IGameSessionService gameSession;
    private IProgressService progressService;

    [Inject]
    public void Construct(IAudioService audioService, IGameSessionService gameSession, IProgressService progressService)
    {
        this.audioService = audioService;
        this.gameSession = gameSession;
        this.progressService = progressService;
    }

    public void Init(bool useGravity, Vector2 pushForce)
    {
        gravity = useGravity;
        if (pushForce != Vector2.zero)
            forceSpawn = pushForce;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        col = GetComponent<Collider2D>();
        if (col == null)
            col = gameObject.AddComponent<BoxCollider2D>();
    }

    private IEnumerator Start()
    {
        if (gravity)
        {
            rig = GetComponent<Rigidbody2D>();
            if (rig == null)
                rig = gameObject.AddComponent<Rigidbody2D>();

            rig.constraints = RigidbodyConstraints2D.FreezeRotation;
            col.isTrigger = false;

            float horiz = Random.Range(-Mathf.Abs(forceSpawn.x), Mathf.Abs(forceSpawn.x));
            rig.linearVelocity = new Vector2(horiz, forceSpawn.y);

            yield return new WaitForSeconds(0.1f);
            while (rig != null && rig.linearVelocity.y > 0f)
                yield return null;

            allowCollect = true;
            yield return new WaitForSeconds(timeLiveAfterSpawned);
            if (this != null)
                Destroy(gameObject);
            yield break;
        }

        col.isTrigger = true;
        allowCollect = true;
    }

    public void Collect()
    {
        if (!allowCollect || isCollected)
            return;

        isCollected = true;

        switch (itemType)
        {
            case Type.coin:
                progressService.SavedCoins += amount;
                break;
            case Type.dart:
                gameSession.AddBullet(amount);
                break;
            case Type.hearth:
                gameSession.Player.GiveHealth(amount, gameObject);
                break;
        }

        audioService.PlaySfx(sound, soundVol);

        if (effect != null)
        {
            GameObject vfx = SpawnSystemHelper.GetNextObject(effect, true);
            vfx.transform.position = transform.position;
        }

        Destroy(gameObject);
    }
}
