using UnityEngine;
using VContainer;

public class Spring : MonoBehaviour, IStandOnEvent
{
    public float pushUp;
    public AudioClip soundEffect;
    [Tooltip("Push player if his position Y > this position Y + this offset Y")]
    public float centerOffset = 0.2f;
    [Range(0, 1)]
    public float soundEffectVolume = 0.5f;

    private Animator anim;
    private IGameSessionService gameSession;
    private IAudioService audioService;

    [Inject]
    public void Construct(IGameSessionService gameSession, IAudioService audioService)
    {
        this.gameSession = gameSession;
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Push()
    {
        if (gameSession?.Player == null)
            return;

        gameSession.Player.SetForce(new Vector2(gameSession.Player.velocity.x, pushUp), true);

        if (anim != null)
            anim.SetTrigger("push");

        audioService?.PlaySfx(soundEffect, soundEffectVolume);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + centerOffset, transform.position.z), 0.1f);
    }

    public void StandOnEvent(GameObject instigator)
    {
        Push();
    }
}
