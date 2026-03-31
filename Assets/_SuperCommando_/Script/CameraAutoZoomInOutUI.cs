using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAutoZoomInOutUI : MonoBehaviour
{
    public float speed = 1;
    public float minSize = 4;
    public float maxSize = 5;
    Camera camera;

    // Update is called once per frame
    IEnumerator Start()
    {
        camera = GetComponent<Camera>();

        float percent = 0;
        while (true)
        {
            percent = 0;
            while (percent < 1)
            {
                percent += Time.deltaTime * speed;
                percent = Mathf.Clamp01(percent);
                camera.orthographicSize = Mathf.Lerp(minSize, maxSize, percent);
                yield return null;
            }

            percent = 0;
            while (percent < 1)
            {
                percent += Time.deltaTime * speed;
                percent = Mathf.Clamp01(percent);
                camera.orthographicSize = Mathf.Lerp(maxSize, minSize, percent);
                yield return null;
            }
        }
    }
}
