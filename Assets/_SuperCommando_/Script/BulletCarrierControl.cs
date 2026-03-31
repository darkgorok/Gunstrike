using UnityEngine;
using VContainer;

public class BulletCarrierControl : MonoBehaviour, ICanTakeDamage
{
    public float speed = 2;
    public GameObject destroyObj;
    public GameObject[] dropBullet;
    public AudioClip soundDestroy;

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

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (destroyObj)
            Instantiate(destroyObj, transform.position, Quaternion.identity);

        Instantiate(dropBullet[Random.Range(0, dropBullet.Length)], transform.position, Quaternion.identity);
        audioService?.PlaySfx(soundDestroy);
        Destroy(gameObject);
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }
}
