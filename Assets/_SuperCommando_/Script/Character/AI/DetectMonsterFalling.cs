using UnityEngine;
using VContainer;

public class DetectMonsterFalling : MonoBehaviour
{
    public Rigidbody2D monsterIV;
    public AudioClip soundShowUp;

    private IAudioService audioService;

    [Inject]
    public void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;

        audioService?.PlaySfx(soundShowUp);
        monsterIV.isKinematic = false;
    }
}
