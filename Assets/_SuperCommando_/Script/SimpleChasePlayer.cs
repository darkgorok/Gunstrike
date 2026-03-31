using UnityEngine;
using VContainer;

public class SimpleChasePlayer : MonoBehaviour
{
    public float speed = 10;
    public AudioClip soundShowUp;
    public SpriteRenderer image;
    public GameObject endPoint;
    public AudioClip sound;

    private Vector3 originalPos;
    private Vector3 endPos;
    private AudioSource engineAudio;
    private bool isMovingBack;
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
        originalPos = transform.position;
        if (endPoint)
            endPos = endPoint.transform.position;

        engineAudio = gameObject.AddComponent<AudioSource>();
        engineAudio.clip = sound;
        engineAudio.Play();
        engineAudio.loop = true;
        engineAudio.volume = 0;
    }

    private void OnEnable()
    {
        if (gameSession == null || gameSession.State != GameManager.GameState.Playing)
            return;

        StartCoroutine(MMFade.FadeSprite(image, 0.2f, new Color(1f, 1f, 1f, 1f)));
        audioService?.PlaySfx(soundShowUp);
    }

    private void LateUpdate()
    {
        if (gameSession == null || gameSession.State != GameManager.GameState.Playing)
        {
            engineAudio.volume = 0;
            return;
        }

        transform.Translate(speed * Time.deltaTime, 0, 0, Space.Self);
        engineAudio.volume = 0.8f;

        if (!isMovingBack)
        {
            if (endPoint && Mathf.Abs(transform.position.x - endPos.x) < 0.5f)
            {
                speed = -speed;
                engineAudio.Stop();
                isMovingBack = true;
            }

            return;
        }

        if (Mathf.Abs(transform.position.x - originalPos.x) < 0.5f)
            gameObject.SetActive(false);
    }

    public void IOnRespawn()
    {
        transform.position = originalPos;
        gameObject.SetActive(false);
        engineAudio.Play();
    }

    private void OnDrawGizmos()
    {
        if (endPoint)
            Gizmos.DrawLine(transform.position, endPoint.transform.position);
    }
}
