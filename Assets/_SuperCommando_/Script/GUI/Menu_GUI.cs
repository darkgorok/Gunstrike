using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class Menu_GUI : MonoBehaviour
{
    public static Menu_GUI Instance;
    public Text bulletText;
    public Text grenadeTxt;
    public Text liveTxt;

    private bool firstPlay = true;
    private IGameSessionService gameSession;
    private IProgressService progressService;

    [Inject]
    public void Construct(IGameSessionService gameSession, IProgressService progressService)
    {
        this.gameSession = gameSession;
        this.progressService = progressService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        Instance = this;
    }

    private void Start()
    {
        firstPlay = false;
    }

    private void OnEnable()
    {
        if (firstPlay)
            return;
    }

    private void Update()
    {
        bulletText.text = gameSession.Player.gunTypeID.unlimitedBullet ? "-/-" : gameSession.Player.gunTypeID.bullet.ToString();
        grenadeTxt.text = gameSession.Player.grenadeRemaining.ToString();
        liveTxt.text = progressService.SaveLives.ToString();
    }
}
