using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FlashScene : MonoBehaviour {

	public string sceneLoad = "scene name";
	public float delay = 2;

	// Use this for initialization
	void Start () {
		StartCoroutine (LoadSceneCo ());
	}
	
	IEnumerator LoadSceneCo(){
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowRectBanner(true);
        }
		yield return new WaitForSeconds (delay);
		//SceneManager.LoadSceneAsync (sceneLoad);
        StartCoroutine(LoadAsynchronously(sceneLoad));
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
            if (AdsManager.Instance != null)
            {
                AdsManager.Instance.ShowRectBanner(false);
            }
            yield return null;
        }
    }
}
