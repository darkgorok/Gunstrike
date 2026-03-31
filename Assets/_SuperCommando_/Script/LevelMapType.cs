using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LEVELTYPE { Normal, BossFight}
public enum CONTROLLER { PLATFORM, RUNNER}
public class LevelMapType : MonoBehaviour
{
    public static LevelMapType Instance;
    public LEVELTYPE levelType;
    public bool playerNoLimitLife = false;
    public CONTROLLER controllerType;

    [Header("FORCE LOAD SCENE WHEN PRESS NEXT BUTTON")]
    public bool forceLoadScene = false;
    public string sceneName = "story end game";

    private void Awake()
    {
        Instance = this;
    }
}
