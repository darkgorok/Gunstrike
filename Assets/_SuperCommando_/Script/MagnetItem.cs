using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetItem : MonoBehaviour {
    public float timeUse = 10;
    public GameObject Effect;
    public AudioClip sound;
    [Range(0, 1)]
    public float soundVolume = 0.5f;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Player>() == null)
            return;

        SoundManager.PlaySfx(sound, soundVolume);

        if (Effect != null)
            Instantiate(Effect, transform.position, transform.rotation);

        if (Magnet.Instance)
            Magnet.Instance.ActiveMagnet(timeUse);

        Destroy(gameObject);
    }
}
