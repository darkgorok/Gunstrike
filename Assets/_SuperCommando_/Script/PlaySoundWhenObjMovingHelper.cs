using UnityEngine;
using VContainer;

public class PlaySoundWhenObjMovingHelper : MonoBehaviour
{
    public float volume = 0.6f;
    public AudioClip movingSound;
    [Tooltip("Allow play sound when the distance with Player smaller this value")]
    public float playDistancePlayer = 6;

    private AudioSource soundScr;
    private Vector3 lastPos;
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
        lastPos = transform.position;
    }

    private void Update()
    {
        if (gameSession?.Player == null)
            return;

        bool isMoving = Vector3.Distance(lastPos, transform.position) > 0f;
        bool isPlayerInRange = Vector2.Distance(transform.position, gameSession.Player.transform.position) < playDistancePlayer;
        soundScr.volume = isMoving && isPlayerInRange ? volume : 0f;
        lastPos = transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, playDistancePlayer);
    }
}
