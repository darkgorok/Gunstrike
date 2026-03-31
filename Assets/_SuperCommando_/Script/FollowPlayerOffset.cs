using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerOffset : MonoBehaviour {
	public float speed = 1;

	float cameraX, cameraY;

	float offsetX, offsetY;
	Vector3 originalCamera, originalThis;
	Vector3 newPos;
	// Use this for initialization
	void Start () {
		originalThis = transform.position;
		originalCamera = Camera.main.transform.position;
//		offsetX = Camera.main.transform.position.x - transform.position.x;
//		offsetY = Camera.main.transform.position.y - transform.position.y;
	}
	
	// Update is called once per frame
	void Update () {
		
		newPos = Camera.main.transform.position - originalCamera;

		transform.position = new Vector3 (originalThis.x + newPos.x * speed*0.01f, originalThis.y + newPos.y * speed*0.01f, transform.position.z);
	}
}
