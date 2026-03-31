using System.Collections;
using UnityEngine;
using VContainer;

public class DestroyPlatform : MonoBehaviour, IStandOnEvent
{
    public Sprite crackedImage;
    public float timeLive = 2;
    public GameObject destroyFX;
    public GameObject smokeFX;
    public AudioClip contactSound;
    public float goBackIn = 3f;

    private bool isWorking;
    private Sprite oriSprite;
    private SpriteRenderer spriteRen;
    private Collider2D col;
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
        spriteRen = GetComponent<SpriteRenderer>();
        oriSprite = spriteRen.sprite;
        col = GetComponent<Collider2D>();
    }

    private IEnumerator WorkCo()
    {
        isWorking = true;
        audioService?.PlaySfx(contactSound);
        spriteRen.sprite = crackedImage;

        if (smokeFX)
            SpawnSystemHelper.GetNextObject(smokeFX, true).transform.position = transform.position;

        yield return new WaitForSeconds(timeLive);
        Instantiate(destroyFX, transform.position, Quaternion.identity);
        spriteRen.enabled = false;
        col.enabled = false;
        Invoke(nameof(GoBack), goBackIn);
    }

    private void GoBack()
    {
        spriteRen.sprite = oriSprite;
        spriteRen.enabled = true;
        col.enabled = true;
        isWorking = false;
    }

    public void StandOnEvent(GameObject instigator)
    {
        if (!isWorking)
            StartCoroutine(WorkCo());
    }
}
