using UnityEngine;
using System.Collections;

public class SpawnItem : MonoBehaviour {
	public float timeToLive = 5f;
	public Vector2 minForce;
	public Vector2 maxForce;

	Rigidbody2D rig;

	// Use this for initialization
	void Start () {
		rig = GetComponent<Rigidbody2D> ();
		rig.linearVelocity = new Vector2 (Random.Range (minForce.x, maxForce.x), Random.Range (minForce.y, maxForce.y));

		Destroy (gameObject,timeToLive);
	}
}
