using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class CollectGunItem : MonoBehaviour, ICanCollect
{
    public GunTypeID gunTypeID;
    public AudioClip soundCollect;

    private IAudioService audioService;
    private IGunRuntimeService gunRuntimeService;

    [Inject]
    public void Construct(IAudioService audioService, IGunRuntimeService gunRuntimeService)
    {
        this.audioService = audioService;
        this.gunRuntimeService = gunRuntimeService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    public void Collect()
    {
        audioService.PlaySfx(soundCollect);
        gunRuntimeService.SetNewGunDuringGameplay(gunTypeID);
        Destroy(gameObject);
    }
}
