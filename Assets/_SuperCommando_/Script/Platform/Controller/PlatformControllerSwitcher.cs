using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformControllerSwitcher : MonoBehaviour, ICanTakeDamage
{
    public AudioClip sound;
    public PlatformControllerNEW targetControl;
    bool isStop = true;

    public Animator anim;

    void Awake()
    {
        targetControl.isManualControl = true;
        targetControl.isStop = isStop;
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        isStop = !isStop;
        targetControl.isStop = isStop;
        anim.SetBool("open", !isStop);
        SoundManager.PlaySfx(sound);
    }
}
