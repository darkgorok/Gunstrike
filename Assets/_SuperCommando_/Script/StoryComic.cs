using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryComic : MonoBehaviour
{
    
    public Animator anim;
    public TextTyper textTyper;
    public GameObject textObj;
    public string animNextScene = "next";
    public float delay = 1;

    public string nextSceneName = "MainMenu";
    public GameObject LoadingObj;

    [Header("Audio")]
    public AudioClip backgroundMusic;
    public SceneData[] sceneDatas;
    IEnumerator Start()
    {
        SoundManager.PlayMusic(backgroundMusic, 0.8f);
        textObj.SetActive(false);
        LoadingObj.SetActive(false);
        int numberOfScenes = sceneDatas.Length;
        yield return new WaitForSeconds(delay);
        textObj.SetActive(true);
        for (int i = 0; i < numberOfScenes; i++)
        {
            textTyper.Reset(sceneDatas[i].message);
            anim.SetTrigger(animNextScene);
            SoundManager.PlaySfx(sceneDatas[i].sound);
            if (sceneDatas[i].stopBackgroundMusic)
                SoundManager.Instance.PauseMusic(true);

            if (sceneDatas[i].activeObj != null)
                sceneDatas[i].activeObj.SetActive(true);

            yield return new WaitForSeconds(sceneDatas[i].sceneLengthTime);
        }

        if (BlackScreenUI.instance)
        {
            BlackScreenUI.instance.Show(2);
            yield return new WaitForSeconds(2);
        }

        LoadScene();
    }

    void LoadScene()
    {
        LoadingObj.SetActive(true);
        StartCoroutine(LoadAsynchronously(nextSceneName));
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
