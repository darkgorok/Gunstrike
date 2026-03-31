using UnityEngine;
using System.Collections;

public class MonsterSnail : EnemyAI {
	[Header("Owner")]
	public Animator anim;
	public float timeBackToAlive = 3f;

	public override void Start ()
	{
		base.Start ();
		healthType = HealthType.HitToKill;		//force to HitToKill
	}

	protected override void HitEvent ()
	{
		base.HitEvent ();

		if (currentHitLeft == 1) {
			anim.SetBool ("hit", true);
			isPlaying = false;
			SoundManager.PlaySfx (hurtSound, hurtSoundVolume);
			if (HurtEffect != null)
				Instantiate (HurtEffect, transform.position, transform.rotation);

			StartCoroutine (BackToAliveCo (timeBackToAlive));
		} else if (isDead)
			Dead ();
	}

	protected override void Dead ()
	{
		StopAllCoroutines ();
		base.Dead ();

		SetForce (0, 5);
		controller.HandlePhysic = false;
	}

	protected override void OnRespawn ()
	{
		anim.SetBool ("hit", false);
		controller.HandlePhysic = true;
	}

	IEnumerator BackToAliveCo(float time){
		isSocking = true;
		yield return new WaitForSeconds (time - 1f);

		anim.SetTrigger ("shake");

		yield return new WaitForSeconds (1f);
		anim.SetBool ("hit", false);
		currentHitLeft = maxHitToKill;		//reset hit
		isSocking = false;
	}
}
