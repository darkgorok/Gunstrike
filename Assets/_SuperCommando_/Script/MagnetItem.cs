using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class MagnetItem : MonoBehaviour
{
    public float timeUse = 10;
    public GameObject Effect;
    public AudioClip sound;

    [Range(0, 1)]
    public float soundVolume = 0.5f;

    private IAudioService audioService;

    [Inject]
    private void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Player>() == null)
            return;

        audioService?.PlaySfx(sound, soundVolume);

        if (Effect != null)
            Instantiate(Effect, transform.position, transform.rotation);

        if (Magnet.Instance)
            Magnet.Instance.ActiveMagnet(timeUse);

        Destroy(gameObject);
    }
}
