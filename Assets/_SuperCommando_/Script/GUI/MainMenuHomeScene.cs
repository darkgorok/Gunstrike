using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class MainMenuHomeScene : MonoBehaviour {
	public static MainMenuHomeScene Instance;

	public GameObject StartMenu;
	public GameObject WorldsChoose;
	public GameObject LoadingScreen;
    public GameObject Settings;
	SoundManager soundManager;
    [Header("Sound and Music")]
    public Image soundImage;
    public Image musicImage;
    public Sprite soundImageOn, soundImageOff, musicImageOn, musicImageOff;

    void Awake(){
		Instance = this;
		soundManager = FindObjectOfType<SoundManager> ();
    }
    
	void Start () {
        // if (AdmobController.Instance)
        {
            //  AdmobController.Instance.LoadAllAds();
            if (AdsManager.Instance)
                //  AdsManager.Instance.ShowBanner(true);
                if (AdsManager.Instance)
                AdsManager.Instance.ShowRectBanner(false);
        }

        if (!GlobalValue.isSetDefaultValue)
        {
            GlobalValue.isSetDefaultValue = true;
            if (DefaultValue.Instance)
            {
                GlobalValue.Bullets = DefaultValue.Instance.defaultBulletMax ? int.MaxValue : DefaultValue.Instance.defaultBullet;
               GlobalValue.SaveLives = DefaultValue.Instance.defaultLives;
            }
        }

        StartMenu.SetActive(false);
        WorldsChoose.SetActive (false);
		LoadingScreen.SetActive (false);
        Settings.SetActive(false);
        SoundManager.PlayMusic(SoundManager.Instance.musicsMenu);
        if (GlobalValue.isFirstOpenMainMenu)
        {
            GlobalValue.isFirstOpenMainMenu = false;
            SoundManager.ResetMusic();
        }

        SoundManager.PlayMusic(soundManager.musicsMenu);
        StartMenu.SetActive(true);

        soundManager = FindObjectOfType<SoundManager>();

        soundImage.sprite = GlobalValue.isSound ? soundImageOn : soundImageOff;
        musicImage.sprite = GlobalValue.isMusic ? musicImageOn : musicImageOff;
        if (!GlobalValue.isSound)
            SoundManager.SoundVolume = 0;
        if (!GlobalValue.isMusic)
            SoundManager.MusicVolume = 0;

        SoundManager.PlayGameMusic();
    }

    #region Music and Sound
    public void TurnSound()
    {
        GlobalValue.isSound = !GlobalValue.isSound;
        soundImage.sprite = GlobalValue.isSound ? soundImageOn : soundImageOff;

        SoundManager.SoundVolume = GlobalValue.isSound ? 1 : 0;
    }

    public void TurnMusic()
    {
        GlobalValue.isMusic = !GlobalValue.isMusic;
        musicImage.sprite = GlobalValue.isMusic ? musicImageOn : musicImageOff;

        SoundManager.MusicVolume = GlobalValue.isMusic ? SoundManager.Instance.musicsGameVolume : 0;
    }
    #endregion

    public void TurnExitPanel(bool open)
    {
        SoundManager.Click();
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
        SoundManager.PlaySfx(soundManager.soundClick);
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
        SoundManager.Click();
        Settings.SetActive(open);
        StartMenu.SetActive(!open);
    }

	public void OpenWorldChoose(){
        StartMenu.SetActive(false);
        WorldsChoose.SetActive (true);

		SoundManager.PlaySfx (soundManager.soundClick);
    }

	public void OpenStartMenu(){
        StartMenu.SetActive(true);
        WorldsChoose.SetActive (false);

		SoundManager.PlaySfx (soundManager.soundClick);
    }

    public void LoadScene(string name)
    {
        WorldsChoose.SetActive(false);
        //SceneManager.LoadSceneAsync(name);
        LoadingScreen.SetActive(true);
        StartCoroutine(LoadAsynchronously(name));
    }

    [Header("LOADING PROGRESS")]
    public Slider slider;
    public Text progressText;
    IEnumerator LoadAsynchronously(string name)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(name);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (slider != null)
                slider.value = progress;
            if (progressText != null)
                progressText.text = (int) progress * 100f + "%";
            yield return null;
        }
    }

    public void Exit(){
		Application.Quit ();
	}
}
