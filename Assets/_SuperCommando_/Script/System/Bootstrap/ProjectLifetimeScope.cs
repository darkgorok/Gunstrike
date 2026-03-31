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
        builder.Register<GameSessionRuntimeState>(Lifetime.Singleton).As<IGameSessionRuntimeState>();
        builder.Register<SceneListenerBroadcastService>(Lifetime.Singleton).As<IListenerBroadcastService>();
        builder.Register<SceneLoaderService>(Lifetime.Singleton).As<ISceneLoader>();
        builder.Register<LegacyAudioService>(Lifetime.Singleton).As<IAudioService>();
        builder.Register<LegacyAdsService>(Lifetime.Singleton).As<IAdsService>();
        builder.Register<LegacyControllerInputService>(Lifetime.Singleton).As<IControllerInputService>();
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
