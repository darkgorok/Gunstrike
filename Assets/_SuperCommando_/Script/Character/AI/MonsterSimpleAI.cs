using UnityEngine;
using System.Collections;

public class MonsterSimpleAI : EnemyAI {
	[Header("Owner Setup")]
	public Animator animator;
	public string hitEventName="n/a";
	public string deadEventName="n/a";

	protected override void HitEvent ()
	{
		base.HitEvent ();
		if (isDead)
			Dead ();

		if (animator != null && hitEventName.CompareTo ("n/a") != 0)
			animator.SetTrigger (hitEventName);
	}

	public override void Update ()
	{
		base.Update ();

	}

	protected override void Dead ()
	{
		base.Dead ();
		if (animator != null && deadEventName.CompareTo ("n/a") != 0)
			animator.SetTrigger (deadEventName);
		
		//SetForce (0, 5);
		//controller.HandlePhysic = false;

  //      Destroy(gameObject);
	}

	protected override void OnRespawn ()
	{
		base.OnRespawn ();
		controller.HandlePhysic = true;
	}
}
