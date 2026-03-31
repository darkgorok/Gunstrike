using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class MainMenu_Level : MonoBehaviour
{
    public int levelNumber = 0;
    public GameObject imgLock, imgOpen, imgPass;
    public Text TextLevel;

    void Start()
    {
        imgLock.SetActive(false);
        imgOpen.SetActive(false);
        imgPass.SetActive(false);

        if (GetComponent<Animator>())
            GetComponent<Animator>().enabled = levelNumber == GlobalValue.LevelHighest;
        GetComponent<Button>().interactable = levelNumber <= GlobalValue.LevelHighest;

        if (levelNumber == GlobalValue.LevelHighest)
            GlobalValue.currentHighestLevelObj = transform;

        if (levelNumber <= GlobalValue.LevelHighest)
        {
            TextLevel.gameObject.SetActive(true);
            TextLevel.text = levelNumber.ToString();

            imgOpen.SetActive(levelNumber == GlobalValue.LevelHighest);
            imgPass.SetActive(levelNumber < GlobalValue.LevelHighest);
        }
        else
        {
            TextLevel.gameObject.SetActive(false);
            imgLock.SetActive(true);
        }

    }

    public void LoadScene()
    {
        GlobalValue.levelPlaying = levelNumber;
        if (AdsManager.Instance)
            AdsManager.Instance.ShowBanner(false);
        MainMenuHomeScene.Instance.LoadScene("Level " + GlobalValue.levelPlaying);
    }
}