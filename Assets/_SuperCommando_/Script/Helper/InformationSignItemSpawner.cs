using System.Collections;
using UnityEngine;

public sealed class InformationSignItemSpawner : IInformationSignItemSpawner
{
    private readonly MonoBehaviour runner;
    private readonly Transform spawnOrigin;
    private readonly bool shouldSpawnItem;
    private readonly ItemType helperItemPrefab;
    private readonly float spawnInterval;
    private readonly Vector2 spawnOffset;
    private readonly Vector2 spawnForce;

    private Coroutine spawnRoutine;

    public ItemType CurrentItemAvailable { get; private set; }
    public bool IsSpawning { get; private set; }

    public InformationSignItemSpawner(MonoBehaviour runner, Transform spawnOrigin, InformationSignSpawnConfig config)
    {
        this.runner = runner;
        this.spawnOrigin = spawnOrigin;
        shouldSpawnItem = config.ShouldSpawnItem;
        helperItemPrefab = config.HelperItemPrefab;
        spawnInterval = config.SpawnInterval;
        spawnOffset = config.SpawnOffset;
        spawnForce = config.SpawnForce;
    }

    public void StartSpawning()
    {
        if (!CanSpawnHelpers() || spawnRoutine != null)
            return;

        IsSpawning = true;
        spawnRoutine = runner.StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        IsSpawning = false;

        if (spawnRoutine == null)
            return;

        runner.StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    private IEnumerator SpawnLoop()
    {
        var delay = new WaitForSeconds(spawnInterval);

        while (IsSpawning)
        {
            SpawnHelperIfNeeded();
            yield return delay;
        }

        spawnRoutine = null;
    }

    private bool CanSpawnHelpers()
    {
        return shouldSpawnItem && helperItemPrefab != null;
    }

    private void SpawnHelperIfNeeded()
    {
        if (CurrentItemAvailable != null && CurrentItemAvailable.gameObject.activeInHierarchy)
            return;

        CurrentItemAvailable = Object.Instantiate(
            helperItemPrefab,
            spawnOrigin.position + (Vector3)spawnOffset,
            Quaternion.identity);

        CurrentItemAvailable.Init(true, spawnForce);
    }
}
