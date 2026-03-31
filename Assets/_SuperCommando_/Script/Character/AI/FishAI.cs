using UnityEngine;
using System.Collections;

public class FishAI : MonoBehaviour, ICanTakeDamage {
	
	public GameObject DestroyEffect;
	public AudioClip deadSound;
	[Range(0,1)]
	public float deadSoundVolume = 0.5f;

	Vector3 _oldPosition;

	// Use this for initialization
	void Start () {
		_oldPosition = transform.position;
	}

	public void TakeDamage (int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
	{
		if (DestroyEffect != null)
			Instantiate (DestroyEffect, transform.position, transform.rotation);

		SoundManager.PlaySfx (deadSound, deadSoundVolume);

        //try spawn random item
        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
        {
            spawnItem.SpawnItem();
        }

        Destroy(gameObject);
    }
}
