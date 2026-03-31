using UnityEngine;
using System.Collections;

public class BirdDetectPlayerHelper : MonoBehaviour {
	public BirdAI bird;

	void OnTriggerEnter2D(Collider2D other){
		if (other.gameObject.GetComponent<Player> () == null)
			return;

		if (!bird.isDead && bird.chasePlayer)
			bird.isChasing = true;
	}
}
