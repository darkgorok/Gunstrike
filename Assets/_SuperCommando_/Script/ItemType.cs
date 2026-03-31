using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemType : MonoBehaviour
{
    public enum Type { coin, dart, hearth } // note: "hearth" kept to avoid breaking existing references
    public Type itemType;

    [Header("VALUES")]
    public int amount = 1;
    [Range(0f, 1f)] public float soundVol = 0.8f;
    public AudioClip sound;

    [Header("OPTION")]
    public bool gravity = false;
    public float timeLiveAfterSpawned = 6f;
    public Vector2 forceSpawn = new Vector2(-5f, 5f); // x = horizontal range (abs used), y = upward force
    public GameObject effect;

    private Rigidbody2D rig;
    private Collider2D col;
    private bool isCollected = false;
    private bool allowCollect = false;

    public void Init(bool useGravity, Vector2 pushForce)
    {
        gravity = useGravity;
        if (pushForce != Vector2.zero)
            forceSpawn = pushForce;
    }

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
    }

    private IEnumerator Start()
    {
        if (gravity)
        {
            // Ensure a Rigidbody2D exists
            rig = GetComponent<Rigidbody2D>();
            if (rig == null) rig = gameObject.AddComponent<Rigidbody2D>();

            // Freeze rotation via constraints (modern, reliable)
            rig.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Solid collision while falling
            col.isTrigger = false;

            // Apply initial toss: random horizontal within ±|forceSpawn.x|, vertical = forceSpawn.y
            float horiz = Random.Range(-Mathf.Abs(forceSpawn.x), Mathf.Abs(forceSpawn.x));
            rig.linearVelocity = new Vector2(horiz, forceSpawn.y);

            // Small delay so physics steps kick in
            yield return new WaitForSeconds(0.1f);

            // Wait until the item starts falling downwards
            while (rig != null && rig.linearVelocity.y > 0f)
                yield return null;

            // Now allow collection, then auto-despawn after lifetime
            allowCollect = true;
            yield return new WaitForSeconds(timeLiveAfterSpawned);
            if (this != null) Destroy(gameObject);
        }
        else
        {
            // Collectible immediately; trigger collider for overlap
            col.isTrigger = true;
            allowCollect = true;
            yield break;
        }
    }

    // Called by Player
    public void Collect()
    {
        if (!allowCollect || isCollected) return;
        isCollected = true;

        switch (itemType)
        {
            case Type.coin:
                // GameManager.Instance.AddCoin(amount);
                GlobalValue.SavedCoins += amount;
                break;

            case Type.dart:
                GameManager.Instance.AddBullet(amount);
                break;

            case Type.hearth:
                GameManager.Instance.Player.GiveHealth(amount, gameObject);
                break;
        }

        SoundManager.PlaySfx(sound, soundVol);

        if (effect != null)
        {
            var vfx = SpawnSystemHelper.GetNextObject(effect, true);
            vfx.transform.position = transform.position;
        }

        Destroy(gameObject);
    }
}
