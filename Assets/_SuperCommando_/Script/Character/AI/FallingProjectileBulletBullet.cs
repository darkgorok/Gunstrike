using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingProjectileBulletBullet : MonoBehaviour, ICanTakeDamage {
	

	public LayerMask targetLayer;
	public float speed = 1;

	public GameObject ExplosionFX;
	public AudioClip soundExplosion;

	public int damageToGive = 100;
	public float timeDestroy = 10;

	Vector2 direction;
	GameObject Owner;


	bool allowMoving = false;

	void OnEnable(){
	}

	public void Action (Vector2 _direction, float _speed, GameObject owner = null){
		//		Debug.LogError (_direction);
		speed = _speed;
		direction = _direction;
		Owner = owner;
		allowMoving = true;

		transform.right = direction;
	}

	void Start(){
		allowMoving = false;
	}

	// Update is called once per frame
	void  Update () {
		if (!allowMoving)
			return;
		
		transform.Translate (speed * Time.deltaTime, 0, 0,Space.Self);
	}

	void OnTriggerEnter2D(Collider2D other){
		if (!allowMoving)
			return;

        if (targetLayer != (targetLayer | (1 << other.gameObject.layer)))
            return;
		
		if (Owner!=null/* &&  other.gameObject.GetComponent<Rocket>()!=null*/ && other.gameObject.GetComponent<FallingProjectileBulletBullet>()!=null && other.gameObject.GetComponent<FallingProjectileBulletBullet>().Owner == Owner)
			return;

		SoundManager.PlaySfx (soundExplosion);

		var damage = (ICanTakeDamage)other.gameObject.GetComponent (typeof(ICanTakeDamage));
		if (damage == null || other.GetComponent<Player>()) {

			if (damage != null && targetLayer == (targetLayer | (1 << other.gameObject.layer))) {
				damage.TakeDamage (damageToGive, Vector2.zero, GameManager.Instance.Player.gameObject, Vector2.zero);
			}

			if (ExplosionFX != null)
				Instantiate (ExplosionFX, transform.position, Quaternion.identity);

			Destroy (gameObject);
			return;
		}

		if (targetLayer == (targetLayer | (1 << other.gameObject.layer)))
			damage.TakeDamage (damageToGive, Vector2.zero, GameManager.Instance.Player.gameObject, Vector2.zero);

		if (ExplosionFX != null)
			Instantiate (ExplosionFX, transform.position, Quaternion.identity);

		Destroy (gameObject);
	}

	#region ICanTakeDamage implementation
	public void TakeDamage (int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
	{
		if (!allowMoving)
			return;
		
		if (ExplosionFX != null)
			Instantiate (ExplosionFX, transform.position, Quaternion.identity);

		Destroy (gameObject);
	}
	#endregion
}
