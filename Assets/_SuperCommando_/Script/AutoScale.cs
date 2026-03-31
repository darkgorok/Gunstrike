using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoScale : MonoBehaviour
{
    public Vector3 scaleA = new Vector3(1, 1, 1);
    public Vector3 scaleB = new Vector3(1.2f, 1.2f, 1.2f);
    public float speedAB = 1;
    public float speedBA = 2;

    public Transform target;
    Transform _target;
    float percent = 0;
    bool isAB = true;

    private void Start()
    {
        if (target == null)
            _target = transform;
        else
            _target = target;

        percent = 0;
    }
    private void Update()
    {
        if (percent < 1)
        {

            if (isAB)
            {
                percent += speedAB * Time.deltaTime;
                _target.localScale = Vector3.Lerp(scaleA, scaleB, percent);
            }
            else
            {
                percent += speedBA * Time.deltaTime;
                _target.localScale = Vector3.Lerp(scaleB, scaleA, percent);
            }
        }
        else
        {
            percent = 0;
            isAB = !isAB;
        }
    }
}
