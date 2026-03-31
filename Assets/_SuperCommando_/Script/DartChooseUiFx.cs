using UnityEngine;
using VContainer;

public class DartChooseUiFx : MonoBehaviour
{
    public float callFunctionDelay = 0.15f;
    public GameObject hitFX;
    public AudioClip startSound, hitSound;
    public bool shakeCamera = true;

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

    private void Start()
    {
        audioService?.PlaySfx(startSound, 0.6f);
    }

    public void Hit()
    {
        audioService?.PlaySfx(hitSound, 0.6f);
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
