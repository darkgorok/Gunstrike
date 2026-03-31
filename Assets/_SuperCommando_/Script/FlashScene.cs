using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

public class FlashScene : MonoBehaviour {

	public string sceneLoad = "scene name";
	public float delay = 2;
    private IAdsService adsService;
    private ISceneLoader sceneLoader;

    [Inject]
    public void Construct(IAdsService adsService, ISceneLoader sceneLoader)
    {
        this.adsService = adsService;
        this.sceneLoader = sceneLoader;
    }

    void Awake()
    {
        ProjectScope.Inject(this);
    }

	// Use this for initialization
	void Start () {
		StartCoroutine (LoadSceneCo ());
	}
	
	IEnumerator LoadSceneCo(){
        adsService.ShowRectBanner(true);
		yield return new WaitForSeconds (delay);
		//SceneManager.LoadSceneAsync (sceneLoad);
        sceneLoader.BeginLoad(this, sceneLoad, LoadingScreenViewResolver.Resolve(gameObject, slider, progressText), new SceneLoadOptions
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
