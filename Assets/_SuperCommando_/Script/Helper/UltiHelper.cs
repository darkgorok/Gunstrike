using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltiHelper {

    //get random with weighted object
    public static int GetRandomWeightedIndex(List<float> weights)
    {
        if (weights == null || weights.Count == 0) return -1;

        float w = 0;
        float t = 0;
        int i;
        for (i = 0; i < weights.Count; i++)
        {
            w = weights[i];
            if (float.IsPositiveInfinity(w)) return i;
            else if (w >= 0f && !float.IsNaN(w)) t += weights[i];
        }

        float r = Random.value;
        float s = 0f;

        for (i = 0; i < weights.Count; i++)
        {
            w = weights[i];
            if (float.IsNaN(w) || w <= 0f) continue;

            s += w / t;
            if (s >= r) return i;
        }

        return -1;
    }

    public static int GetRandomWeightedIndex(float[] weights)
    {
        if (weights == null || weights.Length == 0) return -1;

        float w = 0;
        float t = 0;
        int i;
        for (i = 0; i < weights.Length; i++)
        {
            w = weights[i];
            if (float.IsPositiveInfinity(w)) return i;
            else if (w >= 0f && !float.IsNaN(w)) t += weights[i];
        }

        float r = Random.value;
        float s = 0f;

        for (i = 0; i < weights.Length; i++)
        {
            w = weights[i];
            if (float.IsNaN(w) || w <= 0f) continue;

            s += w / t;
            if (s >= r) return i;
        }

        return -1;
    }

    public static float angleInput(Vector2 input ){
		float angle = Mathf.Atan2 (input.y, input.x) * Mathf.Rad2Deg;

		return angle;
	}

	//returns the duration of an animation
	public static float GetAnimDuration(string animName, Animator anim) {
		RuntimeAnimatorController ac = anim.runtimeAnimatorController;
		for (int i = 0; i < ac.animationClips.Length; i++) {
			if (ac.animationClips [i].name == animName) {
				return ac.animationClips [i].length / anim.speed;
			}
		}
		Debug.LogError ("no animation found with name: " + animName);
		return 0f;
	}
    
    public static void LookAtTarget(GameObject owner, GameObject target, float offsetZ = 0)
    {
        //foreach (var obj in rotateObj)
        //{
            Vector3 diff = target.transform.position - owner.transform.position;
            diff.Normalize();
            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            float finalZ = rot_z;
        //Debug.LogError(finalZ);
        owner.transform.rotation = Quaternion.Euler(0, 0, finalZ + offsetZ);
        //}
    }

    public static Vector2 AngleToVector2(float degree)
    {
        Vector2 dir = (Vector2)(Quaternion.Euler(0, 0, degree) * Vector2.right);

        return dir;
    }

    public static float Vector2ToAngle(Vector2 vec2)
    {
        var angle = Mathf.Atan2(vec2.y, vec2.x) * Mathf.Rad2Deg;
        return angle;
    }
}
