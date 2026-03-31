using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VContainer;
//using UnityEngine.Advertisements;

public class MenuManager : MonoBehaviour
{
    public GameObject Startmenu;
    public GameObject GUI;
    public GameObject Gameover;
    public GameObject GameFinish;
    public GameObject GamePause;
    public GameObject Controller;
    public GameObject SaveMe;
    public GameObject Loading;

    [Header("Sound and Music")]
    public Image soundImage;
    public Image musicImage;
    public Sprite soundImageOn, soundImageOff, musicImageOn, musicImageOff;

    public GameObject passLevelButton;
    private IAdsService adsService;
    private ILevelCatalogService levelCatalogService;
    private ISceneLoader sceneLoader;
    private IAudioService audioService;
    private IControllerInputService controllerInputService;
    private IGameSessionService gameSession;
    private IProgressService progressService;

    [Inject]
    public void Construct(IAdsService adsService, ILevelCatalogService levelCatalogService, ISceneLoader sceneLoader, IAudioService audioService, IControllerInputService controllerInputService, IGameSessionService gameSession, IProgressService progressService)
    {
        this.adsService = adsService;
        this.levelCatalogService = levelCatalogService;
        this.sceneLoader = sceneLoader;
        this.audioService = audioService;
        this.controllerInputService = controllerInputService;
        this.gameSession = gameSession;
        this.progressService = progressService;
    }

    void Awake()
    {
        ProjectScope.Inject(this);
    }

    // Use this for initialization
    void Start()
    {
        adsService.ShowBanner(false);
        adsService.ShowRectBanner(false);

        Startmenu.SetActive(true);
        GUI.SetActive(false);
        Gameover.SetActive(false);
        GameFinish.SetActive(false);
        GamePause.SetActive(false);
        SaveMe.SetActive(false);
        Loading.SetActive(false);
        StartCoroutine(StartGame(2));

        soundImage.sprite = progressService.IsSoundEnabled ? soundImageOn : soundImageOff;
        musicImage.sprite = progressService.IsMusicEnabled ? musicImageOn : musicImageOff;
        if (!progressService.IsSoundEnabled)
            audioService.SoundVolume = 0;
        if (!progressService.IsMusicEnabled)
            audioService.MusicVolume = 0;

    }

    #region Music and Sound
    public void TurnSound()
    {
        progressService.IsSoundEnabled = !progressService.IsSoundEnabled;
        soundImage.sprite = progressService.IsSoundEnabled ? soundImageOn : soundImageOff;
        audioService.SoundVolume = progressService.IsSoundEnabled ? 1f : 0f;
    }

    public void TurnMusic()
    {
        progressService.IsMusicEnabled = !progressService.IsMusicEnabled;
        musicImage.sprite = progressService.IsMusicEnabled ? musicImageOn : musicImageOff;
        audioService.MusicVolume = progressService.IsMusicEnabled ? audioService.GameplayMusicVolume : 0f;
    }
    #endregion

    public void NextLevel()
    {
        Time.timeScale = 1;
        audioService.PlayClick();

        gameSession.UnlockLevel();

        progressService.LevelPlaying++;

        if (progressService.LevelPlaying <= progressService.TotalLevel)
            sceneLoader.BeginLoad(this, levelCatalogService.GetLevelSceneName(progressService.LevelPlaying), LoadingScreenViewResolver.Resolve(Loading, slider, progressText));
        else
            sceneLoader.BeginLoad(this, "MainMenu", LoadingScreenViewResolver.Resolve(Loading, slider, progressText));
    }

    [Header("LOADING PROGRESS")]
    public Slider slider;
    public Text progressText;
    public void TurnController(bool turnOn)
    {
        Controller.SetActive(turnOn);
    }
    public void TurnGUI(bool turnOn)
    {
        GUI.SetActive(turnOn && !gameSession.HideGui);
    }

    public void OpenSaveMe(bool open)
    {
        if (open)
            StartCoroutine(OpenSaveMe());
        else
            SaveMe.SetActive(false);
    }

    IEnumerator OpenSaveMe()
    {
        yield return new WaitForSeconds(1);
        SaveMe.SetActive(true);
    }


    public void RestartGame()
    {
        Time.timeScale = 1;
        audioService.PlayClick();
        sceneLoader.BeginReloadCurrent(this, LoadingScreenViewResolver.Resolve(Loading, slider, progressText));
    }

    public void HomeScene()
    {
        audioService.PlayClick();
        Time.timeScale = 1;
        sceneLoader.BeginLoad(this, "MainMenu", LoadingScreenViewResolver.Resolve(Loading, slider, progressText));

    }

    public void Gamefinish()
    {
        StartCoroutine(GamefinishCo(2));
    }

    public void GameOver()
    {
        StartCoroutine(GameOverCo(2));
    }

    public void Pause()
    {
        audioService.PlayClick();
        if (Time.timeScale == 0)
        {
            adsService.ShowBanner(false);
            GamePause.SetActive(false);
            GUI.SetActive(!gameSession.HideGui);
            Time.timeScale = 1;
            audioService.PauseMusic(false);
        }
        else
        {
            adsService.ShowBanner(true);
            GamePause.SetActive(true);
            GUI.SetActive(false);
            Time.timeScale = 0;
            audioService.PauseMusic(true);
        }

        controllerInputService.StopMove();
    }

    public enum WatchVideoType { Checkpoint, Restart, Next }
    public WatchVideoType watchVideoType;

    IEnumerator StartGame(float time)
    {
        yield return new WaitForSeconds(time - 0.5f);
        Startmenu.GetComponent<Animator>().SetTrigger("play");

        yield return new WaitForSeconds(0.5f);
        Startmenu.SetActive(false);
        GUI.SetActive(!gameSession.HideGui);

        gameSession.StartGame();
    }

    IEnumerator GamefinishCo(float time)
    {
        GUI.SetActive(false);
        yield return new WaitForSeconds(time);
        adsService.TryShowInterstitial(GameManager.GameState.Finish);
        yield return new WaitForSeconds(0.2f);
        adsService.ShowBanner(true);
        GameFinish.SetActive(true);
        audioService.MusicVolume = 1f;
        audioService.PlayMusic(audioService.FinishPanelMusic, false);
    }

    IEnumerator GameOverCo(float time)
    {
        GUI.SetActive(false);

        yield return new WaitForSeconds(time);

        //show ads
       
        Gameover.SetActive(true);
        adsService.ShowBanner(true);
    }

    public void ForceFinishLevel()
    {
        gameSession.GameFinish();
    }
}
