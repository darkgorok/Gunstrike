using UnityEngine;
using VContainer;

public class ShakingHelper : MonoBehaviour
{
    public bool playOnStart = false;
    public AudioClip sound;
    public float shakeDecay = 0.02f;
    public float shakeIntensity = 0.2f;
    public float wide = 0.2f;
    public GameObject Target;
    public float timeShake = 1;
    public float timeRate = 1;

    private bool shaking;
    private bool isLoop;
    private float currentShakeDecay;
    private float currentShakeIntensity;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private IAudioService audioService;

    [Inject]
    public void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        if (Target == null)
            Target = gameObject;

        shaking = false;
    }

    public virtual void Start()
    {
        if (playOnStart)
            DoShake();
    }

    public void DoShake(bool loop = false)
    {
        if (shaking)
            return;

        isLoop = loop;
        audioService?.PlaySfx(sound);
        originalPos = Target.transform.position;
        originalRot = Target.transform.rotation;
        currentShakeIntensity = shakeIntensity;
        currentShakeDecay = shakeDecay;
        shaking = true;
    }

    public void StopShake()
    {
        shaking = false;
        isLoop = false;
    }

    private void Update()
    {
        if (currentShakeIntensity > 0)
        {
            Target.transform.position = originalPos + Random.insideUnitSphere * currentShakeIntensity;
            Target.transform.rotation = new Quaternion(
                originalRot.x + Random.Range(-currentShakeIntensity, currentShakeIntensity) * wide,
                originalRot.y + Random.Range(-currentShakeIntensity, currentShakeIntensity) * wide,
                originalRot.z + Random.Range(-currentShakeIntensity, currentShakeIntensity) * wide,
                originalRot.w + Random.Range(-currentShakeIntensity, currentShakeIntensity) * wide);

            currentShakeIntensity -= currentShakeDecay;
        }
        else if (shaking)
        {
            if (isLoop)
            {
                currentShakeIntensity = shakeIntensity;
                currentShakeDecay = shakeDecay;
            }
            else
            {
                shaking = false;
            }
        }
    }

    private void OnDisable()
    {
        StopShake();
    }
}
