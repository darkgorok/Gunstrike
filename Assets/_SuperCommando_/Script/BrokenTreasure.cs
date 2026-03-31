using UnityEngine;
using VContainer;

public class BrokenTreasure : MonoBehaviour, ICanTakeDamage
{
    public enum BlockTyle { Destroyable, ChangeSprite }

    public BlockTyle blockTyle;
    public Sprite changeSprite;
    public GameObject destroyFX;
    [Range(0, 1)]
    public float volume = 0.6f;
    public AudioClip sound;

    private bool isWorked;
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

    public void DestroyAndGivePlayerProp()
    {
        if (gameSession?.Player == null)
            return;

        TakeDamage(1000, Vector2.zero, gameSession.Player.gameObject, Vector2.zero);
    }

    public void BoxHit()
    {
        if (gameSession?.Player == null)
            return;

        TakeDamage(1000, Vector2.zero, gameSession.Player.gameObject, Vector2.zero);
        gameSession.Player.velocity.y = 0;
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (isWorked)
            return;

        isWorked = true;

        EnemySpawnItem spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
            spawnItem.SpawnItem();

        Collider2D hitCollider = GetComponent<Collider2D>();
        if (hitCollider != null)
            hitCollider.enabled = false;

        audioService?.PlaySfx(sound, volume);

        if (blockTyle == BlockTyle.Destroyable)
        {
            if (destroyFX != null)
                Instantiate(destroyFX, transform.position, Quaternion.identity);

            Destroy(gameObject);
            return;
        }

        if (blockTyle == BlockTyle.ChangeSprite)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.sprite = changeSprite;
        }
    }
}
