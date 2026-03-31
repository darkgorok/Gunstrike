using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjController : MonoBehaviour, ICanTakeDamage
{
    public Animator anim;
    public float angleStep = 45;
    public Transform target;
    public float speedRotate = 10;
    
    bool isRotating = false;

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (isRotating)
            return;

        anim.SetBool("open",true);
        StartCoroutine(RotatingCo());
    }

    IEnumerator RotatingCo()
    {
        isRotating = true;
        float fromAngle = target.rotation.eulerAngles.z;
        float toAngle = fromAngle + angleStep;
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * speedRotate;
            percent = Mathf.Clamp01(percent);
            float newAngle = Mathf.Lerp(fromAngle, toAngle, percent);
            target.rotation = Quaternion.Euler(0, 0, newAngle);
            yield return null;
        }
        isRotating = false;
        anim.SetBool("open", false);
    }
}
