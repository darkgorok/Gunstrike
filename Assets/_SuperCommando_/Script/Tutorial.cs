using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance;

    public Image ImageTut;
    public GameObject Panel;
    public AudioClip sound;

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
        Instance = this;
        Panel.SetActive(false);
    }

    public void Open(Sprite image)
    {
        if (gameSession?.Player != null)
            gameSession.Player.velocity.x = 0;

        audioService?.PlaySfx(sound);
        ImageTut.sprite = image;
        Panel.SetActive(true);
        Time.timeScale = 0;
    }

    public void Close()
    {
        Panel.SetActive(false);
        Time.timeScale = 1;
    }
}
