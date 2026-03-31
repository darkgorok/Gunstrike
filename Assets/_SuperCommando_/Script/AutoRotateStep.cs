using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotateStep : MonoBehaviour
{
    public float delayOnStart = 0;
    public float rate = 1;
    public float angleStep = 45;
    public Transform target;
    public float speedRotate = 10;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(delayOnStart);

        while (true)
        {
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

            yield return new WaitForSeconds(rate);
        }
    }
}
