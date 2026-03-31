using UnityEngine;
using System.Collections;

public class PathedProjectile : MonoBehaviour, ICanTakeDamage {

	Transform _destination;
	float _speed;
	public bool canBeKill;

	public GameObject DestroyEffect;
	public int pointToGivePlayer;

	public AudioClip soundDestroy;
	[Range(0,1)]
	public float soundDestroyVolume = 0.5f;

	// Use this for initialization
	public void Initalize (Transform destination, float speed) {
		_destination = destination;
		_speed = speed;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = Vector3.MoveTowards (transform.position, _destination.position, Time.deltaTime * _speed);
		var distance = (_destination.position - transform.position).sqrMagnitude;
		if (distance > 0.1f * 0.1f)
			return;

		if (DestroyEffect != null)
			Instantiate (DestroyEffect, transform.position, Quaternion.identity);
		
		Destroy (gameObject);
	}


	void ICanTakeDamage.TakeDamage (int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
	{
		if (!canBeKill)
			return;
		
		if (DestroyEffect != null)
			Instantiate (DestroyEffect, transform.position, Quaternion.identity);

		Destroy (gameObject);
		SoundManager.PlaySfx (soundDestroy, soundDestroyVolume);

		var projectile = instigator.GetComponent<Projectile> ();
		if (projectile != null && projectile.Owner.GetComponent<Player> () != null && pointToGivePlayer != 0) {
			GameManager.Instance.AddPoint (pointToGivePlayer);
			//GameManager.Instance.ShowFloatingText ("+" + pointToGivePlayer, transform.position,Color.yellow);
		}
	}

}
