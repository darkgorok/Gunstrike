using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class CollectGunItem : MonoBehaviour, ICanCollect
{
    public GunTypeID gunTypeID;
    public AudioClip soundCollect;

    private IAudioService audioService;

    [Inject]
    public void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    public void Collect()
    {
        audioService.PlaySfx(soundCollect);
        GunManager.Instance.SetNewGunDuringGameplay(gunTypeID);
        Destroy(gameObject);
    }
}
