using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class MonsterTriggerHelper : MonoBehaviour {

	public GameObject[] Monsters;
	public Rigidbody2D[] rigidbodyObjs;
	public float delayActiveRigidbody = 0.1f;
    bool isWorked = false;

	void Start(){
		StartCoroutine (DisableAllEnemiesCo (0.1f));

		if (rigidbodyObjs != null) {
			foreach (var rig in rigidbodyObjs) {
				if (rig != null)
					rig.isKinematic = true;
			}
		}
	}

	//when detect Player, set active all monsters in list
	IEnumerator OnTriggerEnter2D(Collider2D other){
        if (isWorked)
            yield break;

        if (other.GetComponent<Player> () == null)
			yield break;

        isWorked = true;


        foreach (var monster in Monsters) {
			if (monster != null)
				monster.SetActive (true);
		}

		foreach (var rig in rigidbodyObjs) {
			if (rig != null) {
				rig.isKinematic = false;
				yield return new WaitForSeconds (delayActiveRigidbody);
			}
		}
	}

	IEnumerator DisableAllEnemiesCo(float delay){
		yield return new WaitForSeconds (delay);
		foreach (var monster in Monsters) {
			if (monster != null)
				monster.SetActive (false);
		}
	}

	void OnDrawGizmosSelected(){
		foreach (var obj in Monsters) {
			Gizmos.DrawLine (transform.position, obj.transform.position);
		}

		if (rigidbodyObjs.Length > 0) {
			foreach (var obj in rigidbodyObjs) {
				if(obj)
					Gizmos.DrawLine (transform.position, obj.transform.position);
			}
		}
	}
}
