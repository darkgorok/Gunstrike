using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public GameObject testLevelMap;

    private CameraFollow Camera;
    private ILevelCatalogService levelCatalogService;
    private IProgressService progressService;

    [Inject]
    public void Construct(ILevelCatalogService levelCatalogService, IProgressService progressService)
    {
        this.levelCatalogService = levelCatalogService;
        this.progressService = progressService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        Instance = this;

        if (FindObjectOfType<LevelMapType>())
        {
            Debug.LogError("Notice: There are a Level on this scene!");
            return;
        }

        if (DefaultValue.Instance)
        {
            GameObject levelMap = levelCatalogService.LoadLevelMap(progressService.LevelPlaying);
            if (levelMap != null)
                Instantiate(levelMap, Vector2.zero, Quaternion.identity);
        }
        else if (testLevelMap)
        {
            Instantiate(testLevelMap, Vector2.zero, Quaternion.identity);
        }
    }

    private void Start()
    {
        Camera = FindObjectOfType<CameraFollow>();
    }
}
