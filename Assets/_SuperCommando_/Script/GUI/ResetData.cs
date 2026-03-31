using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using VContainer;

public class ResetData : MonoBehaviour
{
    private ISceneLoader sceneLoader;
    private IProgressService progressService;
    private ICharacterSelectionService characterSelectionService;
    private IAudioService audioService;

    [Inject]
    public void Construct(
        ISceneLoader sceneLoader,
        IProgressService progressService,
        ICharacterSelectionService characterSelectionService,
        IAudioService audioService)
    {
        this.sceneLoader = sceneLoader;
        this.progressService = progressService;
        this.characterSelectionService = characterSelectionService;
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    void Start()
    {
        characterSelectionService.RefreshSelectedCharacter();
    }

    public void Reset()
    {
        progressService.ResetAllPreservingRemoveAds();
        sceneLoader.LoadImmediateAsync(SceneManager.GetActiveScene().buildIndex);
        audioService.PlaySfx(audioService.ClickClip);
    }

    public void UnlockAll()
    {
        progressService.UnlockAllLevels();
        sceneLoader.LoadImmediateAsync(SceneManager.GetActiveScene().buildIndex);
        audioService.PlaySfx(audioService.ClickClip);
    }
}
