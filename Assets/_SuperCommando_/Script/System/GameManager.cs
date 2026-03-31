using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, Playing, Dead, Finish, Waiting };
    public GameState State { get; set; }

    public LayerMask playerLayer;
    public LayerMask enemyLayer;
    public LayerMask groundLayer;

    // Left for compatibility: some scripts/UI may still read/write this flag.
    public bool isWatchingAd { get; set; }

    [Header("CONTINUE GAME OPTION")]
    public int continueCoinCost = 100;

    // Ads removed: continue only via coins.
    public bool canBeSave()
    {
        return (GlobalValue.SavedCoins >= continueCoinCost);
    }

    public GameObject FadeInEffect;

    [Header("Floating Text")]
    public GameObject FloatingText;

    private MenuManager menuManager;

    [ReadOnly] public Vector2 currentCheckpoint = Vector2.zero;

    public bool isSpecialBullet { get; set; }

    public bool isHasKey
    {
        get { return hasKey; }
        set
        {
            if (value)
                KeyUI.Instance.Get();
            else
                KeyUI.Instance.Used();

            hasKey = value;
        }
    }
    bool hasKey = false;

    public Player Player { get; private set; }
    SoundManager soundManager;

    [HideInInspector]
    public bool isNoLives = false;

    public int MissionStarCollected { get; set; }
    public bool hideGUI = false;

    void Awake()
    {
        Application.targetFrameRate = 60;
        isSpecialBullet = false;
        Instance = this;
        State = GameState.Menu;
        Player = FindObjectOfType<Player>();

        if (CharacterHolder.Instance != null && CharacterHolder.Instance.CharacterPicked != null)
        {
            Instantiate(CharacterHolder.Instance.CharacterPicked, Player.transform.position, Player.transform.rotation);
            Destroy(Player.gameObject);

            Player = FindObjectOfType<Player>();
        }

        var startPoint = GameObject.FindGameObjectWithTag("StartPoint");
        if (startPoint)
            Player.transform.position = startPoint.transform.position;
    }

    public int Point { get; set; }
    int savePointCheckPoint;

    public int Coin { get; set; }
    int saveCoinCheckPoint;
    int checkpointBigStar;

    public void AddPoint(int addpoint)
    {
        Point += addpoint;
    }

    public void AddBullet(int addbullet)
    {
        /*Bullet*/
        if (DefaultValue.Instance && DefaultValue.Instance.defaultBulletMax)
            Debug.LogWarning("NO LIMIT BULLET");
        else
            GlobalValue.Bullets += addbullet;
    }

    public void PauseCamera(bool pause)
    {
        CameraFollow.Instance.pauseCamera = pause;
        if (pause == false)
            CameraFollow.Instance.MoveCameraToPlayerPos();
    }

    void Start()
    {
        menuManager = FindObjectOfType<MenuManager>();
        currentCheckpoint = Player.transform.position;

        soundManager = FindObjectOfType<SoundManager>();

        SoundManager.PlaySfx(SoundManager.Instance.beginSoundInMainMenu);
    }

    private void Update()
    {
        ShortKey();
    }

    void ShortKey()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(0);
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
        {
            GlobalValue.SaveLives += 999;
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
        {
            GlobalValue.SavedCoins += 999999;
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        {
            FinishGame();
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.U))
        {
            PlayerPrefs.SetInt(GlobalValue.WorldReached, int.MaxValue);
            for (int i = 1; i < 100; i++)
            {
                PlayerPrefs.SetInt(i.ToString(), 10000);        // world i, unlock 10000 levels
            }

            GlobalValue.LevelHighest = 9999;

            SceneManager.LoadSceneAsync(1);

            SoundManager.PlaySfx(soundManager.soundClick);
        }
    }

    public void SaveCheckPoint(Vector2 newCheckpoint)
    {
        currentCheckpoint = newCheckpoint;
    }

    private void ResetCheckPoint()
    {
        if (savePointCheckPoint != 0)
        {
            Point = savePointCheckPoint;
            State = GameState.Playing;
        }
        else
        {
            // Coin = GlobalValue.SavedCoins;
        }
    }

    public void ShowFloatingText(string text, Vector2 positon, Color color)
    {
        GameObject floatingText = Instantiate(FloatingText) as GameObject;
        var _position = Camera.main.WorldToScreenPoint(positon);

        floatingText.transform.SetParent(menuManager.transform, false);
        floatingText.transform.position = _position;

        var _FloatingText = floatingText.GetComponent<FloatingText>();
        _FloatingText.SetText(text, color);
    }

    public void StartGame()
    {
        State = GameState.Playing;

        var listener_ = FindObjectsOfType<MonoBehaviour>().OfType<IListener>();
        foreach (var _listener in listener_)
        {
            _listener.IPlay();
        }

        SoundManager.PlayGameMusic();
    }

    public void GameFinish(int delay = 0)
    {
        if (State == GameState.Finish)
            return;

        State = GameState.Finish;

        var listener_ = FindObjectsOfType<MonoBehaviour>().OfType<IListener>();
        foreach (var _listener in listener_)
        {
            _listener.ISuccess();
        }

        Invoke("FinishGame", delay);
    }

    void FinishGame()
    {
    

        
        SoundManager.Instance.PauseMusic(true);
        SoundManager.PlaySfx(soundManager.soundGamefinish, 0.3f);
    }

    public void UnlockLevel()
    {
        if (GlobalValue.levelPlaying == (GlobalValue.LevelHighest))
        {
            GlobalValue.LevelHighest++;
            Debug.LogWarning("Unlock new level");
        }
    }

    public void GameOver(bool forceGameOver = false)
    {
        StartCoroutine(GameOverCo(forceGameOver));
    }

    public IEnumerator GameOverCo(bool forceGameOver = false)
    {
        ControllerInput.Instance.Shot(false);

        if (State != GameState.Dead && State != GameState.Waiting)
        {
            GlobalValue.Attempt++;
        }

        if (State == GameState.Dead)
            yield break;

        // Ads removed: no interstitial attempts here.

        if (!forceGameOver && canBeSave())
        {
            if (State == GameState.Dead || State == GameState.Waiting)
                yield break;

            State = GameState.Waiting;
            Player.Kill();

            ControllerInput.Instance.StopMove();
            MenuManager.Instance.GUI.SetActive(false && !hideGUI);
            SoundManager.Instance.PauseMusic(true);

            MenuManager.Instance.OpenSaveMe(true);
        }
        else
        {
            // Ads removed: do nothing here.

            var listener_ = FindObjectsOfType<MonoBehaviour>().OfType<IListener>();
            foreach (var _listener in listener_)
            {
                _listener.IGameOver();
            }

            ControllerInput.Instance.StopMove();
            State = GameState.Dead;

            MenuManager.Instance.GameOver();
            SoundManager.PlaySfx(soundManager.soundGameover, 0.5f);
            SoundManager.Instance.PauseMusic(true);

            GlobalValue.ResetLives();
        }
    }

    public void Continue()
    {
        StartCoroutine(ContinueCo());
    }

    IEnumerator ContinueCo()
    {
        var listener_ = FindObjectsOfType<MonoBehaviour>().OfType<IListener>();
        foreach (var _listener in listener_)
        {
            _listener.IOnRespawn();
        }

        MenuManager.Instance.OpenSaveMe(false);
        Player.RespawnAt(currentCheckpoint);

        yield return new WaitForSeconds(1.5f);
        State = GameState.Playing;
        MenuManager.Instance.GUI.SetActive(true && !hideGUI);
        SoundManager.Instance.PauseMusic(false);
    }

    public void ResetLevel()
    {
        MenuManager.Instance.RestartGame();
    }
}
