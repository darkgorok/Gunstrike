using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFlag : MonoBehaviour {

	public Sprite tutorialSprite;

	void OnTriggerEnter2D(Collider2D other){
		if (other.gameObject.GetComponent<Player> () == null)
			return;

		Tutorial.Instance.Open (tutorialSprite);
		GetComponent<BoxCollider2D>().enabled = false;
	}
}
