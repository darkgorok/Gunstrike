using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingObj : MonoBehaviour {
	Vector2 originalPos;
	Rigidbody2D rig;
	bool isFalling = false;
	// Use this for initialization
	void Start () {
		originalPos = transform.position;
		rig = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	#region IListener implementation

	public void IPlay ()
	{
	}

	public void ISuccess ()
	{
	}

	public void IPause ()
	{
	}

	public void IUnPause ()
	{
	}

	public void IGameOver ()
	{
	}

	public void IOnRespawn ()
	{
		rig.linearVelocity = Vector2.zero;
		transform.position = originalPos;
		transform.rotation = Quaternion.Euler (0, 0, 0);
		rig.freezeRotation = true;
		rig.freezeRotation = false;
		rig.isKinematic = true;
	}
	public void IOnStopMovingOn ()
	{
		rig.isKinematic = true;
		if (rig.linearVelocity.y != 0)
			isFalling = true;
		
		rig.linearVelocity = Vector2.zero;
	}

	public void IOnStopMovingOff ()
	{
		if (isFalling)
			rig.isKinematic = false;
	}

	#endregion
}
