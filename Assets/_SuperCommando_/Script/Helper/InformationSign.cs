using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class InformationSign : MonoBehaviour
{
    public enum TutorialButton
    {
        None,
        Jump,
        Weapon,
        Dash
    }

    [Header("Behavior")]
    [FormerlySerializedAs("activeButton")]
    [SerializeField] private TutorialButton buttonToEnable = TutorialButton.None;
    [FormerlySerializedAs("sound")]
    [SerializeField] private AudioClip enterSound;

    [Header("View")]
    [FormerlySerializedAs("spriteRenderer")]
    [SerializeField] private SpriteRenderer signSpriteRenderer;
    [FormerlySerializedAs("inforContainer")]
    [SerializeField] private GameObject infoContainer;
    [FormerlySerializedAs("mobileTut")]
    [SerializeField] private GameObject mobileTutorial;
    [FormerlySerializedAs("windowsTut")]
    [SerializeField] private GameObject desktopTutorial;
    [SerializeField] private float fadeDuration = 0.2f;

    [Header("Spawn Helper")]
    [FormerlySerializedAs("spawnItem")]
    [SerializeField] private bool shouldSpawnItem;
    [FormerlySerializedAs("item")]
    [SerializeField] private ItemType helperItemPrefab;
    [FormerlySerializedAs("spawnDelay")]
    [Min(0.05f)]
    [SerializeField] private float spawnInterval = 2f;
    [FormerlySerializedAs("localOffset")]
    [SerializeField] private Vector2 spawnOffset = new Vector2(0f, 0.5f);
    [FormerlySerializedAs("forceSpawn")]
    [SerializeField] private Vector2 spawnForce = new Vector2(0f, 3f);

    private bool triggered;

    public ItemType CurrentItemAvailable => itemSpawner?.CurrentItemAvailable;
    public bool IsSpawning => itemSpawner != null && itemSpawner.IsSpawning;

    private IAudioService audioService;
    private IControllerInputService controllerInputService;
    private Collider2D triggerCollider;
    private IInformationSignView viewPresenter;
    private IInformationSignItemSpawner itemSpawner;
    private IInformationSignButtonHint buttonTogglePresenter;
    private bool missingAudioServiceLogged;

    [Inject]
    public void Construct(IAudioService audioService, IControllerInputService controllerInputService)
    {
        this.audioService = audioService;
        this.controllerInputService = controllerInputService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);

        viewPresenter = new InformationSignViewPresenter(
            this,
            new InformationSignViewConfig(
                signSpriteRenderer,
                infoContainer,
                mobileTutorial,
                desktopTutorial,
                fadeDuration));

        itemSpawner = new InformationSignItemSpawner(
            this,
            transform,
            new InformationSignSpawnConfig(
                shouldSpawnItem,
                helperItemPrefab,
                spawnInterval,
                spawnOffset,
                spawnForce));

        buttonTogglePresenter = new InformationSignButtonTogglePresenter(buttonToEnable, controllerInputService);

        viewPresenter.ApplyPlatformTutorialState();
        HideImmediate();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.TryGetComponent(out Player player)) return;

        triggered = true;
        Show();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!triggered || !other.TryGetComponent(out Player player)) return;

        triggered = false;
        Hide();
    }

    private void OnDisable()
    {
        HideImmediate();
    }

    private void Show()
    {
        PlayEnterSound();
        viewPresenter.Show();
        buttonTogglePresenter.SetEnabled(true);
        itemSpawner.StartSpawning();
    }

    private void Hide()
    {
        itemSpawner.StopSpawning();
        viewPresenter.Hide();
        buttonTogglePresenter.SetEnabled(false);
    }

    private void HideImmediate()
    {
        triggered = false;
        itemSpawner?.StopSpawning();
        viewPresenter?.HideImmediate();
        buttonTogglePresenter?.SetEnabled(false);
    }

    private void PlayEnterSound()
    {
        if (enterSound == null) return;

        audioService.PlaySfx(enterSound);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (triggerCollider == null) TryGetComponent(out triggerCollider);
    }
#endif
}
