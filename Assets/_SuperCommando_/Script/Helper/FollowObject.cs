using UnityEngine;
using System.Collections;

public class FollowObject : MonoBehaviour {
	[Tooltip("if this target == null, the target will be Main Camera")]
	public Transform target;
	public Vector2 offset;
	public bool followX = true;
	public bool followY = true;

    public void Init(Transform _target)
    {
        target = _target;
    }

    void Start(){
		if (target == null)
			target = Camera.main.transform;
	}

	void Update () {
		Vector3 follow = target.position + (Vector3)offset;

		if (followX && followY)
			transform.position = new Vector3 (follow.x, follow.y, transform.position.z);
		else if (followX)
			transform.position = new Vector3 (follow.x, transform.position.y, transform.position.z);
		else if (followY)
			transform.position = new Vector3 (transform.position.x, follow.y, transform.position.z);
	}
}
