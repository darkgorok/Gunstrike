using UnityEngine;
using VContainer;

public class AutoSpawn : MonoBehaviour
{
    public GameObject[] SpawnObjects;
    public int maxItemsSpawned = 7;
    [Tooltip("start spawn item when enable or wait for command message")]
    public bool autoSpawn = false;

    public float TimeMin;
    public float TimeMax;

    public AudioClip spawnSound;
    [Range(0, 1)]
    public float spawnSoundVolume = 0.5f;

    private int counter;
    private bool isSpawning;
    private float nextSpawnTime = -1f;
    private IGameSessionService gameSession;
    private IAudioService audioService;

    [Inject]
    public void Construct(IGameSessionService gameSession, IAudioService audioService)
    {
        this.gameSession = gameSession;
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        counter = 0;
        if (autoSpawn)
            Play();
    }

    private void Update()
    {
        if (!isSpawning || nextSpawnTime < 0f || Time.time < nextSpawnTime)
            return;

        if (gameSession == null || gameSession.State != GameManager.GameState.Playing)
        {
            ScheduleNextSpawn();
            return;
        }

        SpawnEnemy();
    }

    public void Play()
    {
        if (SpawnObjects == null || SpawnObjects.Length == 0)
            return;

        isSpawning = true;
        ScheduleNextSpawn();
    }

    private void SpawnEnemy()
    {
        audioService?.PlaySfx(spawnSound, spawnSoundVolume);
        Instantiate(SpawnObjects[Random.Range(0, SpawnObjects.Length)], transform.position, Quaternion.identity);
        counter++;

        if (maxItemsSpawned > 0 && counter >= maxItemsSpawned)
        {
            isSpawning = false;
            nextSpawnTime = -1f;
            return;
        }

        ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(TimeMin, TimeMax);
    }
}
