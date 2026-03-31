using UnityEngine;
using System.Collections;
using VContainer;

[RequireComponent(typeof(Collider2D))]
public class GameFinishFlag : MonoBehaviour
{
    public AudioClip sound;
    [SerializeField] private Animator cachedAnimator;

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
        if (cachedAnimator == null)
            TryGetComponent(out cachedAnimator);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Player _))
            return;

        if (gameSession.State == GameManager.GameState.Finish)
            return;

        gameSession.GameFinish();
        if (cachedAnimator != null)
            cachedAnimator.SetBool("finish", true);

        audioService.PlaySfx(sound, 0.5f);
        Destroy(this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (cachedAnimator == null)
            TryGetComponent(out cachedAnimator);
    }
#endif
}
