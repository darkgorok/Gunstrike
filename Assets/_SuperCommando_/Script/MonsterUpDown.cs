using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class MonsterUpDown : MonoBehaviour, ICanTakeDamage
{
    public GameObject destroyFX;
    public AudioClip deadSound;

    private IAudioService audioService;

    [Inject]
    private void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (destroyFX)
            Instantiate(destroyFX, transform.position, Quaternion.identity);

        audioService?.PlaySfx(deadSound);

        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
            spawnItem.SpawnItem();

        Destroy(gameObject);
    }
}
