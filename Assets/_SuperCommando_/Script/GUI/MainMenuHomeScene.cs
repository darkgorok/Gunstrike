using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using VContainer;

public class MainMenuHomeScene : MonoBehaviour
{
    public GameObject StartMenu;
    public GameObject WorldsChoose;
    public GameObject LoadingScreen;
    public GameObject Settings;

    private IAdsService adsService;
    private ISceneLoader sceneLoader;
    private IAudioService audioService;
    private IProgressService progressService;
    private IDefaultGameConfigService defaultGameConfigService;

    [Header("Sound and Music")]
    public Image soundImage;
    public Image musicImage;
    public Sprite soundImageOn, soundImageOff, musicImageOn, musicImageOff;

    [Inject]
    public void Construct(
        IAdsService adsService,
        ISceneLoader sceneLoader,
        IAudioService audioService,
        IProgressService progressService,
        IDefaultGameConfigService defaultGameConfigService)
    {
        this.adsService = adsService;
        this.sceneLoader = sceneLoader;
        this.audioService = audioService;
        this.progressService = progressService;
        this.defaultGameConfigService = defaultGameConfigService;
    }

    void Awake()
    {
        ProjectScope.Inject(this);
    }

    void Start()
    {
        adsService.ShowRectBanner(false);

        if (!progressService.IsSetDefaultValue)
        {
            progressService.IsSetDefaultValue = true;
            if (defaultGameConfigService.HasDefaults)
            {
                progressService.Bullets = defaultGameConfigService.DefaultBulletMax ? int.MaxValue : defaultGameConfigService.DefaultBullet;
                progressService.SaveLives = defaultGameConfigService.DefaultLives;
            }
        }

        StartMenu.SetActive(false);
        WorldsChoose.SetActive(false);
        LoadingScreen.SetActive(false);
        Settings.SetActive(false);

        audioService.PlayMenuMusic();
        if (progressService.IsFirstOpenMainMenu)
        {
            progressService.IsFirstOpenMainMenu = false;
            audioService.ResetMusic();
        }

        audioService.PlayMusic(audioService.MenuMusic);
        StartMenu.SetActive(true);

        soundImage.sprite = progressService.IsSoundEnabled ? soundImageOn : soundImageOff;
        musicImage.sprite = progressService.IsMusicEnabled ? musicImageOn : musicImageOff;
        if (!progressService.IsSoundEnabled)
            audioService.SoundVolume = 0;
        if (!progressService.IsMusicEnabled)
            audioService.MusicVolume = 0;
    }

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

    public void TurnExitPanel(bool open)
    {
        audioService.PlayClick();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenFacebook()
    {
#if !UNITY_WEBGL
        GameMode.Instance.OpenFacebook();
#else
        openPage(facebookLink);
#endif
        audioService.PlaySfx(audioService.ClickClip);
    }

    public void RemoveAds()
    {
#if UNITY_PURCHASING
        if (Purchaser.Instance)
            Purchaser.Instance.BuyRemoveAds();
#endif
    }

    public void OpenSettings(bool open)
    {
        audioService.PlayClick();
        Settings.SetActive(open);
        StartMenu.SetActive(!open);
    }

    public void OpenWorldChoose()
    {
        StartMenu.SetActive(false);
        WorldsChoose.SetActive(true);
        audioService.PlaySfx(audioService.ClickClip);
    }

    public void OpenStartMenu()
    {
        StartMenu.SetActive(true);
        WorldsChoose.SetActive(false);
        audioService.PlaySfx(audioService.ClickClip);
    }

    public void LoadScene(string name)
    {
        WorldsChoose.SetActive(false);
        sceneLoader.BeginLoad(this, name, LoadingScreenViewResolver.Resolve(LoadingScreen, slider, progressText));
    }

    [Header("LOADING PROGRESS")]
    public Slider slider;
    public Text progressText;

    public void Exit()
    {
        Application.Quit();
    }
}
