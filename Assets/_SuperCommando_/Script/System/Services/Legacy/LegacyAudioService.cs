using UnityEngine;

public sealed class LegacyAudioService : IAudioService
{
    private readonly IProgressService progressService;
    private SoundManager cachedSoundManager;

    public LegacyAudioService(IProgressService progressService)
    {
        this.progressService = progressService;
    }

    private SoundManager Current
    {
        get
        {
            if (cachedSoundManager == null)
                cachedSoundManager = Object.FindFirstObjectByType<SoundManager>();

            return cachedSoundManager;
        }
    }

    public bool IsSoundEnabled
    {
        get => progressService.IsSoundEnabled;
        set
        {
            progressService.IsSoundEnabled = value;
            SoundVolume = value ? 1f : 0f;
        }
    }

    public bool IsMusicEnabled
    {
        get => progressService.IsMusicEnabled;
        set
        {
            progressService.IsMusicEnabled = value;
            MusicVolume = value ? GameplayMusicVolume : 0f;
        }
    }

    public float SoundVolume
    {
        get => Current != null ? Current.CurrentSoundVolume : 0f;
        set
        {
            if (Current != null)
                Current.CurrentSoundVolume = value;
        }
    }

    public float MusicVolume
    {
        get => Current != null ? Current.CurrentMusicVolume : 0f;
        set
        {
            if (Current != null)
                Current.CurrentMusicVolume = value;
        }
    }

    public float GameplayMusicVolume => Current != null ? Current.musicsGameVolume : 0.5f;

    public AudioClip MenuMusic => Current != null ? Current.musicsMenu : null;
    public AudioClip FinishPanelMusic => Current != null ? Current.musicFinishPanel : null;
    public AudioClip ClickClip => Current != null ? Current.soundClick : null;
    public AudioClip BeginMainMenuClip => Current != null ? Current.beginSoundInMainMenu : null;
    public AudioClip GameFinishClip => Current != null ? Current.soundGamefinish : null;
    public AudioClip GameOverClip => Current != null ? Current.soundGameover : null;
    public AudioClip UpgradeClip => Current != null ? Current.soundUpgrade : null;
    public AudioClip NotEnoughCoinClip => Current != null ? Current.soundNotEnoughCoin : null;
    public AudioClip PurchasedClip => Current != null ? Current.soundPurchased : null;
    public AudioClip SwapGunClip => Current != null ? Current.swapGun : null;

    public void PlayClick()
    {
        if (Current != null)
            Current.ClickInstance();
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null || Current == null)
            return;

        if (Mathf.Approximately(volume, 1f))
            Current.PlaySfxInstance(clip);
        else
            Current.PlaySfxInstance(clip, volume);
    }

    public void PlaySfx(AudioClip[] clips, float volume = 1f)
    {
        if (clips == null || clips.Length == 0 || Current == null)
            return;

        if (Mathf.Approximately(volume, 1f))
            Current.PlaySfxInstance(clips);
        else
            Current.PlaySfxInstance(clips, volume);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || Current == null)
            return;

        Current.PlayMusicInstance(clip, loop);
    }

    public void PlayMusic(AudioClip clip, float volume)
    {
        if (clip == null || Current == null)
            return;

        Current.PlayMusicInstance(clip, volume);
    }

    public void PlayMenuMusic()
    {
        if (Current != null && MenuMusic != null)
            Current.PlayMusicInstance(MenuMusic);
    }

    public void PlayGameMusic()
    {
        if (Current != null)
            Current.PlayGameMusicInstance();
    }

    public void ResetMusic()
    {
        if (Current != null)
            Current.ResetMusicInstance();
    }

    public void PauseMusic(bool isPaused)
    {
        if (Current != null)
            Current.PauseMusic(isPaused);
    }
}
