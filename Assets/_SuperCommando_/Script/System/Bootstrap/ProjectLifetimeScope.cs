using UnityEngine;
using VContainer;
using VContainer.Unity;

public sealed class ProjectLifetimeScope : LifetimeScope
{
    public static ProjectLifetimeScope Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        base.Awake();

        ProjectScope.SetResolver(Container);
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<UnityPrefsKeyValueStore>(Lifetime.Singleton).As<IKeyValueStore>();
        builder.Register<ReflectionFirebaseAnalyticsService>(Lifetime.Singleton).As<IAnalyticsService>();
        builder.Register<ConsentService>(Lifetime.Singleton).As<IConsentService>();
        builder.Register<GameSessionRuntimeState>(Lifetime.Singleton).As<IGameSessionRuntimeState>();
        builder.Register<SceneListenerBroadcastService>(Lifetime.Singleton).As<IListenerBroadcastService>();
        builder.Register<SceneLoaderService>(Lifetime.Singleton).As<ISceneLoader>();
        builder.Register<LegacyAudioService>(Lifetime.Singleton).As<IAudioService>();
        builder.Register<LegacyAdsService>(Lifetime.Singleton).As<IAdsService>();
        builder.Register<LegacyControllerInputService>(Lifetime.Singleton).As<IControllerInputService>();
        builder.Register<LegacyCameraRigService>(Lifetime.Singleton).As<ICameraRigService>();
        builder.Register<LegacyMenuFlowService>(Lifetime.Singleton).As<IMenuFlowService>();
        builder.Register<LegacyKeyPresentationService>(Lifetime.Singleton).As<IKeyPresentationService>();
        builder.Register<LegacyDialogFlowService>(Lifetime.Singleton).As<IDialogFlowService>();
        builder.Register<LegacyMainMenuSceneService>(Lifetime.Singleton).As<IMainMenuSceneService>();
        builder.Register<LegacyTutorialOverlayService>(Lifetime.Singleton).As<ITutorialOverlayService>();
        builder.Register<LegacyFloatingTextService>(Lifetime.Singleton).As<IFloatingTextService>();
        builder.Register<LegacyBossHealthbarService>(Lifetime.Singleton).As<IBossHealthbarService>();
        builder.Register<LegacyGunRuntimeService>(Lifetime.Singleton).As<IGunRuntimeService>();
        builder.Register<LegacyKeyboardBindingService>(Lifetime.Singleton).As<IKeyboardBindingService>();
        builder.Register<LegacyLevelMapSettingsService>(Lifetime.Singleton).As<ILevelMapSettingsService>();
        builder.Register<LegacyDefaultGameConfigService>(Lifetime.Singleton).As<IDefaultGameConfigService>();
        builder.Register<LegacyGameplayPresentationService>(Lifetime.Singleton).As<IGameplayPresentationService>();
        builder.Register<LegacyLevelCatalogService>(Lifetime.Singleton).As<ILevelCatalogService>();
        builder.Register<LegacyPlayerProfileService>(Lifetime.Singleton).As<IPlayerProfileService>();
        builder.Register<LegacyProgressService>(Lifetime.Singleton).As<IProgressService>();
        builder.Register<LegacyInventoryService>(Lifetime.Singleton).As<IInventoryService>();
        builder.Register<LegacyUpgradeService>(Lifetime.Singleton).As<IUpgradeService>();
        builder.Register<LegacyLevelSelectionState>(Lifetime.Singleton).As<ILevelSelectionState>();
        builder.Register<LegacyCharacterSelectionService>(Lifetime.Singleton).As<ICharacterSelectionService>();
        builder.Register<LegacyGameSessionService>(Lifetime.Singleton).As<IGameSessionService>();
    }
}
