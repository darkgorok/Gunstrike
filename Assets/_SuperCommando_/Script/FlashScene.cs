using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

public class FlashScene : MonoBehaviour {

	public string sceneLoad = "scene name";
	public float delay = 2;
    [SerializeField] private GameObject loadingScreenRoot;
    private IAdsService adsService;
    private ISceneLoader sceneLoader;
    private IConsentService consentService;
    private bool isLaunchQueued;

    [Inject]
    public void Construct(IAdsService adsService, ISceneLoader sceneLoader, IConsentService consentService)
    {
        this.adsService = adsService;
        this.sceneLoader = sceneLoader;
        this.consentService = consentService;
    }

    void Awake()
    {
        ProjectScope.Inject(this);
    }

	// Use this for initialization
	void Start () {
        consentService.EnsureLaunchConsent(BeginLaunchFlow);
	}

    private void BeginLaunchFlow()
    {
        if (isLaunchQueued)
            return;

        isLaunchQueued = true;
        StartCoroutine(LoadSceneCo());
    }
	
	IEnumerator LoadSceneCo(){
        adsService.ShowRectBanner(true);
		yield return new WaitForSeconds (delay);
        sceneLoader.BeginLoad(this, sceneLoad, LoadingScreenViewResolver.Resolve(loadingScreenRoot, slider, progressText), new SceneLoadOptions
        {
            OnLoadingTick = () =>
            {
                adsService.ShowRectBanner(false);
            }
        });
    }

    [Header("LOADING PROGRESS")]
    public Slider slider;
    public Text progressText;
}
