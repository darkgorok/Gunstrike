using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MainMenu_Level : MonoBehaviour
{
    public int levelNumber = 0;
    public GameObject imgLock, imgOpen, imgPass;
    public Text TextLevel;

    private ILevelCatalogService levelCatalogService;
    private IAdsService adsService;
    private IProgressService progressService;
    private ILevelSelectionState levelSelectionState;

    [Inject]
    public void Construct(IAdsService adsService, ILevelCatalogService levelCatalogService, IProgressService progressService, ILevelSelectionState levelSelectionState)
    {
        this.adsService = adsService;
        this.levelCatalogService = levelCatalogService;
        this.progressService = progressService;
        this.levelSelectionState = levelSelectionState;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        imgLock.SetActive(false);
        imgOpen.SetActive(false);
        imgPass.SetActive(false);

        if (GetComponent<Animator>())
            GetComponent<Animator>().enabled = levelNumber == progressService.LevelHighest;

        GetComponent<Button>().interactable = levelNumber <= progressService.LevelHighest;

        if (levelNumber == progressService.LevelHighest)
            levelSelectionState.CurrentHighestLevelTransform = transform;

        if (levelNumber <= progressService.LevelHighest)
        {
            TextLevel.gameObject.SetActive(true);
            TextLevel.text = levelNumber.ToString();

            imgOpen.SetActive(levelNumber == progressService.LevelHighest);
            imgPass.SetActive(levelNumber < progressService.LevelHighest);
        }
        else
        {
            TextLevel.gameObject.SetActive(false);
            imgLock.SetActive(true);
        }
    }

    public void LoadScene()
    {
        progressService.LevelPlaying = levelNumber;
        adsService.ShowBanner(false);

        MainMenuHomeScene.Instance.LoadScene(levelCatalogService.GetLevelSceneName(progressService.LevelPlaying));
    }
}
