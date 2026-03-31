using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using UnityEngine.Advertisements;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

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

    void Awake()
    {
        Instance = this;
       

    }

    // Use this for initialization
    void Start()
    {
        if (AdsManager.Instance)
        {
            AdsManager.Instance.ShowBanner(false);
            AdsManager.Instance.ShowRectBanner(false);

        }

        Startmenu.SetActive(true);
        GUI.SetActive(false);
        Gameover.SetActive(false);
        GameFinish.SetActive(false);
        GamePause.SetActive(false);
        SaveMe.SetActive(false);
        Loading.SetActive(false);
        StartCoroutine(StartGame(2));

        soundImage.sprite = GlobalValue.isSound ? soundImageOn : soundImageOff;
        musicImage.sprite = GlobalValue.isMusic ? musicImageOn : musicImageOff;
        if (!GlobalValue.isSound)
            SoundManager.SoundVolume = 0;
        if (!GlobalValue.isMusic)
            SoundManager.MusicVolume = 0;

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

    public void NextLevel()
    {
        Time.timeScale = 1;
        SoundManager.PlaySfx(SoundManager.Instance.soundClick);

        GameManager.Instance.UnlockLevel();

        GlobalValue.levelPlaying++;

        Loading.SetActive(true);
        if (GlobalValue.levelPlaying <= GlobalValue.totalLevel)
            StartCoroutine(LoadAsynchronously("Level " + GlobalValue.levelPlaying));
        else
            StartCoroutine(LoadAsynchronously("MainMenu"));
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
                progressText.text = (int)progress * 100f + "%";
            //			Debug.LogError (progress);
            yield return null;
        }
    }

    public void TurnController(bool turnOn)
    {
        Controller.SetActive(turnOn);
    }
    public void TurnGUI(bool turnOn)
    {
        GUI.SetActive(turnOn && !GameManager.Instance.hideGUI);
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
        SoundManager.PlaySfx(SoundManager.Instance.soundClick);
        //if (!GlobalValue.RemoveAds && DefaultValue.Instance && DefaultValue.Instance.restartLevelAd && Advertisement.IsReady())
        //{
        //    watchVideoType = WatchVideoType.Restart;
        //    var options = new ShowOptions { resultCallback = HandleShowResult };
        //    if (!Advertisement.isShowing)
        //        Advertisement.Show(options);
        //}
        //else
        //SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        Loading.SetActive(true);
        StartCoroutine(LoadAsynchronously(SceneManager.GetActiveScene().name));
    }

    public void HomeScene()
    {
        //if (GameManager.Instance.State != GameManager.GameState.Finish)
        //{
        //    if (LevelMapType.Instance && !LevelMapType.Instance.playerNoLimitLife)
        //        GlobalValue.SaveLives -= 1;
        //}
        SoundManager.PlaySfx(SoundManager.Instance.soundClick);
        Time.timeScale = 1;
        //SceneManager.LoadSceneAsync("MainMenu");
        Loading.SetActive(true);
        StartCoroutine(LoadAsynchronously("MainMenu"));

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
        SoundManager.PlaySfx(SoundManager.Instance.soundClick);
        if (Time.timeScale == 0)
        {
            if(AdsManager.Instance)
            AdsManager.Instance.ShowBanner(false);
            GamePause.SetActive(false);
            GUI.SetActive(true && !GameManager.Instance.hideGUI);
            Time.timeScale = 1;
            SoundManager.Instance.PauseMusic(false);
        }
        else
        {
            if(AdsManager.Instance)
            AdsManager.Instance.ShowBanner(true);
            GamePause.SetActive(true);
            GUI.SetActive(false);
            Time.timeScale = 0;
            SoundManager.Instance.PauseMusic(true);
        }

        ControllerInput.Instance.StopMove();
    }

    public enum WatchVideoType { Checkpoint, Restart, Next }
    public WatchVideoType watchVideoType;

    IEnumerator StartGame(float time)
    {
        yield return new WaitForSeconds(time - 0.5f);
        Startmenu.GetComponent<Animator>().SetTrigger("play");

        yield return new WaitForSeconds(0.5f);
        Startmenu.SetActive(false);
        GUI.SetActive(true && !GameManager.Instance.hideGUI);

        GameManager.Instance.StartGame();
    }

    IEnumerator GamefinishCo(float time)
    {
        GUI.SetActive(false);
        yield return new WaitForSeconds(time);
        if (AdsManager.Instance)
            AdsManager.Instance.TryShowInterstitial(GameManager.GameState.Finish);
        yield return new WaitForSeconds(0.2f);
        if (AdsManager.Instance)
            AdsManager.Instance.ShowBanner(true);
        GameFinish.SetActive(true);
        SoundManager.MusicVolume = 1;
        SoundManager.PlayMusic(SoundManager.Instance.musicFinishPanel, false);
    }

    IEnumerator GameOverCo(float time)
    {
        GUI.SetActive(false);

        yield return new WaitForSeconds(time);

        //show ads
       
        Gameover.SetActive(true);
        if(AdsManager.Instance)
        AdsManager.Instance.ShowBanner(true);
    }

    public void ForceFinishLevel()
    {
        GameManager.Instance.GameFinish();
    }
}
