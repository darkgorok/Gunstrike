using UnityEngine;
using System.Collections;

public class AutoRotate : MonoBehaviour {
	public float speed = 100;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround (transform.position, Vector3.forward, speed * Time.deltaTime);
	}
}
