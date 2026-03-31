using UnityEngine;
using System.Collections;

public class PathedProjectileSpawner : MonoBehaviour {

	public Transform Destination;
	public PathedProjectile Projectile;
	public GameObject SpawnEffect;

	public AudioClip spawnSound;
	[Range(0,1)]
	public float spawnSoundVolume = 0.5f;

	public float speed;
	public float fireRate;
	float nextFireRate;

	// Use this for initialization
	void Start () {
		nextFireRate = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (GameManager.Instance.State != GameManager.GameState.Playing)
			return;
		
		if (Time.time < nextFireRate + fireRate)
			return;

		nextFireRate = Time.time;

		var projectile = (PathedProjectile) Instantiate (Projectile, transform.position, Quaternion.identity);
		projectile.Initalize (Destination, speed);

		if (SpawnEffect != null)
			Instantiate (SpawnEffect, transform.position, Quaternion.identity);

		SoundManager.PlaySfx (spawnSound, spawnSoundVolume);
	}

	public void OnDrawGizmos(){
		if (Destination == null)
			return;

		Gizmos.color = Color.red;
		Gizmos.DrawLine (transform.position, Destination.position);
	}
}
