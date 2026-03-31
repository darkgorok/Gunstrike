
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DartChooseUiFx : MonoBehaviour
{
    public float callFunctionDelay = 0.15f;
    public GameObject hitFX;
    public AudioClip startSound, hitSound;
    public bool shakeCamera = true;

    private void Start()
    {
        SoundManager.PlaySfx(startSound,0.6f);
    }

    //call by own animation event
    public void Hit()
    {
        SoundManager.PlaySfx(hitSound, 0.6f);
        if (hitFX)
            Instantiate(hitFX, transform.position, Quaternion.identity);
        CameraPlay.EarthQuakeShake(0.1f, 60, 1f);
        Destroy(gameObject, callFunctionDelay);
    }

    private void OnDisable()
    {
        DartButtonManager.Instance.CallButtonAction();
        Destroy(gameObject);
    }
}
