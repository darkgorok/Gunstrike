using UnityEngine;
using System.Collections;

public class MonsterFish : MonoBehaviour, ICanTakeDamage {
	public Vector2 Attackdirection;
	public float AttackForce = 750;

	public float delayAttack = 0.35f;
	public AudioClip soundAttack;
	public AudioClip soundDead;
	public GameObject deadFx;
	public int scoreRewarded = 200;

	Vector3 oldPosition;
	float rotation;
	Rigidbody2D rig;

	void Start(){
		rig = GetComponent<Rigidbody2D> ();
		oldPosition = transform.position;
		rotation = -Vector2.Angle (Attackdirection, Vector2.left);
	}

	public void Attack(){
		transform.Rotate (Vector3.forward, rotation);
		StartCoroutine (WaitAndAttack (delayAttack));
	}

	IEnumerator WaitAndAttack(float time){
		yield return new WaitForSeconds (time);
		SoundManager.PlaySfx (soundAttack);
		rig.isKinematic = false;
		rig.AddRelativeForce(new Vector2(-AttackForce,0));
	}

	public void Dead(){
		SoundManager.PlaySfx(soundDead);
        //try spawn random item
        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
        {
            spawnItem.SpawnItem();
        }

        GameManager.Instance.AddPoint(scoreRewarded);
		if (deadFx != null)
			Instantiate (deadFx, transform.position, Quaternion.identity);

		rig.linearVelocity = Vector2.zero;

		//turn off all colliders if the enemy have
		var boxCo = GetComponents<BoxCollider2D> ();
		foreach (var box in boxCo) {
			box.enabled = false;
		}
		var CirCo = GetComponents<CircleCollider2D> ();
		foreach (var cir in CirCo) {
			cir.enabled = false;
		}
	}
    
	public void TakeDamage (int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
	{
		Dead ();
	}

	void OnDrawGizmosSelected(){
		Gizmos.color = Color.yellow;
		Gizmos.DrawRay (transform.position, Attackdirection);
	}

}