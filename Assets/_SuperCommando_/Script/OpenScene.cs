using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class OpenScene : MonoBehaviour
{
    public string sceneName = "scene name";

    private IProgressService progressService;
    private IMainMenuSceneService mainMenuSceneService;

    [Inject]
    public void Construct(IProgressService progressService, IMainMenuSceneService mainMenuSceneService)
    {
        this.progressService = progressService;
        this.mainMenuSceneService = mainMenuSceneService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    public void Open()
    {
        progressService.LevelPlaying = -1;
        mainMenuSceneService.LoadScene(sceneName);
    }
}
