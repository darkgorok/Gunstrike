using UnityEngine;
using System.Collections;
using VContainer;

public class Block : MonoBehaviour, ICanTakeDamage
{
    public enum BlockTyle { Destroyable, Rocky }

    public BlockTyle blockTyle;
    public LayerMask enemiesLayer;

    public int maxHit = 1;
    public float pushEnemyUp = 7f;
    public float sizeDetectEnemies = 0.25f;
    public int pointToAdd = 100;

    [Header("Destroyable")]
    public GameObject DestroyEffect;

    [Header("HidenTreasure")]
    public GameObject[] Treasure;
    public Transform spawnPoint;
    public Sprite imageBlockStatic;

    [Header("Sound")]
    public AudioClip soundDestroy;

    [Range(0, 1)]
    public float soundDestroyVolume = 0.5f;

    public AudioClip soundSpawn;

    [Range(0, 1)]
    public float soundSpawnVolume = 0.5f;

    Animator anim;
    SpriteRenderer spriteRenderer;
    Sprite oldSprite;
    int currentHitLeft;
    bool isWaitNextHit;
    private IAudioService audioService;

    [Inject]
    private void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    void Start()
    {
        ProjectScope.Inject(this);
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        oldSprite = spriteRenderer.sprite;
        currentHitLeft = Mathf.Clamp(maxHit, 1, int.MaxValue);
    }

    public void BoxHit()
    {
        if (isWaitNextHit || currentHitLeft <= 0)
            return;

        StartCoroutine(BoxHitCo());
    }

    IEnumerator BoxHitCo()
    {
        isWaitNextHit = true;

        var random = Treasure.Length > 0 ? Treasure[Random.Range(0, Treasure.Length)] : null;
        if (random != null)
        {
            Instantiate(random, spawnPoint.position, Quaternion.identity);
            audioService?.PlaySfx(soundSpawn, soundSpawnVolume);
        }

        CheckEnemiesOnTop();
        anim.SetTrigger("hit");

        currentHitLeft--;
        if (currentHitLeft > 0)
        {
            yield return null;
            isWaitNextHit = false;
            yield break;
        }

        if (blockTyle == BlockTyle.Destroyable)
        {
            if (random == null)
                audioService?.PlaySfx(soundDestroy, soundDestroyVolume);

            if (DestroyEffect != null)
                Instantiate(DestroyEffect, transform.position, Quaternion.identity);

            isWaitNextHit = false;
            Destroy(gameObject);
        }
        else if (blockTyle == BlockTyle.Rocky)
        {
            spriteRenderer.sprite = imageBlockStatic;
        }

        yield return null;
        isWaitNextHit = false;
    }

    void CheckEnemiesOnTop()
    {
        var hits = Physics2D.CircleCastAll(spawnPoint.position, sizeDetectEnemies, Vector2.zero, 0, 1 << LayerMask.NameToLayer("Enemies"));
        if (hits.Length <= 0)
            return;

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.GetComponent<Block>() != null)
                continue;

            var damage = (ICanTakeDamage)hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage));
            if (damage != null)
                damage.TakeDamage(10000, Vector2.up * pushEnemyUp, gameObject, Vector2.zero);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnPoint.position, sizeDetectEnemies);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        BoxHit();
    }
}
