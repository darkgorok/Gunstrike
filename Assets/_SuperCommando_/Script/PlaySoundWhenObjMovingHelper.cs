using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundWhenObjMovingHelper : MonoBehaviour
{
    public float volume = 0.6f;
    public AudioClip movingSound;
    [Tooltip("Allow play sound when the distance with Player smaller this value")]
    public float playDistancePlayer = 6;
    AudioSource soundScr;
    Vector3 lastPos;

    void Start()
    {
        soundScr = gameObject.AddComponent<AudioSource>();
        soundScr.clip = movingSound;
        soundScr.pitch = 1;
        soundScr.Play();
        soundScr.loop = true;
        soundScr.volume = 0;

        lastPos = transform.position;
        InvokeRepeating("CheckingMoving", 0, 0.1f);
    }

    void CheckingMoving()
    {
        if (Vector3.Distance(lastPos, transform.position) != 0 && Vector2.Distance(transform.position, GameManager.Instance.Player.transform.position) < playDistancePlayer)
        {
            soundScr.volume = volume;
        }else
            soundScr.volume = 0;

        lastPos = transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, playDistancePlayer);
    }
}
