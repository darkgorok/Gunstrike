using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenTreasure : MonoBehaviour, ICanTakeDamage
{
    public enum BlockTyle { Destroyable, ChangeSprite }
    public BlockTyle blockTyle;
    public Sprite changeSprite;
    public GameObject destroyFX;

    //public Vector2 localSpawnPoint = new Vector2(0, 0.5f);
    //[Header("Spawn item")]
    //public Vector2 spawnForce = new Vector2(5, 5);
    //public bool spawnItem = true;
    //public ItemType[] randomItem;
    //[Header("Spawn item chance")]
    //public bool spawnChanceItem = true;
    //[Range(0,1)]
    //public float chanceSpawn = 0.5f;
    //public ItemType[] randomChanceItem;
    [Range(0,1)]
    public float volume = 0.6f;
    public AudioClip sound;

    bool isWorked = false;
    
   public void DestroyAndGivePlayerProp()
    {
        TakeDamage(1000, Vector2.zero, GameManager.Instance.Player.gameObject, Vector2.zero);
    }

    public void BoxHit()
    {
        TakeDamage(1000, Vector2.zero, GameManager.Instance.Player.gameObject, Vector2.zero);
        GameManager.Instance.Player.velocity.y = 0;
    }

    #region ICanTakeDamage implementation

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (isWorked)
            return;

        isWorked = true;

        //try spawn random item
        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
        {
            spawnItem.SpawnItem();
        }

        GetComponent<Collider2D>().enabled = false;
        
        SoundManager.PlaySfx(sound, volume);
        
        if (blockTyle == BlockTyle.Destroyable)
        {
            if (destroyFX)
                Instantiate(destroyFX, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
        else if(blockTyle == BlockTyle.ChangeSprite)
        {
            GetComponent<SpriteRenderer>().sprite = changeSprite;
        }
    }

    #endregion
}
