using System;
using UnityEngine;

[Serializable]
public struct BossDeathSequenceSettings
{
    public float IntroDelay;
    public int FirstWaveExplosions;
    public int SecondWaveExplosions;
    public float ExplosionInterval;
    public float BlackScreenShowDuration;
    public float BlackScreenHideDuration;
    public float EarthQuakeTime;
    public float EarthQuakeSpeed;
    public float EarthQuakeSize;
    public GameObject ExplosionPrefab;
    public Vector2 ExplosionArea;
    public AudioClip DeathSound;
    public AudioClip ExplosionSound;
}

public sealed class BossDeathSequence
{
    private enum Phase
    {
        Idle,
        IntroDelay,
        FirstWave,
        SecondWave,
        Completed
    }

    private readonly IAudioService audioService;
    private readonly IControllerInputService controllerInputService;
    private readonly IGameplayPresentationService presentationService;
    private readonly IGameSessionService gameSession;
    private readonly Transform bossTransform;
    private readonly Action onCompleted;

    private BossDeathSequenceSettings settings;
    private Phase phase = Phase.Idle;
    private float timer;
    private int explosionsRemaining;

    public bool IsRunning => phase != Phase.Idle && phase != Phase.Completed;

    public BossDeathSequence(
        IAudioService audioService,
        IControllerInputService controllerInputService,
        IGameplayPresentationService presentationService,
        IGameSessionService gameSession,
        Transform bossTransform,
        Action onCompleted)
    {
        this.audioService = audioService;
        this.controllerInputService = controllerInputService;
        this.presentationService = presentationService;
        this.gameSession = gameSession;
        this.bossTransform = bossTransform;
        this.onCompleted = onCompleted;
    }

    public void Begin(BossDeathSequenceSettings settings)
    {
        this.settings = settings;
        phase = Phase.IntroDelay;
        timer = settings.IntroDelay;

        audioService.PauseMusic(true);
        gameSession.MissionStarCollected = 3;
        controllerInputService.StopMove();
        presentationService.SetControllerVisible(false);
        presentationService.SetGameplayUiVisible(false);
        audioService.PlaySfx(settings.DeathSound);
    }

    public void Tick(float deltaTime)
    {
        if (phase == Phase.Idle || phase == Phase.Completed)
            return;

        timer -= deltaTime;
        if (timer > 0f)
            return;

        switch (phase)
        {
            case Phase.IntroDelay:
                StartFirstWave();
                break;
            case Phase.FirstWave:
                TickWave(Phase.SecondWave, settings.SecondWaveExplosions, true);
                break;
            case Phase.SecondWave:
                TickWave(Phase.Completed, 0, false);
                break;
        }
    }

    private void StartFirstWave()
    {
        phase = Phase.FirstWave;
        explosionsRemaining = settings.FirstWaveExplosions;
        SpawnExplosion();
    }

    private void TickWave(Phase nextPhase, int nextWaveCount, bool showBlackScreenBeforeNextWave)
    {
        explosionsRemaining--;
        if (explosionsRemaining > 0)
        {
            SpawnExplosion();
            return;
        }

        if (showBlackScreenBeforeNextWave)
        {
            presentationService.ShowBlackScreen(settings.BlackScreenShowDuration, Color.white);
            phase = nextPhase;
            explosionsRemaining = nextWaveCount;
            SpawnExplosion();
            return;
        }

        presentationService.HideBlackScreen(settings.BlackScreenHideDuration);
        phase = Phase.Completed;
        gameSession.GameFinish(1);
        onCompleted?.Invoke();
    }

    private void SpawnExplosion()
    {
        if (settings.ExplosionPrefab != null)
        {
            var spawnOffset = new Vector3(
                UnityEngine.Random.Range(-settings.ExplosionArea.x, settings.ExplosionArea.x),
                UnityEngine.Random.Range(0, settings.ExplosionArea.y),
                0f);
            UnityEngine.Object.Instantiate(settings.ExplosionPrefab, bossTransform.position + spawnOffset, Quaternion.identity);
        }

        audioService.PlaySfx(settings.ExplosionSound);
        CameraPlay.EarthQuakeShake(settings.EarthQuakeTime, settings.EarthQuakeSpeed, settings.EarthQuakeSize);
        timer = settings.ExplosionInterval;
    }
}
