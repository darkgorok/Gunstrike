using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectGunItem : MonoBehaviour, ICanCollect
{
    public GunTypeID gunTypeID;
    public AudioClip soundCollect;

    public void Collect()
    {
        SoundManager.PlaySfx(soundCollect);
        GunManager.Instance.SetNewGunDuringGameplay(gunTypeID);
        Destroy(gameObject);
    }
}
