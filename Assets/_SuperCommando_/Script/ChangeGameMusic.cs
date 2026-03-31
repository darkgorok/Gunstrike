using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeGameMusic : MonoBehaviour {
    public AudioClip gameMusic;

    bool isWorked = false;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isWorked)
            return;

        if (other.gameObject.GetComponent<Player>() == null)
            return;

        SoundManager.PlayMusic(gameMusic);

        isWorked = true;
    }
}
