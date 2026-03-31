using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEventHelper : MonoBehaviour
{
	public SmartEnemyGrounded smartEnemy;

	public void AnimMeleeAttackStart()
	{
		smartEnemy.AnimMeleeAttackStart();
	}

	public void AnimMeleeAttackEnd()
	{
		smartEnemy.AnimMeleeAttackEnd();
	}

	public void AnimThrow()
	{
		smartEnemy.AnimThrow();
	}
}
