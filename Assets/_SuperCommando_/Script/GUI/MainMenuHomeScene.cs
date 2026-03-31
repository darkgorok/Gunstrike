using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using VContainer;

public class MainMenuHomeScene : MonoBehaviour {
	public static MainMenuHomeScene Instance;

	public GameObject StartMenu;
	public GameObject WorldsChoose;
    public GameObject LoadingScreen;
    public GameObject Settings;
	SoundManager soundManager;
    private IAdsService adsService;
    private ISceneLoader sceneLoader;
    private IAudioService audioService;
    private IProgressService progressService;
    [Header("Sound and Music")]
    public Image soundImage;
    public Image musicImage;
    public Sprite soundImageOn, soundImageOff, musicImageOn, musicImageOff;

    [Inject]
    public void Construct(IAdsService adsService, ISceneLoader sceneLoader, IAudioService audioService, IProgressService progressService)
    {
        this.adsService = adsService;
        this.sceneLoader = sceneLoader;
        this.audioService = audioService;
        this.progressService = progressService;
    }

    void Awake(){
        ProjectScope.Inject(this);
		Instance = this;
		soundManager = FindObjectOfType<SoundManager> ();
    }
    
	void Start () {
        // if (AdmobController.Instance)
        {
            //  AdmobController.Instance.LoadAllAds();
            adsService.ShowRectBanner(false);
        }

        if (!progressService.IsSetDefaultValue)
        {
            progressService.IsSetDefaultValue = true;
            if (DefaultValue.Instance)
            {
                progressService.Bullets = DefaultValue.Instance.defaultBulletMax ? int.MaxValue : DefaultValue.Instance.defaultBullet;
                progressService.SaveLives = DefaultValue.Instance.defaultLives;
            }
        }

        StartMenu.SetActive(false);
        WorldsChoose.SetActive (false);
		LoadingScreen.SetActive (false);
        Settings.SetActive(false);
        audioService.PlayMenuMusic();
        if (progressService.IsFirstOpenMainMenu)
        {
            progressService.IsFirstOpenMainMenu = false;
            audioService.ResetMusic();
        }

        audioService.PlayMusic(audioService.MenuMusic);
        StartMenu.SetActive(true);

        soundManager = FindObjectOfType<SoundManager>();

        soundImage.sprite = progressService.IsSoundEnabled ? soundImageOn : soundImageOff;
        musicImage.sprite = progressService.IsMusicEnabled ? musicImageOn : musicImageOff;
        if (!progressService.IsSoundEnabled)
            audioService.SoundVolume = 0;
        if (!progressService.IsMusicEnabled)
            audioService.MusicVolume = 0;

        audioService.PlayGameMusic();
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
        {
            Purchaser.Instance.BuyRemoveAds();
        }
#endif
    }

    public void OpenSettings(bool open)
    {
        audioService.PlayClick();
        Settings.SetActive(open);
        StartMenu.SetActive(!open);
    }

	public void OpenWorldChoose(){
        StartMenu.SetActive(false);
        WorldsChoose.SetActive (true);

        audioService.PlaySfx(audioService.ClickClip);
    }

	public void OpenStartMenu(){
        StartMenu.SetActive(true);
        WorldsChoose.SetActive (false);

        audioService.PlaySfx(audioService.ClickClip);
    }

    public void LoadScene(string name)
    {
        WorldsChoose.SetActive(false);
        //SceneManager.LoadSceneAsync(name);
        sceneLoader.BeginLoad(this, name, LoadingScreenViewResolver.Resolve(LoadingScreen, slider, progressText));
    }

    [Header("LOADING PROGRESS")]
    public Slider slider;
    public Text progressText;

    public void Exit(){
		Application.Quit ();
	}
}
