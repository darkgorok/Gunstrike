using UnityEngine;
using System.Collections;
using VContainer;

public class GameFinishFlag : MonoBehaviour
{
    public AudioClip sound;
    private IAudioService audioService;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IAudioService audioService, IGameSessionService gameSession)
    {
        this.audioService = audioService;
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Player>() == null)
            return;

        if (gameSession.State == GameManager.GameState.Finish)
            return;

        gameSession.GameFinish();
        if (GetComponent<Animator>() != null)
            GetComponent<Animator>().SetBool("finish", true);
        audioService.PlaySfx(sound, 0.5f);
        Destroy(this);
    }
}
