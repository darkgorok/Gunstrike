using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AimHelperEnemy {
	public static float offsetTargetY = 0;
	public static float shootAngle{get;set;}

	public static float Aim(Transform fromPos, Transform toPos, bool isFacingRight){
		Vector2 diff = toPos.position + Vector3.up*offsetTargetY - fromPos.position;
		diff.Normalize();
		float angle = Mathf.Atan2 (diff.y, diff.x) * Mathf.Rad2Deg;

		shootAngle = angle;
		return shootAngle;
	}
}
