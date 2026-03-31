using UnityEngine;

public interface IAudioService
{
    bool IsSoundEnabled { get; set; }
    bool IsMusicEnabled { get; set; }

    float SoundVolume { get; set; }
    float MusicVolume { get; set; }
    float GameplayMusicVolume { get; }

    AudioClip MenuMusic { get; }
    AudioClip FinishPanelMusic { get; }
    AudioClip ClickClip { get; }
    AudioClip BeginMainMenuClip { get; }
    AudioClip GameFinishClip { get; }
    AudioClip GameOverClip { get; }
    AudioClip UpgradeClip { get; }
    AudioClip NotEnoughCoinClip { get; }
    AudioClip PurchasedClip { get; }
    AudioClip SwapGunClip { get; }

    void PlayClick();
    void PlaySfx(AudioClip clip, float volume = 1f);
    void PlaySfx(AudioClip[] clips, float volume = 1f);
    void PlayMusic(AudioClip clip, bool loop = true);
    void PlayMusic(AudioClip clip, float volume);
    void PlayMenuMusic();
    void PlayGameMusic();
    void ResetMusic();
    void PauseMusic(bool isPaused);
}
