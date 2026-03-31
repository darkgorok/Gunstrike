using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;
    //[Header("FORCE PLAY MUSIC")]
    //public AudioClip forcePlayMusic;

    [Header("MAIN MENU")]
    public AudioClip beginSoundInMainMenu;
    [Tooltip("Play music clip when start")]
    public AudioClip musicsMenu;
    [Range(0, 1)]
    public float musicMenuVolume = 0.5f;

    [Header("GAMEPLAY")]
    public AudioClip musicsGame;
    [Range(0, 1)]
    public float musicsGameVolume = 0.5f;

    public AudioClip musicFinishPanel;
    public AudioClip swapGun;
    [Header("Shop")]
    public AudioClip soundPurchased;
    public AudioClip soundUpgrade;
    public AudioClip soundNotEnoughCoin;

    [Tooltip("Default click sound used by the audio service and UI flows.")]
    public AudioClip soundClick;
    public AudioClip soundGamefinish;
    public AudioClip soundGameover;

    private AudioSource musicAudio;
    private AudioSource soundFx;

    //GET and SET
    public static float MusicVolume {
        set { Instance.musicAudio.volume = value; }
        get { return Instance.musicAudio.volume; }
    }
    public static float SoundVolume {
        set { Instance.soundFx.volume = value; }
        get { return Instance.soundFx.volume; }
    }

    public float CurrentMusicVolume
    {
        get => musicAudio != null ? musicAudio.volume : 0f;
        set
        {
            if (musicAudio != null)
                musicAudio.volume = value;
        }
    }

    public float CurrentSoundVolume
    {
        get => soundFx != null ? soundFx.volume : 0f;
        set
        {
            if (soundFx != null)
                soundFx.volume = value;
        }
    }

    public static void ResetMusic()
    {
        Instance.musicAudio.Stop();
        Instance.musicAudio.Play();
    }

    public void ResetMusicInstance()
    {
        if (musicAudio == null)
            return;

        musicAudio.Stop();
        musicAudio.Play();
    }

    public void PauseMusic(bool isPause)
    {
        if (isPause)
            Instance.musicAudio.mute = true;
        else
            Instance.musicAudio.mute = false;
    }

    public static void Click() {
        PlaySfx(Instance.soundClick, 1);
    }

    public void ClickInstance()
    {
        PlaySound(soundClick, soundFx, 1f);
    }

    void Awake() {
        Instance = this;
        musicAudio = gameObject.AddComponent<AudioSource>();
        musicAudio.loop = true;
        musicAudio.volume = 0.5f;
        soundFx = gameObject.AddComponent<AudioSource>();
    }

    public static void PlayGameMusic()
    {
        PlayMusic(Instance.musicsGame, Instance.musicsGameVolume);
    }

    public void PlayGameMusicInstance()
    {
        PlaySound(musicsGame, musicAudio, musicsGameVolume);
    }

	public static void PlaySfx(AudioClip clip){
		Instance.PlaySound(clip, Instance.soundFx);
	}

    public void PlaySfxInstance(AudioClip clip)
    {
        PlaySound(clip, soundFx);
    }

    public static void PlaySfx(AudioClip[] clips)
    {
        if (Instance != null && clips.Length > 0)
            Instance.PlaySound(clips[Random.Range(0, clips.Length)], Instance.soundFx);
    }

    public void PlaySfxInstance(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0)
            PlaySound(clips[Random.Range(0, clips.Length)], soundFx);
    }

    public static void PlaySfx(AudioClip[] clips, float volume)
    {
        if (Instance != null && clips.Length > 0)
            Instance.PlaySound(clips[Random.Range(0, clips.Length)], Instance.soundFx, volume);
    }

    public void PlaySfxInstance(AudioClip[] clips, float volume)
    {
        if (clips != null && clips.Length > 0)
            PlaySound(clips[Random.Range(0, clips.Length)], soundFx, volume);
    }

	public static void PlaySfx(AudioClip clip, float volume){
		Instance.PlaySound(clip, Instance.soundFx, volume);
	}

    public void PlaySfxInstance(AudioClip clip, float volume)
    {
        PlaySound(clip, soundFx, volume);
    }

	public static void PlayMusic(AudioClip clip, bool loop = true){
        if (Instance == null)
            return;

        Instance.musicAudio.loop = loop;
        Instance.PlaySound (clip, Instance.musicAudio);
	}

    public void PlayMusicInstance(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicAudio == null)
            return;

        musicAudio.loop = loop;
        PlaySound(clip, musicAudio);
    }

	public static void PlayMusic(AudioClip clip, float volume){
		Instance.PlaySound (clip, Instance.musicAudio, volume);
	}

    public void PlayMusicInstance(AudioClip clip, float volume)
    {
        PlaySound(clip, musicAudio, volume);
    }

	private void PlaySound(AudioClip clip,AudioSource audioOut){
		if (clip == null) {
//			Debug.Log ("There are no audio file to play", gameObject);
			return;
		}

		if (audioOut == musicAudio) {
			audioOut.clip = clip;
			audioOut.Play ();
		} else
			audioOut.PlayOneShot (clip, SoundVolume);
	}

	private void PlaySound(AudioClip clip,AudioSource audioOut, float volume){
		if (clip == null) {
//			Debug.Log ("There are no audio file to play", gameObject);
			return;
		}

		if (audioOut == musicAudio) {
			audioOut.clip = clip;
			audioOut.Play ();
		} else
			audioOut.PlayOneShot (clip, SoundVolume * volume);
	}


}
