using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

public class MapControllerUI : MonoBehaviour
{
    public RectTransform BlockLevel;
    public int howManyBlocks = 3;
    public float step = 720f;
    public int levelPerBlock = 10;
    public Image[] Dots;
    public AudioClip music;
    public Button btnNext, btnPre;

    private float newPosX = 0;
    private int currentPos = 0;
    private bool allowPressButton = true;
    private IProgressService progressService;
    private IAudioService audioService;

    [Inject]
    public void Construct(IProgressService progressService, IAudioService audioService)
    {
        this.progressService = progressService;
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void OnEnable()
    {
        audioService?.PlayMusic(music);
        SetCurrentWorld(Mathf.Clamp((progressService.LevelHighest / levelPerBlock) + 1, 0, howManyBlocks));
    }

    private void Start()
    {
        SetDots();
    }

    private void OnDisable()
    {
        audioService?.PlayGameMusic();
    }

    private void SetDots()
    {
        foreach (Image obj in Dots)
        {
            obj.color = new Color(1, 1, 1, 0.5f);
            obj.rectTransform.sizeDelta = new Vector2(28, 28);
        }

        Dots[currentPos].color = Color.yellow;
        Dots[currentPos].rectTransform.sizeDelta = new Vector2(38, 38);
        btnNext.interactable = currentPos < howManyBlocks - 1;
        btnPre.interactable = currentPos > 0;
    }

    public void SetCurrentWorld(int world)
    {
        currentPos = world - 1;
        newPosX = -step * (world - 1);
        newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);
        SetMapPosition();
        SetDots();
    }

    public void SetMapPosition()
    {
        BlockLevel.anchoredPosition = new Vector2(newPosX, BlockLevel.anchoredPosition.y);
    }

    public void Next()
    {
        if (allowPressButton)
            StartCoroutine(NextCo());
    }

    private IEnumerator NextCo()
    {
        allowPressButton = false;
        audioService?.PlayClick();

        if (newPosX == -step * (howManyBlocks - 1))
        {
            allowPressButton = true;
            yield break;
        }

        currentPos++;
        newPosX -= step;
        newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);

        BlackScreenSprite.instance.Show(0.1f);
        yield return new WaitForSeconds(0.1f);
        SetMapPosition();
        BlackScreenSprite.instance.Hide(0.1f);
        SetDots();
        allowPressButton = true;
    }

    public void Pre()
    {
        if (allowPressButton)
            StartCoroutine(PreCo());
    }

    private IEnumerator PreCo()
    {
        allowPressButton = false;
        audioService?.PlayClick();

        if (newPosX == 0)
        {
            allowPressButton = true;
            yield break;
        }

        currentPos--;
        newPosX += step;
        newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);

        BlackScreenSprite.instance.Show(0.1f);
        yield return new WaitForSeconds(0.1f);
        SetMapPosition();
        BlackScreenSprite.instance.Hide(0.1f);
        SetDots();
        allowPressButton = true;
    }

    public void UnlockAllLevels()
    {
        progressService.LevelHighest += 1000;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        audioService?.PlayClick();
    }
}
