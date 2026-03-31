using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class OpenScene : MonoBehaviour
{
    public string sceneName = "scene name";

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

    public void Open()
    {
        progressService.LevelPlaying = -1;
        MainMenuHomeScene.Instance.LoadScene(sceneName);
    }
}
