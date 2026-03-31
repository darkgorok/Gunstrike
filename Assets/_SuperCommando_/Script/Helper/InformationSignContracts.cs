using UnityEngine;

public interface IInformationSignView
{
    void ApplyPlatformTutorialState();
    void Show();
    void Hide();
    void HideImmediate();
}

public interface IInformationSignItemSpawner
{
    ItemType CurrentItemAvailable { get; }
    bool IsSpawning { get; }

    void StartSpawning();
    void StopSpawning();
}

public interface IInformationSignButtonHint
{
    void SetEnabled(bool enabled);
}

public readonly struct InformationSignViewConfig
{
    public InformationSignViewConfig(
        SpriteRenderer signSpriteRenderer,
        GameObject infoContainer,
        GameObject mobileTutorial,
        GameObject desktopTutorial,
        float fadeDuration)
    {
        SignSpriteRenderer = signSpriteRenderer;
        InfoContainer = infoContainer;
        MobileTutorial = mobileTutorial;
        DesktopTutorial = desktopTutorial;
        FadeDuration = fadeDuration;
    }

    public SpriteRenderer SignSpriteRenderer { get; }
    public GameObject InfoContainer { get; }
    public GameObject MobileTutorial { get; }
    public GameObject DesktopTutorial { get; }
    public float FadeDuration { get; }
}

public readonly struct InformationSignSpawnConfig
{
    public InformationSignSpawnConfig(
        bool shouldSpawnItem,
        ItemType helperItemPrefab,
        float spawnInterval,
        Vector2 spawnOffset,
        Vector2 spawnForce)
    {
        ShouldSpawnItem = shouldSpawnItem;
        HelperItemPrefab = helperItemPrefab;
        SpawnInterval = spawnInterval;
        SpawnOffset = spawnOffset;
        SpawnForce = spawnForce;
    }

    public bool ShouldSpawnItem { get; }
    public ItemType HelperItemPrefab { get; }
    public float SpawnInterval { get; }
    public Vector2 SpawnOffset { get; }
    public Vector2 SpawnForce { get; }
}
