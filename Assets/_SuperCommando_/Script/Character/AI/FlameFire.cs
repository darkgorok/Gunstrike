using UnityEngine;
using System.Collections;

public class FlameFire : MonoBehaviour {
	[Range(2,10)]
	public float fireRate = 3f;
	public int damageToPlayer = 50;

	Animator anim;
	bool isFiring = false;

	void Start () {
		anim = GetComponent<Animator> ();
		StartCoroutine (FireRateCo (fireRate));
	}

	public void Fire(){
		anim.SetTrigger ("fire");
		isFiring = true;
	}

	public void Stopped(){
		isFiring = false;
	}
	
	IEnumerator FireRateCo(float time){
		yield return new WaitForSeconds (time);

		Fire ();
		StartCoroutine (FireRateCo (fireRate));
	}

	void OnTriggerStay2D(Collider2D other){
		if (!isFiring || other.GetComponent<Player>()==null)
			return;

		other.GetComponent<Player> ().TakeDamage (damageToPlayer, new Vector2 (5, 5), gameObject, Vector2.zero);
		isFiring = false;
	}
}
