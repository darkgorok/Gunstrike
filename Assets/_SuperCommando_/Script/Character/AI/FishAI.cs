using UnityEngine;
using VContainer;

public class FishAI : MonoBehaviour, ICanTakeDamage
{
    public GameObject DestroyEffect;
    public AudioClip deadSound;
    [Range(0, 1)] public float deadSoundVolume = 0.5f;

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
        if (DestroyEffect != null)
            Instantiate(DestroyEffect, transform.position, transform.rotation);

        audioService.PlaySfx(deadSound, deadSoundVolume);

        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
            spawnItem.SpawnItem();

        Destroy(gameObject);
    }
}
