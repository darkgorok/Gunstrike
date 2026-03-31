using UnityEngine;
using System.Collections;
using VContainer;

public class Bridge : MonoBehaviour, IStandOnEvent
{
    public enum RespawnType { PlayerDead, AfterTime }

    public RespawnType respawnType;
    public float delayRespawn = 1;
    public float delayFalling = 0.5f;
    public AudioClip soundBridge;

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
        oriPos = transform.position;
    }

    void Work()
    {
        if (isWorking)
            return;

        isWorking = true;

        audioService?.PlaySfx(soundBridge);
        GetComponent<Animator>().SetTrigger("Shake");
        StartCoroutine(Falling(delayFalling));
    }

    IEnumerator Falling(float time)
    {
        yield return new WaitForSeconds(time);
        GetComponent<Rigidbody2D>().isKinematic = false;
        GetComponent<BoxCollider2D>().enabled = false;

        if (respawnType == RespawnType.AfterTime)
            Invoke(nameof(RespawnPos), delayRespawn);
    }

    void RespawnPos()
    {
        transform.position = oriPos;
        transform.rotation = Quaternion.identity;
        GetComponent<BoxCollider2D>().enabled = true;
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        isWorking = false;
        GetComponent<Animator>().SetTrigger("reset");
    }

    public void StandOnEvent(GameObject instigator)
    {
        Work();
    }
}
