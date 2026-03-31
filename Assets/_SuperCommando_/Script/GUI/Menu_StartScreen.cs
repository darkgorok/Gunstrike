using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class Menu_StartScreen : MonoBehaviour
{
    public Text worldTxt;

    private IProgressService progressService;

    [Inject]
    public void Construct(IProgressService progressService)
    {
        this.progressService = progressService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        if (progressService.LevelPlaying == -1)
        {
            worldTxt.text = "TEST GAMEPLAY";
        }
        else
        {
            worldTxt.text = "LEVEL: " + progressService.LevelPlaying;
        }
    }
}
