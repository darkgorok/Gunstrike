using UnityEngine;
using VContainer;

public class PlaySoundWhenPlayerInRange : MonoBehaviour
{
    public float volume = 0.6f;
    public AudioClip movingSound;
    [Tooltip("Allow play sound when the distance with Player smaller this value")]
    public float playDistancePlayer = 6;

    private AudioSource soundScr;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IGameSessionService gameSession)
    {
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        soundScr = gameObject.AddComponent<AudioSource>();
        soundScr.clip = movingSound;
        soundScr.pitch = 1;
        soundScr.Play();
        soundScr.loop = true;
        soundScr.volume = 0;
    }

    private void Update()
    {
        if (gameSession?.Player == null)
            return;

        soundScr.volume = Vector3.Distance(gameSession.Player.transform.position, transform.position) < playDistancePlayer
            ? volume
            : 0f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, playDistancePlayer);
    }
}
