using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class PlatformControllerSwitcher : MonoBehaviour, ICanTakeDamage
{
    public AudioClip sound;
    public PlatformControllerNEW targetControl;
    public Animator anim;

    bool isStop = true;
    private IAudioService audioService;

    [Inject]
    private void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    void Awake()
    {
        ProjectScope.Inject(this);
        targetControl.isManualControl = true;
        targetControl.isStop = isStop;
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        isStop = !isStop;
        targetControl.isStop = isStop;
        anim.SetBool("open", !isStop);
        audioService?.PlaySfx(sound);
    }
}
