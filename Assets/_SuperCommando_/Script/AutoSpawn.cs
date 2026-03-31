using UnityEngine;
using System.Collections;

public class AutoSpawn : MonoBehaviour {
	public GameObject[] SpawnObjects;
	public int maxItemsSpawned = 7;
	int counter;
	[Tooltip("start spawn item when enable or wait for command message")]
	public bool autoSpawn = false;

	public float TimeMin;
	public float TimeMax;

	public AudioClip spawnSound;
	[Range(0,1)]
	public float spawnSoundVolume = 0.5f;


	// Use this for initialization
	void Start () {
		if (autoSpawn)
			Play ();
		
		counter = 0;
	}

	public void Play(){
		StartCoroutine (SpawnEnemy (Random.Range (TimeMin, TimeMax)));
	}

	IEnumerator SpawnEnemy(float delay){
		yield return new WaitForSeconds (delay);
		SoundManager.PlaySfx (spawnSound, spawnSoundVolume);
		Instantiate (SpawnObjects [Random.Range (0, SpawnObjects.Length)], transform.position, Quaternion.identity);
		counter++;


		if (maxItemsSpawned > 0 && counter < maxItemsSpawned && GameManager.Instance.State == GameManager.GameState.Playing)
			StartCoroutine (SpawnEnemy (Random.Range (TimeMin, TimeMax)));
	}
}
