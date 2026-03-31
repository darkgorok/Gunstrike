using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearTrap : MonoBehaviour
{
    public GameObject spearObj;
    public int numberOfSpear = 5;
    public float widthOfSpear = 0.3f;
    public bool arrangeToRight = false;
    public Vector2 offset;
    public Vector3[] localWaypoints;

    List<GameObject> spearList;

    [Header("Spear Setup")]
    public float speed = 10;
    public float delayWarning = 0.5f;
    public float delayPerSpear = 0.1f;
    public AudioClip sound;

    [Header("CheckPlayer")]
    public Vector2 checkSize = new Vector2(0.5f, 2);
    public Vector3 checkOffset;
    
    void Start()
    {
        SetupSpear();
        InvokeRepeating("CheckPlayerInvoke", 0, 0.1f);
    }

    void SetupSpear()
    {
        spearList = new List<GameObject>();
        for (int i = 0; i < numberOfSpear; i++)
        {
            spearList.Add(Instantiate(spearObj, transform));
            spearList[i].transform.position = (Vector2)transform.position + (arrangeToRight ? Vector2.right : Vector2.left) * widthOfSpear * i + offset + (i == 0 ? Vector2.up * 0.5f : Vector2.zero);
        }
    }

    void CheckPlayerInvoke()
    {
        if( Physics2D.BoxCast(transform.position + checkOffset, checkSize, 0, Vector2.zero, 0, GameManager.Instance.playerLayer))
        {
            StartCoroutine(WorkingCo());
            CancelInvoke();
        }

    }

    IEnumerator WorkingCo()
    {
        for (int i = 0; i < numberOfSpear; i++)
        {
            SoundManager.PlaySfx(sound);
            if (i == 0)
            {
                spearList[0].AddComponent<SimplePathedMoving>().Init(0, speed, localWaypoints, false);
                
                yield return new WaitForSeconds(delayWarning);
            }
            else
            {
                spearList[i].AddComponent<SimplePathedMoving>().Init(0, speed, localWaypoints, false);
                yield return new WaitForSeconds(delayPerSpear);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying || numberOfSpear == 0)
            return;

        for (int i = 0; i < numberOfSpear; i++)
        {
            Gizmos.DrawSphere((Vector2)transform.position + (arrangeToRight ? Vector2.right : Vector2.left) * widthOfSpear * i + offset + (i == 0 ? Vector2.up * 0.5f : Vector2.zero), 0.1f);
        }

        if (localWaypoints.Length > 1)
        {
            Gizmos.DrawWireSphere(localWaypoints[1] + transform.position, 0.1f);
            Gizmos.DrawLine(localWaypoints[1] + transform.position, localWaypoints[0] + transform.position);
        }

        Gizmos.DrawWireCube(transform.position + checkOffset, checkSize);
    }
}
