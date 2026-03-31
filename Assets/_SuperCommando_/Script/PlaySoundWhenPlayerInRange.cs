using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundWhenPlayerInRange : MonoBehaviour
{
    public float volume = 0.6f;
    public AudioClip movingSound;
    [Tooltip("Allow play sound when the distance with Player smaller this value")]
    public float playDistancePlayer = 6;
    AudioSource soundScr;

    void Start()
    {
        soundScr = gameObject.AddComponent<AudioSource>();
        soundScr.clip = movingSound;
        soundScr.pitch = 1;
        soundScr.Play();
        soundScr.loop = true;
        soundScr.volume = 0;
        
        InvokeRepeating("CheckingPlayer", 0, 0.1f);
    }

    void CheckingPlayer()
    {
        if (Vector3.Distance(GameManager.Instance.Player.transform.position, transform.position) < playDistancePlayer)
        {
            soundScr.volume = volume;
        }
        else
            soundScr.volume = 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, playDistancePlayer);
    }
}
