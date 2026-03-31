using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Menu_StartScreen : MonoBehaviour {
    public Text worldTxt;

    void Start()
    {
        if (GlobalValue.levelPlaying == -1)
        {
            worldTxt.text = "TEST GAMEPLAY";
        }
        else
        {
            worldTxt.text = "LEVEL: " + GlobalValue.levelPlaying;
        }
    }
}
