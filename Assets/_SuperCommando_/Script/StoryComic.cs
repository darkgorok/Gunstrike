using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

public class StoryComic : MonoBehaviour
{
    
    public Animator anim;
    public TextTyper textTyper;
    public GameObject textObj;
    public string animNextScene = "next";
    public float delay = 1;

    public string nextSceneName = "MainMenu";
    public GameObject LoadingObj;
    private ISceneLoader sceneLoader;
    private IAudioService audioService;
    private IGameplayPresentationService presentationService;

    [Header("Audio")]
    public AudioClip backgroundMusic;
    public SceneData[] sceneDatas;

    [Inject]
    public void Construct(IAudioService audioService, IGameplayPresentationService presentationService, ISceneLoader sceneLoader)
    {
        this.audioService = audioService;
        this.presentationService = presentationService;
        this.sceneLoader = sceneLoader;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    IEnumerator Start()
    {
        audioService.PlayMusic(backgroundMusic, 0.8f);
        textObj.SetActive(false);
        LoadingObj.SetActive(false);
        int numberOfScenes = sceneDatas.Length;
        yield return new WaitForSeconds(delay);
        textObj.SetActive(true);
        for (int i = 0; i < numberOfScenes; i++)
        {
            textTyper.Reset(sceneDatas[i].message);
            anim.SetTrigger(animNextScene);
            audioService.PlaySfx(sceneDatas[i].sound);
            if (sceneDatas[i].stopBackgroundMusic)
                audioService.PauseMusic(true);

            if (sceneDatas[i].activeObj != null)
                sceneDatas[i].activeObj.SetActive(true);

            yield return new WaitForSeconds(sceneDatas[i].sceneLengthTime);
        }

        presentationService.ShowBlackScreen(2f, Color.black);
        yield return new WaitForSeconds(2f);

        LoadScene();
    }

    void LoadScene()
    {
        sceneLoader.BeginLoad(this, nextSceneName, LoadingScreenViewResolver.Resolve(LoadingObj, slider, progressText));
    }

    [Header("LOADING PROGRESS")]
    public Slider slider;
    public Text progressText;

    public void Skip()
    {
        StopAllCoroutines();
        LoadScene();
    }
}

[System.Serializable]
public class SceneData
{
    public string message;
    public float sceneLengthTime = 3;
    public bool stopBackgroundMusic = false;
    public GameObject activeObj;
    public AudioClip sound;
}
