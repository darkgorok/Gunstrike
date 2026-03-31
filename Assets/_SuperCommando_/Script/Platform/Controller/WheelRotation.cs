using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelRotation : MonoBehaviour
{
    public float speed = 300;
    Vector2 lastPos;
    // Start is called before the first frame update
    void Start()
    {
        lastPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 currentPos = transform.position;
        if (currentPos.x > lastPos.x || currentPos.y > lastPos.y)
            transform.RotateAround(transform.position, Vector3.forward, -speed * Time.deltaTime);
        else if (currentPos.x < lastPos.x || currentPos.y < lastPos.y)
            transform.RotateAround(transform.position, Vector3.forward, speed * Time.deltaTime);

        lastPos = currentPos;
    }
}
