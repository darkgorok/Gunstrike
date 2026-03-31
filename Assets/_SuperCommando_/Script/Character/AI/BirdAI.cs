using UnityEngine;
using System.Collections;

public class BirdAI : EnemyAI {
	[Header("Owner Setup")]
	public bool chasePlayer = true;
	public float chaseSpeed = 3;
	public float offsetPlayerY = 0.5f;
	public float finishDistance = 0.5f;

	public Animator animator;
	public string hitEventName="n/a";
	public string deadEventName="n/a";

	public bool isChasing{ get; set; }
	private float _directionFace;

	public override void LateUpdate ()
	{
		if (!isChasing || !isPlaying)
			base.LateUpdate ();
		else if (isPlaying) {
			if (Mathf.Abs (Vector3.Distance (transform.position, GameManager.Instance.Player.transform.position)) > finishDistance) {
				transform.position = Vector3.MoveTowards (transform.position, GameManager.Instance.Player.transform.position + new Vector3(0,offsetPlayerY,0), chaseSpeed * Time.deltaTime);
				_directionFace = transform.position.x > GameManager.Instance.Player.transform.position.x ? 1 : -1;
				transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * _directionFace, transform.localScale.y, transform.localScale.z);
			}
		}
	}

	protected override void HitEvent ()
	{
		base.HitEvent ();
		if (isDead)
			Dead ();
		
		if (animator != null && hitEventName.CompareTo ("n/a") != 0)
			animator.SetTrigger (hitEventName);

	}

	protected override void Dead ()
	{
		if (animator != null && deadEventName.CompareTo ("n/a") != 0)
			animator.SetTrigger (deadEventName);

		base.Dead ();

		isChasing = false;
		gravity = 35f;
		SetForce (0, 7);
		controller.HandlePhysic = false;
	}

	protected override void OnRespawn ()
	{
		base.OnRespawn ();
		gravity = 0;
		isChasing = false;
		controller.HandlePhysic = true;
	}
}
