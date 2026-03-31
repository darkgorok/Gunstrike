using UnityEngine;
using System.Collections;
using VContainer;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(Animator))]
public class Bridge : MonoBehaviour, IStandOnEvent
{
    public enum RespawnType { PlayerDead, AfterTime }

    public RespawnType respawnType;
    public float delayRespawn = 1;
    public float delayFalling = 0.5f;
    public AudioClip soundBridge;

    [SerializeField] private Animator cachedAnimator;
    [SerializeField] private Rigidbody2D cachedRigidbody;
    [SerializeField] private BoxCollider2D cachedCollider;

    bool isWorking = false;
    Vector3 oriPos;
    private IAudioService audioService;

    [Inject]
    private void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    void Start()
    {
        ProjectScope.Inject(this);
        if (cachedAnimator == null)
            cachedAnimator = GetComponent<Animator>();
        if (cachedRigidbody == null)
            cachedRigidbody = GetComponent<Rigidbody2D>();
        if (cachedCollider == null)
            cachedCollider = GetComponent<BoxCollider2D>();

        oriPos = transform.position;
    }

    void Work()
    {
        if (isWorking)
            return;

        isWorking = true;
        audioService?.PlaySfx(soundBridge);
        cachedAnimator.SetTrigger("Shake");
        StartCoroutine(Falling(delayFalling));
    }

    IEnumerator Falling(float time)
    {
        yield return new WaitForSeconds(time);
        cachedRigidbody.isKinematic = false;
        cachedCollider.enabled = false;

        if (respawnType == RespawnType.AfterTime)
            Invoke(nameof(RespawnPos), delayRespawn);
    }

    void RespawnPos()
    {
        transform.position = oriPos;
        transform.rotation = Quaternion.identity;
        cachedCollider.enabled = true;
        cachedRigidbody.isKinematic = true;
        cachedRigidbody.linearVelocity = Vector2.zero;
        isWorking = false;
        cachedAnimator.SetTrigger("reset");
    }

    public void StandOnEvent(GameObject instigator)
    {
        Work();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (cachedAnimator == null)
            TryGetComponent(out cachedAnimator);
        if (cachedRigidbody == null)
            TryGetComponent(out cachedRigidbody);
        if (cachedCollider == null)
            TryGetComponent(out cachedCollider);
    }
#endif
}
