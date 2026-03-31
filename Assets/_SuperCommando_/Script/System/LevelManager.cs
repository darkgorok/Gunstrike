using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public GameObject testLevelMap;

    [SerializeField] private CameraFollow cameraFollow;
    private ILevelCatalogService levelCatalogService;
    private IProgressService progressService;
    private IDefaultGameConfigService defaultGameConfigService;

    [Inject]
    public void Construct(ILevelCatalogService levelCatalogService, IProgressService progressService, IDefaultGameConfigService defaultGameConfigService)
    {
        this.levelCatalogService = levelCatalogService;
        this.progressService = progressService;
        this.defaultGameConfigService = defaultGameConfigService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        Instance = this;

        if (Object.FindFirstObjectByType<LevelMapType>() != null)
        {
            Debug.LogError("Notice: There are a Level on this scene!");
            return;
        }

        if (defaultGameConfigService.HasDefaults)
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
        if (cameraFollow == null)
            cameraFollow = Object.FindFirstObjectByType<CameraFollow>();
    }
}
