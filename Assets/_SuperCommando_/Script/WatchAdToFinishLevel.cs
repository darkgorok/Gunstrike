using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class WatchAdToFinishLevel : MonoBehaviour
{
    public GameObject buttonVideo;

    private IAdsService adsService;
    private ILevelCatalogService levelCatalogService;
    private IProgressService progressService;
    private IMenuFlowService menuFlowService;

    [Inject]
    public void Construct(IAdsService adsService, ILevelCatalogService levelCatalogService, IProgressService progressService, IMenuFlowService menuFlowService)
    {
        this.adsService = adsService;
        this.levelCatalogService = levelCatalogService;
        this.progressService = progressService;
        this.menuFlowService = menuFlowService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void OnEnable()
    {
        GameObject nextLevelMap = levelCatalogService.LoadLevelMap(progressService.LevelPlaying + 1);
        if (nextLevelMap == null)
        {
            buttonVideo.SetActive(false);
            enabled = false;
        }
    }

    private void Update()
    {
        buttonVideo.SetActive(adsService.IsInitialized);
    }

    public void WatchAd()
    {
        adsService.ShowRewardedVideo(AdsManager_AdResult);
    }

    private void AdsManager_AdResult(bool isSuccess, int rewarded)
    {
        if (isSuccess)
            menuFlowService.LoadNextLevel();
    }
}
