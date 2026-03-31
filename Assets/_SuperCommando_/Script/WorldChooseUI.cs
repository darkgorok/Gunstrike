using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WorldChooseUI : MonoBehaviour
{
    public Scrollbar scrollbar;
    public float smooth = 1000;

    void Awake()
    {
    }

    private IEnumerator Start()
    {
        yield return null;

        // If no target - nothing to do (original logic preserved)
        if (GlobalValue.currentHighestLevelObj == null)
            yield break;

        // Guard: scrollbar must be assigned in Inspector
        if (scrollbar == null)
        {
            Debug.LogError("WorldChooseUI: Scrollbar is not assigned.", this);
            yield break;
        }

        // Guard: Camera.main can be null if no MainCamera tag / camera disabled
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("WorldChooseUI: MainCamera not found (Camera.main is null).", this);
            yield break;
        }

        // Compute initial positions
        var playerPosX = GlobalValue.currentHighestLevelObj.position.x;
        var limitPosX = cam.ScreenToWorldPoint(new Vector3(Screen.width / 2f, 0, 0)).x;

        // Safety: avoid infinite tight loop; also handle target disappearing mid-loop
        while (GlobalValue.currentHighestLevelObj != null && playerPosX > limitPosX)
        {
            scrollbar.value = Mathf.Clamp01(scrollbar.value + Time.deltaTime);

            playerPosX = GlobalValue.currentHighestLevelObj.position.x;
            limitPosX = cam.ScreenToWorldPoint(new Vector3(Screen.width / 2f, 0, 0)).x;

            yield return null;
        }
    }

    public void Back_performed()
    {
        MainMenuHomeScene.Instance.OpenStartMenu();
    }

    private void OnEnable()
    {
        int numberOfLevels = FindObjectsOfType<MainMenu_Level>().Length;
        GlobalValue.totalLevel = numberOfLevels;
        Debug.Log("TOTAL LEVELS = " + numberOfLevels);
    }
}
