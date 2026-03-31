using UnityEngine;
using System.Collections;

public class Star : MonoBehaviour {
	float speed = 2f;

	void Start(){
	}
	// Update is called once per frame
	void Update () {
		transform.position = Vector3.MoveTowards (transform.position, GameManager.Instance.Player.transform.position, speed * Time.deltaTime);
	}
}
