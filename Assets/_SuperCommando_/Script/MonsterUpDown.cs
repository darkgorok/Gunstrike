using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterUpDown : MonoBehaviour, ICanTakeDamage {
    public GameObject destroyFX;
    public AudioClip deadSound;

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (destroyFX)
            Instantiate(destroyFX, transform.position, Quaternion.identity);

        SoundManager.PlaySfx(deadSound);

        //try spawn random item
        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
        {
            spawnItem.SpawnItem();
        }

        //gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
