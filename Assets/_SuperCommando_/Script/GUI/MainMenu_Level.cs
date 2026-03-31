using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MainMenu_Level : MonoBehaviour
{
    public int levelNumber = 0;
    public GameObject imgLock, imgOpen, imgPass;
    public Text TextLevel;

    [SerializeField] private Button cachedButton;
    [SerializeField] private Animator cachedAnimator;

    private ILevelCatalogService levelCatalogService;
    private IAdsService adsService;
    private IProgressService progressService;
    private ILevelSelectionState levelSelectionState;
    private IMainMenuSceneService mainMenuSceneService;

    [Inject]
    public void Construct(IAdsService adsService, ILevelCatalogService levelCatalogService, IProgressService progressService, ILevelSelectionState levelSelectionState, IMainMenuSceneService mainMenuSceneService)
    {
        this.adsService = adsService;
        this.levelCatalogService = levelCatalogService;
        this.progressService = progressService;
        this.levelSelectionState = levelSelectionState;
        this.mainMenuSceneService = mainMenuSceneService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        if (cachedButton == null)
            cachedButton = GetComponent<Button>();
        if (cachedAnimator == null)
            TryGetComponent(out cachedAnimator);
    }

    private void Start()
    {
        imgLock.SetActive(false);
        imgOpen.SetActive(false);
        imgPass.SetActive(false);

        if (cachedAnimator != null)
            cachedAnimator.enabled = levelNumber == progressService.LevelHighest;

        cachedButton.interactable = levelNumber <= progressService.LevelHighest;

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
        mainMenuSceneService.LoadScene(levelCatalogService.GetLevelSceneName(progressService.LevelPlaying));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (cachedButton == null)
            TryGetComponent(out cachedButton);
        if (cachedAnimator == null)
            TryGetComponent(out cachedAnimator);
    }
#endif
}
