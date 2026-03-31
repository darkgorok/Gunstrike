using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

public class WorldChooseUI : MonoBehaviour
{
    public Scrollbar scrollbar;
    public float smooth = 1000;

    private IProgressService progressService;
    private ILevelSelectionState levelSelectionState;
    private IMainMenuSceneService mainMenuSceneService;

    [Inject]
    public void Construct(IProgressService progressService, ILevelSelectionState levelSelectionState, IMainMenuSceneService mainMenuSceneService)
    {
        this.progressService = progressService;
        this.levelSelectionState = levelSelectionState;
        this.mainMenuSceneService = mainMenuSceneService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private IEnumerator Start()
    {
        yield return null;

        Transform target = levelSelectionState.CurrentHighestLevelTransform;
        if (target == null)
            yield break;

        if (scrollbar == null)
        {
            Debug.LogError("WorldChooseUI: Scrollbar is not assigned.", this);
            yield break;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("WorldChooseUI: MainCamera not found (Camera.main is null).", this);
            yield break;
        }

        float playerPosX = target.position.x;
        float limitPosX = cam.ScreenToWorldPoint(new Vector3(Screen.width / 2f, 0f, 0f)).x;

        while (levelSelectionState.CurrentHighestLevelTransform != null && playerPosX > limitPosX)
        {
            scrollbar.value = Mathf.Clamp01(scrollbar.value + Time.deltaTime);

            playerPosX = levelSelectionState.CurrentHighestLevelTransform.position.x;
            limitPosX = cam.ScreenToWorldPoint(new Vector3(Screen.width / 2f, 0f, 0f)).x;

            yield return null;
        }
    }

    public void Back_performed()
    {
        mainMenuSceneService.OpenStartMenu();
    }

    private void OnEnable()
    {
        int numberOfLevels = FindObjectsOfType<MainMenu_Level>().Length;
        progressService.TotalLevel = numberOfLevels;
        Debug.Log("TOTAL LEVELS = " + numberOfLevels);
    }
}
