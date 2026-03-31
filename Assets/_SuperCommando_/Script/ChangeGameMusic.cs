using UnityEngine;
using VContainer;

public class ChangeGameMusic : MonoBehaviour
{
    public AudioClip gameMusic;

    private bool isWorked;
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
        if (isWorked || other.gameObject.GetComponent<Player>() == null)
            return;

        audioService?.PlayMusic(gameMusic);
        isWorked = true;
    }
}
