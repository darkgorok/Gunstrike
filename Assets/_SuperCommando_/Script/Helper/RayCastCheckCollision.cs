using UnityEngine;
using System.Collections;
/// <summary>
/// Ray cast check collision. Use this to check the collision to Left, Right, Bottom
/// </summary>
public class RayCastCheckCollision : MonoBehaviour {
	[Tooltip("If this = null, this will be this object transform")]
	public Transform RaycastPoint;
	public LayerMask CollisionMask;
	public float rayLengthLeft = 1f;
	public float rayLengthRight = 1f;
	public float rayLengthBottom = 2f;
	public float rayBottomRange = 0.6f;

	public Collison State;
	// Use this for initialization
	void Start () {
		if (RaycastPoint == null)
			RaycastPoint = transform;

		CheckCollision ();
	}
	
	// Update is called once per frame
	void Update () {
		CheckCollision ();
	}

	private void CheckCollision(){
		State.isCollidingLeft = Physics2D.Raycast (RaycastPoint.position + new Vector3 (0, .2f, 0), Vector2.left, rayLengthLeft, CollisionMask);
		State.isCollidingRight = Physics2D.Raycast (RaycastPoint.position + new Vector3 (0, .2f, 0), Vector2.right, rayLengthRight, CollisionMask);
		State.isCollidingBottomLeft = Physics2D.Raycast (RaycastPoint.position + new Vector3 (-rayBottomRange / 2, -0.1f, 0), Vector2.down, rayLengthBottom, CollisionMask);
		State.isCollidingBottomRight = Physics2D.Raycast (RaycastPoint.position + new Vector3 (rayBottomRange / 2, -0.1f, 0), Vector2.down, rayLengthBottom, CollisionMask);

	}

	public struct Collison{
		public bool isCollidingLeft{ get;  set;}
		public bool isCollidingRight{ get;  set;}
		public bool isCollidingBottomLeft{ get;  set;}
		public bool isCollidingBottomRight{ get;  set;}
	}

	public void OnDrawGizmosSelected (){
		if (RaycastPoint == null)
			RaycastPoint = transform;
		Gizmos.color = Color.red;
		Gizmos.DrawRay (RaycastPoint.position + new Vector3 (0, .2f, 0), Vector3.left * rayLengthLeft);
		Gizmos.DrawRay (RaycastPoint.position + new Vector3 (0, .2f, 0), Vector3.right * rayLengthRight);
		Gizmos.DrawRay (RaycastPoint.position + new Vector3 (-rayBottomRange / 2, 0.1f, 0), Vector3.down * rayLengthBottom);
		Gizmos.DrawRay (RaycastPoint.position + new Vector3 (rayBottomRange / 2, 0.1f, 0), Vector3.down * rayLengthBottom);
	}
}
