//Spawn item when enemy die


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnItem : MonoBehaviour {

    [Header("Spawn random item")]
    public bool useSpawn = true;
    public int spawnNumber = 1;
    [Range(0, 1)]
    public float chanceSpawn = 0.5f;
    public ItemType[] randomChanceItem;
    
    [Header("Force Spawn item")]
    public bool useForceSpawnItem = true;
    public ItemType[] forceSpawnRandomItem;
    public GameObject forceSpawnObj;
    
    [Header("SET UP")]
    public AudioClip sound;
    public Vector2 localOffset = new Vector2(0, 1);
    public Vector2 spawnForce = new Vector2(0, 3);

    public void SpawnItem()
    {
        if (!useSpawn && !useForceSpawnItem)
            return;

        if (useSpawn)
        {
            for (int i = 0; i < spawnNumber; i++)
            {
                if (randomChanceItem.Length > 0)
                {
                    if (Random.Range(0f, 1f) < chanceSpawn)
                    {
                        int pickObj = Random.Range(0, randomChanceItem.Length);
                        if (randomChanceItem[pickObj] != null)
                        {
                            Instantiate(randomChanceItem[pickObj], transform.position + (Vector3)localOffset, Quaternion.identity).Init(true, spawnForce);
                        }
                    }
                }
            }
        }

        if (useForceSpawnItem)
        {
            if (forceSpawnObj)
                Instantiate(forceSpawnObj, transform.position + (Vector3)localOffset, Quaternion.identity);
            if (forceSpawnRandomItem.Length > 0)
            {
                int pickObj = Random.Range(0, forceSpawnRandomItem.Length);
                if (forceSpawnRandomItem[pickObj] != null)
                {
                    Instantiate(forceSpawnRandomItem[pickObj], transform.position + (Vector3)localOffset, Quaternion.identity).Init(true, spawnForce);
                }
            }
        }

        SoundManager.PlaySfx(sound);
    }
}
